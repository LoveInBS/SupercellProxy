using SupercellProxy.Playground.Network.Streams;

namespace SupercellProxy.Playground.Network.Messages;

public record MessageContainer(ushort Id, ushort Version, SupercellStream Payload);