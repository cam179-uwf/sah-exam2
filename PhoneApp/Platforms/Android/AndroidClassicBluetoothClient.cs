using Android.Bluetooth;
using Android.Content;
using CarController.Services;
using Java.Util;

namespace PhoneApp;

public class AndroidClassicBluetoothClient : IBluetoothClient
{
    private static readonly UUID? SppUuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
    
    private readonly string _deviceName;
    private Stream? _inputStream;
    private Stream? _outputStream;
    private BluetoothSocket? _socket;
    
    public bool Connected => _socket?.IsConnected ?? false;
    public bool DataAvailable => _inputStream?.IsDataAvailable() ?? false;

    public AndroidClassicBluetoothClient(string deviceName)
    {
        _deviceName = deviceName;
    } 
    
    /// <summary>
    /// Don't really understand this code perfectly.
    /// But I know it is getting a bluetooth device from
    /// our connected bluetooth devices and is setting up
    /// an input stream and output stream between us and
    /// the device.
    /// </summary>
    public async Task Connect()
    {
        var bluetoothManager = Android.App.Application.Context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var adapter = bluetoothManager?.Adapter;
        
        if (adapter is null || !adapter.IsEnabled)
        {
            throw new BluetoothNotAvailableException("Bluetooth is not available or not enabled.");
        }

        var device = adapter.BondedDevices?.FirstOrDefault(d => d.Name == _deviceName);
        
        if (device is null)
        {
            throw new BluetoothDeviceNotFoundException($"Device '{_deviceName}' not paired.");
        }

        _socket = device.CreateRfcommSocketToServiceRecord(SppUuid);
        await Task.Run(() => _socket?.Connect());

        _inputStream = _socket?.InputStream;
        _outputStream = _socket?.OutputStream;
    }

    /// <summary>
    /// Closes the socket connection.
    /// </summary>
    public void Disconnect()
    {
        _socket?.Close();
        _inputStream?.Close();
        _outputStream?.Close();
        
        _socket = null;
        _inputStream = null;
        _outputStream = null;
    }

    public int Read(ref byte[] buffer, int offset, int count)
    {
        return _inputStream?.Read(buffer, offset, count) ?? 0;
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        _outputStream?.Write(buffer);
    }
}