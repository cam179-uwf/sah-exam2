namespace CarController.Services;

public class CarService
{
    private const int TickDelayMilliseconds = 100;
    
    public bool IsConnected => _client.Connected;
    public bool IsStoppedMoving => _direction is Direction.Stopped;
    public event Action? OnLeftIrSensorDetected;
    public event Action? OnRightIrSensorDetected;
    
    private readonly IBluetoothClient _client;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private Direction _direction = Direction.Stopped;
    private bool _foundAck;
    
    public CarService(IBluetoothClient client) => _client = client;

    /// <summary>
    /// Attempts to connect to the desired device.
    /// </summary>
    public async Task Connect()
    {
        if (_client.Connected) return;
        
        await _client.Connect();
        
        // receive the Connect message that the ESP32 sends.
        SpinWait.SpinUntil(() => _client.DataAvailable, TimeSpan.FromSeconds(1));
        var buffer = new byte[256];
        var count = _client.Read(ref buffer, 0, buffer.Length);
        var connectMsg = Convert.ToBase64String(buffer[..count]);
        Console.WriteLine($"ESP32 says: {connectMsg}");

        // this listener loop should be run on a different thread
        // so we call Task.Run
        _ = Task.Run(ListenerLoop);
    }

    /// <summary>
    /// Disconnects from the device.
    /// </summary>
    public void Disconnect()
    {
        if (!_client.Connected) return;
     
        _client.Disconnect();
    }
    
    /// <summary>
    /// Changes the speed of the car to the specified value.<br/>
    /// </summary>
    public async Task ChangeSpeed(byte newSpeed)
    {
        await ObtainLock(async () =>
        {
            _client.Write([5, newSpeed]);
            await Task.Run(SpinWaitForAck);
        });
    }

    /// <summary>
    /// Commands the car to start turning to the left.
    /// </summary>
    public async Task TurnLeft()
    {
        await ObtainLock(async () =>
        {
            if (_direction is Direction.Left) return;
            
            _client.Write([3]);
            await Task.Run(SpinWaitForAck);
            
            _direction = Direction.Left;
        });
    }
    
    /// <summary>
    /// Commands the car to start turning to the right.
    /// </summary>
    public async Task TurnRight()
    {
        await ObtainLock(async () =>
        {
            if (_direction is Direction.Right) return;
            
            _client.Write([4]);
            await Task.Run(SpinWaitForAck);
            
            _direction = Direction.Right;
        });
    }
    
    /// <summary>
    /// Commands the car to start moving backwards.
    /// </summary>
    public async Task MoveBackwards()
    {
        await ObtainLock(async () =>
        {
            if (_direction is Direction.Backward) return;
        
            _client.Write([2]);
            await Task.Run(SpinWaitForAck);
        
            _direction = Direction.Backward;
        });
    }
    
    /// <summary>
    /// Commands the car to start moving forwards.
    /// </summary>
    public async Task MoveForwards()
    {
        await ObtainLock(async () =>
        {
            if (_direction is Direction.Forward) return;
        
            _client.Write([1]);
            await Task.Run(SpinWaitForAck);
        
            _direction = Direction.Forward;
        });
    }

    /// <summary>
    /// Commands the car to stop moving.
    /// </summary>
    public async Task StopMoving()
    {
        await ObtainLock(async () =>
        {
            if (_direction is Direction.Stopped) return;
        
            _client.Write([0]);
            await Task.Run(SpinWaitForAck);
        
            _direction = Direction.Stopped;
        });
    }
    
    /// <summary>
    /// This method has an internal loop that runs until the client disconnects.
    /// The loop constantly listens for incoming data from the ESP32.
    /// </summary>
    private async Task ListenerLoop()
    {
        while (_client.Connected)
        {
            await ObtainLock(() =>
            {
                ProcessIncomingStream();
                return Task.CompletedTask;
            });
            
            await Task.Delay(TickDelayMilliseconds);
        }
    }

    /// <summary>
    /// This method causes the calling thread to wait until an acknowledgement bit is sent from the ESP32.
    /// </summary>
    /// <exception cref="CarServiceException"></exception>
    private void SpinWaitForAck()
    {
        SpinWait.SpinUntil(() => _client.DataAvailable, TimeSpan.FromSeconds(10));

        if (_client.DataAvailable)
        {
            ProcessIncomingStream();

            if (!_foundAck)
            {
                throw new CarServiceException("The device sent data but the command was not acknowledged.");
            }
        }
        else
        {
            throw new CarServiceException("After 10 seconds an acknowledgement was not sent by the controller.");
        }
    }
    
    /// <summary>
    /// Obtains a semaphore lock for the code you want to run.
    /// </summary>
    /// <exception cref="CarServiceException"></exception>
    private async Task ObtainLock(Func<Task> func)
    {
        try
        {
            if (await _semaphore.WaitAsync(TimeSpan.FromSeconds(10)))
            {
                await func();
            }
            else
            {
                throw new CarServiceException("Unable to obtain lock within the time of 10 seconds.");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// Checks to see if there is anything ready to be read in and processed from the ESP32.
    /// </summary>
    private void ProcessIncomingStream()
    {
        var buffer = new byte[256];
        
        if (_client.DataAvailable)
        {
            var n = _client.Read(ref buffer, 0, buffer.Length);

            for (var i = 0; i < n; i++)
            {
                switch (buffer[i])
                {
                    case 1:
                        _foundAck = true;
                        break;
                    case 2:
                        OnLeftIrSensorDetected?.Invoke();
                        break;
                    case 3:
                        OnRightIrSensorDetected?.Invoke();
                        break;
                }
            }
        }
    }
    
    private enum Direction
    {
        Stopped,
        Forward,
        Backward,
        Left,
        Right
    }
}