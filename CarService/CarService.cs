using System.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Sockets;

namespace CarController.Services;

public class CarService : IDisposable
{
    private readonly BluetoothClient _client;
    private readonly NetworkStream? _stream;

    private Direction _direction = Direction.Stopped;
    
    public bool IsConnected => _client.Connected;
    public bool IsStoppedMoving => _direction is Direction.Stopped;

    public event Action? OnLeftIrSensorDetected;
    public event Action? OnRightIrSensorDetected;
    private bool _foundAck;
    private object _processIncomingStreamLock = new();
    
    public CarService(string deviceName)
    {
        _client = new BluetoothClient();

        var devices = _client.DiscoverDevices();

        var device = devices.FirstOrDefault(x => x.DeviceName == deviceName);
        if (device is null)
        {
            throw new BluetoothDeviceNotFoundException("No devices found.");
        }

        var ep = new BluetoothEndPoint(device.DeviceAddress, InTheHand.Net.Bluetooth.BluetoothService.SerialPort);
        _client.Connect(ep);

        if (!_client.Connected)
        {
            throw new BluetoothDeviceFailedToConnectException("Failed to connect.");
        }
        
        _stream = _client.GetStream();
        
        Console.WriteLine("Waiting for connected message from device.");
        SpinWait.SpinUntil(() =>
        {
            try
            {
                return _stream.DataAvailable;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }, TimeSpan.FromSeconds(1));
        
        var buffer = new byte[256];
        var count = _stream.Read(buffer, 0, buffer.Length);
        var line = Convert.ToString(buffer[..count]);
        
        Console.WriteLine(line);

        Task.Run(Tick);
    }

    public async Task ChangeSpeed(byte newSpeed)
    {
        _stream?.Write([5, newSpeed]);
        await Task.Run(SpinWaitForAck);
    }
    
    private void ProcessIncomingStream()
    {
        if (_stream is null) return;
        
        var buffer = new byte[256];
        
        if (_stream?.DataAvailable ?? false)
        {
            var n = _stream?.Read(buffer, 0, buffer.Length);

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

    private async Task Tick()
    {
        while (_client.Connected)
        {
            lock (_processIncomingStreamLock)
            {
                ProcessIncomingStream();
            }
            
            await Task.Delay(100);
        }
    }

    public async Task TurnLeft()
    {
        if (_direction is Direction.Left) return;
        
        Console.WriteLine("Turn left.");
        _stream?.Write([3]);
        await Task.Run(SpinWaitForAck);
        
        _direction = Direction.Left;
    }
    
    public async Task TurnRight()
    {
        if (_direction is Direction.Right) return;
        
        Console.WriteLine("Turn right.");
        _stream?.Write([4]);
        await Task.Run(SpinWaitForAck);
        
        _direction = Direction.Right;
    }
    
    public async Task MoveBackwards()
    {
        if (_direction is Direction.Backward) return;
        
        Console.WriteLine("Move backwards.");
        _stream?.Write([2]);
        await Task.Run(SpinWaitForAck);
        
        _direction = Direction.Backward;
    }
    
    public async Task MoveForwards()
    {
        if (_direction is Direction.Forward) return;
        
        Console.WriteLine("Move forwards.");
        _stream?.Write([1]);
        await Task.Run(SpinWaitForAck);
        
        _direction = Direction.Forward;
    }

    public async Task StopMoving()
    {
        if (_direction is Direction.Stopped) return;
        
        Console.WriteLine("Stop moving.");
        _stream?.Write([0]);
        await Task.Run(SpinWaitForAck);
        
        _direction = Direction.Stopped;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private void SpinWaitForAck()
    {
        if (_stream is null)
        {
            throw new CarServiceException("Cannot wait for an acknowledgement because the bluetooth stream is null.");
        }

        lock (_processIncomingStreamLock)
        {
            SpinWait.SpinUntil(() =>
            {
                try
                {
                    return _stream.DataAvailable;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }, TimeSpan.FromSeconds(10));

            if (_stream.DataAvailable)
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

public class BluetoothDeviceNotFoundException(string message) : Exception(message);
public class BluetoothDeviceFailedToConnectException(string message) : Exception(message);
public class CarServiceException(string message) : Exception(message);