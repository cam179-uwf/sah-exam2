using InTheHand.Net.Sockets;

namespace CarController.Services;

public interface IBluetoothClient
{
    bool Connected { get; }
    bool DataAvailable { get; }
    Task Connect();
    void Disconnect();
    int Read(ref byte[] buffer, int offset, int count);
    void Write(ReadOnlySpan<byte> buffer);
}