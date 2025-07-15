namespace CarController.Services;

public class CarService
{
    private const int TickDelayMilliseconds = 100;
    
    private readonly IBluetoothClient _client;

    private Direction _direction = Direction.Stopped;
    
    public bool IsConnected => _client.Connected;
    public bool IsStoppedMoving => _direction is Direction.Stopped;

    public event Action? OnLeftIrSensorDetected;
    public event Action? OnRightIrSensorDetected;
    private bool _foundAck;
    private SemaphoreSlim _semaphoreSlim = new(1, 1);
    
    public CarService(IBluetoothClient client)
    {
        _client = client;
    }

    public async Task Connect()
    {
        if (_client.Connected) return;
        
        await _client.Connect();
        
        Console.WriteLine("Waiting for connected message from device.");
        SpinWait.SpinUntil(() => _client.DataAvailable, TimeSpan.FromSeconds(1));
        
        var buffer = new byte[256];
        var count = _client.Read(ref buffer, 0, buffer.Length);
        var line = Convert.ToBase64String(buffer[..count]);
        
        Console.WriteLine(line);

        _ = Task.Run(Tick);
    }

    public void Disconnect()
    {
        if (!_client.Connected) return;
     
        _client.Disconnect();
    }
    
    public async Task ChangeSpeed(byte newSpeed)
    {
        await ObtainLock(async () =>
        {
            _client.Write([5, newSpeed]);
            await Task.Run(SpinWaitForAck);
        });
    }

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
    
    private async Task Tick()
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
    
    private async Task ObtainLock(Func<Task> func)
    {
        try
        {
            if (await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(10)))
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
            _semaphoreSlim.Release();
        }
    }
    
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