namespace CarController.Services;

/// <summary>
/// An interface to define a bluetooth client.
/// </summary>
public interface IBluetoothClient
{
    bool Connected { get; }
    bool DataAvailable { get; }
    Task Connect();
    void Disconnect();
    int Read(ref byte[] buffer, int offset, int count);
    void Write(ReadOnlySpan<byte> buffer);
}