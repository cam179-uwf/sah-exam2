using System.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Sockets;

namespace CarController.Services;

/// <summary>
/// This is a bluetooth client based on the bluetooth classic protocol.
/// </summary>
public class InTheHandBluetoothClient : IBluetoothClient
{
    public bool Connected => _client?.Connected ?? false;
    public bool DataAvailable => _stream?.DataAvailable ?? false;
    
    private BluetoothClient? _client;
    private NetworkStream? _stream;
    private readonly string _deviceName;

    public InTheHandBluetoothClient(string deviceName)
    {
        _deviceName = deviceName;
    }
    
    public Task Connect()
    {
        // initialize a new bluetooth client
        _client = new BluetoothClient();

        // discover devices
        var devices = _client.DiscoverDevices();

        // grab the device with our desired device name
        var device = devices.FirstOrDefault(x => x.DeviceName == _deviceName);
        
        // if we didn't get the desired device throw an error
        if (device is null)
        {
            throw new BluetoothDeviceNotFoundException($"Could not find the device {_deviceName}.");
        }

        // don't know tbh
        var ep = new BluetoothEndPoint(device.DeviceAddress, InTheHand.Net.Bluetooth.BluetoothService.SerialPort);
        
        // connect the client to this endpoint
        _client.Connect(ep);

        // if we didn't connect error out
        if (!_client.Connected)
        {
            throw new BluetoothDeviceFailedToConnectException($"Failed to connect to {_deviceName}.");
        }
        
        // if we did connect get the connection stream and cache it
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