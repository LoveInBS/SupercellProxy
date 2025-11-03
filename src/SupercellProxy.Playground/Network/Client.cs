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

        SendMessage(supercellStream, 10100, 0, CreateLoginMessage());
        Console.WriteLine(ReadMessage(supercellStream));
    }

    private static void SendMessage(ScStream stream, ushort id, ushort version, ReadOnlySpan<byte> message)
    {
        var span = (stackalloc byte[7]);
        
        BinaryPrimitives.WriteUInt16BigEndian(span[..2], id);

        var length = message.Length;
        
        span[2] = (byte)(length >> 16);
        span[3] = (byte)(length >> 8);
        span[4] = (byte)length;
        
        BinaryPrimitives.WriteUInt16BigEndian(span[5..7], version);

        stream.WriteBytes(span);
        stream.WriteBytes(message);
    }

    private static Message ReadMessage(ScStream stream)
    {
        var header = stream.ReadBytes(7);
        var span = header.AsSpan();

        var id = BinaryPrimitives.ReadUInt16BigEndian(span[0..2]);
        var length = (span[2] << 16) | (span[3] << 8) | span[4];
        var version = BinaryPrimitives.ReadUInt16BigEndian(span[5..7]);

        return new Message(id, version, stream.ReadBytes(length));
    }

    private static ReadOnlySpan<byte> CreateLoginMessage()
    {
        var memoryStream = new MemoryStream();
        var supercellStream = new ScStream(memoryStream);
        
        supercellStream.WriteInt32(3);
        supercellStream.WriteInt32(38);
        
        supercellStream.WriteInt32(1);
        supercellStream.WriteInt32(67);
        supercellStream.WriteInt32(175);
        
        supercellStream.WriteString("be514e02b198d18287af1405089a0e72b849ac69");
        
        supercellStream.WriteInt32(1);
        supercellStream.WriteInt32(1);
        
        return memoryStream.ToArray();
    }
}