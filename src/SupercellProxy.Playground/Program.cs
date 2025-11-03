using SupercellProxy.Playground;
using SupercellProxy.Playground.Crypto;
using SupercellProxy.Playground;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using SupercellProxy.Playground.Network;
using SupercellProxy.Playground.Network.Streams;

var upstreamHost = args.Length > 0 ? args[0] : "game.haydaygame.com";
var listenPort = args.Length > 1 && int.TryParse(args[1], out var lp) ? lp : 9339;
var upstreamPort = args.Length > 2 && int.TryParse(args[2], out var up) ? up : 9339;

var proxy = new Proxy(listenPort, upstreamPort, upstreamHost);
// await proxy.RunAsync();