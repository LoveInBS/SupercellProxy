using SupercellProxy.Playground.Network;

var upstreamHost = args.Length > 0 ? args[0] : "game.haydaygame.com";
var listenPort = args.Length > 1 && int.TryParse(args[1], out var lp) ? lp : 9339;
var upstreamPort = args.Length > 2 && int.TryParse(args[2], out var up) ? up : 9339;

var proxy = new Proxy(upstreamHost, upstreamPort, listenPort);
var client = new Client(upstreamHost, upstreamPort);
// await proxy.RunAsync();