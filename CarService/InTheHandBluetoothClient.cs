using System.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Sockets;

namespace CarController.Services;

public class InTheHandBluetoothClient : IBluetoothClient
{
    private BluetoothClient? _client;
    private NetworkStream? _stream;
    private readonly string _deviceName;

    public bool Connected => _client?.Connected ?? false;
    public bool DataAvailable => _stream?.DataAvailable ?? false;

    public InTheHandBluetoothClient(string deviceName)
    {
        _deviceName = deviceName;
    }
    
    public Task Connect()
    {
        _client = new BluetoothClient();

        var devices = _client.DiscoverDevices();

        var device = devices.FirstOrDefault(x => x.DeviceName == _deviceName);
        if (device is null)
        {
            throw new BluetoothDeviceNotFoundException($"Could not find the device {_deviceName}.");
        }

        var ep = new BluetoothEndPoint(device.DeviceAddress, InTheHand.Net.Bluetooth.BluetoothService.SerialPort);
        _client.Connect(ep);

        if (!_client.Connected)
        {
            throw new BluetoothDeviceFailedToConnectException($"Failed to connect to {_deviceName}.");
        }
        
        _stream = _client.GetStream();

        return Task.CompletedTask;
    }

    public void Disconnect()
    {
        _client?.Close();
    }

    public int Read(ref byte[] buffer, int offset, int count)
    {
        return _stream?.Read(buffer, offset, count) ?? 0;
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        _stream?.Write(buffer);
    }
}