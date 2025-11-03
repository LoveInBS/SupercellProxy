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

        await supercellStream.SendMessageAsync(await CreateLoginMessageAsync(), cancellationToken);
        Console.WriteLine(await supercellStream.ReadMessageAsync(cancellationToken));
    }

    private static async ValueTask<Message> CreateLoginMessageAsync()
    {
        var memoryStream = new MemoryStream();
        var supercellStream = new SupercellStream(memoryStream);
        
        await supercellStream.WriteInt32Async(3);
        await supercellStream.WriteInt32Async(38);
        
        await supercellStream.WriteInt32Async(1);
        await supercellStream.WriteInt32Async(67);
        await supercellStream.WriteInt32Async(175);
        
        await supercellStream.WriteStringAsync("be514e02b198d18287af1405089a0e72b849ac69");
        
        await supercellStream.WriteInt32Async(1);
        await supercellStream.WriteInt32Async(1);
        
        return new Message(10100, 0, memoryStream.ToArray());
    }
}