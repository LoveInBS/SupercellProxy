using System.Net.Sockets;

namespace SupercellProxy.Playground.Network;

public class Client(string upstreamHost, int upstreamPort)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var upstream = new TcpClient();
        await upstream.ConnectAsync(upstreamHost, upstreamPort, cancellationToken);
    }
}