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
        await using var supercellStream = new SupercellStream(networkStream);

        supercellStream.SendMessage(CreateLoginMessage());
        Console.WriteLine(supercellStream.ReadMessage());
    }

    private static Message CreateLoginMessage()
    {
        var memoryStream = new MemoryStream();
        var supercellStream = new SupercellStream(memoryStream);
        
        supercellStream.WriteInt32(3);
        supercellStream.WriteInt32(38);
        
        supercellStream.WriteInt32(1);
        supercellStream.WriteInt32(67);
        supercellStream.WriteInt32(175);
        
        supercellStream.WriteString("be514e02b198d18287af1405089a0e72b849ac69");
        
        supercellStream.WriteInt32(1);
        supercellStream.WriteInt32(1);
        
        return new Message(10100, 0, memoryStream.ToArray());
    }
}