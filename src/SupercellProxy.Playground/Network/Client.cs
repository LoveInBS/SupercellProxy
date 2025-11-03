using System.Buffers.Binary;
using System.Net.Sockets;
using SupercellProxy.Playground.Network.Streams;

namespace SupercellProxy.Playground.Network;

public class Client(string upstreamHost, int upstreamPort)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var upstream = new TcpClient();
        await upstream.ConnectAsync(upstreamHost, upstreamPort, cancellationToken);
        
        await using var networkStream = upstream.GetStream();
        await using var supercellStream = new ScStream(networkStream);

        supercellStream.WriteBytes(WrapMessage(10100, 0, CreateLoginMessage()));
    }

    private static ReadOnlySpan<byte> WrapMessage(ushort id, ushort version, ReadOnlySpan<byte> message)
    {
        var header = new byte[7];
        var span = header.AsSpan();
        
        BinaryPrimitives.WriteUInt16BigEndian(span[0..2], id);

        var length = message.Length;
        
        span[2] = (byte)(length >> 16);
        span[3] = (byte)(length >> 8);
        span[4] = (byte)length;
        
        BinaryPrimitives.WriteUInt16BigEndian(span[5..7], version);

        return header;
    }

    private static ReadOnlySpan<byte> CreateLoginMessage()
    {
        var memoryStream = new MemoryStream();
        var supercellStream = new ScStream(memoryStream);
        
        supercellStream.WriteInt32(3);
        supercellStream.WriteInt32(38);
        
        supercellStream.WriteInt32(1);
        supercellStream.WriteInt32(67);
        supercellStream.WriteInt32(170);
        
        supercellStream.WriteString("be514e02b198d18287af1405089a0e72b849ac69");
        
        supercellStream.WriteInt32(1);
        supercellStream.WriteInt32(1);
        
        return memoryStream.ToArray();
    }
}