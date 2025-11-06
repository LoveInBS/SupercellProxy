namespace SupercellProxy.Playground.Network.Streams;

public partial class SupercellStream
{
    private readonly byte[] _buffer = new byte[65536];

    public Memory<byte> RentExactly(int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, _buffer.Length);
        return _buffer.AsMemory(0, length);
    }
}