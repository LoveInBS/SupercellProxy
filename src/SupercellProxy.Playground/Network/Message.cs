namespace SupercellProxy.Playground.Network;

public record Message(ushort Id, ushort Version, byte[] Payload);