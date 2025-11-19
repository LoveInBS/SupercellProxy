using SupercellProxy.Playground.Network.Sides;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

var upstreamHost = args.Length > 0 ? args[0] : "game.haydaygame.com"; // "game.squadbustersgame.com"; //
var upstreamPort = args.Length > 1 && int.TryParse(args[1], out var up) ? up : 9339;
var listenAddress = args.Length > 2 ? args[2] : "0.0.0.0";
var listenPort = args.Length > 3 && int.TryParse(args[3], out var lp) ? lp : 9339;

var client = new Client(upstreamHost, upstreamPort);
var server = new Server(listenAddress, listenPort);
var proxy = new Proxy(upstreamHost, upstreamPort, listenAddress, listenPort);

await client.RunAsync();
// await server.RunAsync();
// await proxy.RunAsync();
