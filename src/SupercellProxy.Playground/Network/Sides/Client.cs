using System.Net.Sockets;
using System.Text.RegularExpressions;
using SupercellProxy.Playground.Crypto;
using SupercellProxy.Playground.Network.Messages;
using SupercellProxy.Playground.Network.Messages.Clientbound;
using SupercellProxy.Playground.Network.Messages.Serverbound;
using SupercellProxy.Playground.Network.Streams;

namespace SupercellProxy.Playground.Network.Sides;

public partial class Client(string upstreamHost, int upstreamPort)
{
    private readonly HttpClient _httpClient = new();
    
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var upstream = new TcpClient();
        await upstream.ConnectAsync(upstreamHost, upstreamPort, cancellationToken);
        
        await using var networkStream = upstream.GetStream();
        await using var supercellStream = new SupercellStream(networkStream);

        // 10100
        var clientHelloContainer = CreateClientHello().ToContainer();
        Console.WriteLine(clientHelloContainer);
        await supercellStream.WriteMessageAsync(clientHelloContainer, cancellationToken);
        
        // 20100
        var serverHelloContainer = await supercellStream.ReadMessageAsync(cancellationToken);
        var serverHello = ServerHelloMessage.Create(serverHelloContainer);
        Console.WriteLine(serverHelloContainer);

        // 10101
        var loginContainer = CreateLoginMessageContainer(await GetHayDayPublicKeyAsync(cancellationToken), serverHello.SessionKey);
        Console.WriteLine(loginContainer);
        await supercellStream.WriteMessageAsync(loginContainer, cancellationToken);
        
        // any
        var anyContainer = await supercellStream.ReadMessageAsync(cancellationToken);
        Console.WriteLine(anyContainer);
    }

    private static MessageContainer CreateLoginMessageContainer(Memory<byte> serverPublicKey, Memory<byte> sessionKey)
    {
        var loginMessageStream = SupercellStream.Create();

        loginMessageStream.WriteInt64(1); // UserId = reader.ReadInt64();
        loginMessageStream.WriteString(""); // UserToken = reader.ReadString();
        loginMessageStream.WriteInt32(1); // MajorVersion = reader.ReadInt32();
        loginMessageStream.WriteInt32(67); // MinorVersion = reader.ReadInt32();
        loginMessageStream.WriteInt32(175); // PatchVersion = reader.ReadInt32();
        loginMessageStream.WriteString(""); // MasterHash = reader.ReadString();
        loginMessageStream.WriteString(""); // Udid = reader.ReadString();
        loginMessageStream.WriteString(""); // OpenUdid = reader.ReadString();
        loginMessageStream.WriteString(""); // MacAddress = reader.ReadString();
        loginMessageStream.WriteString(""); // DeviceModel = reader.ReadString();
        loginMessageStream.WriteInt32(0); // LocaleKey = reader.ReadInt32();
        loginMessageStream.WriteString(""); // Language = reader.ReadString();
        loginMessageStream.WriteString(""); // AdvertisingGuid = reader.ReadString();
        loginMessageStream.WriteString(""); // OSVersion = reader.ReadString();
        loginMessageStream.WriteByte(0); // Unknown2 = reader.ReadByte();
        loginMessageStream.WriteString(""); // Unknown3 = reader.ReadString();
        loginMessageStream.WriteString(""); // AndroidDeviceId = reader.ReadString();
        loginMessageStream.WriteString(""); // FacebookDistributionID = reader.ReadString();
        loginMessageStream.WriteBoolean(false); // IsAdvertisingTrackingEnabled = reader.ReadBoolean();
        loginMessageStream.WriteString(""); // VendorGUID = reader.ReadString();
        loginMessageStream.WriteInt32(0); // Seed = reader.ReadInt32();
        loginMessageStream.WriteByte(0); // Unknown4 = reader.ReadByte();
        loginMessageStream.WriteString(""); // Unknown5 = reader.ReadString();
        loginMessageStream.WriteString(""); // Unknown6 = reader.ReadString();
        loginMessageStream.WriteString(""); // ClientVersion = reader.ReadString();

        var loginMessageBuffer = loginMessageStream.ToArray();
        
        // https://github.com/FICTURE7/CoCSharp/blob/server-dev/src/CoCSharp.Proxy/MessageProcessorNaClProxy.cs#L102
        
        var localKeyPair = Crypto8.GenerateKeyPair();
        var localNonce = Crypto8.GenerateNonce();
        
        var serverboundCrypto = new Crypto8(Direction.Serverbound, localKeyPair);
        
        loginMessageBuffer = [..sessionKey.Span, ..localNonce, ..loginMessageBuffer];
        serverboundCrypto.UpdateSharedKey(serverPublicKey.ToArray());
        serverboundCrypto.Encrypt(ref loginMessageBuffer);

        Console.WriteLine(Convert.ToHexString(localKeyPair.PublicKey));
        
        return new MessageContainer(10101, 3242, new SupercellStream(new MemoryStream([..localKeyPair.PublicKey, ..loginMessageBuffer])));
    }

    private async ValueTask<byte[]> GetHayDayPublicKeyAsync(CancellationToken cancellationToken = default)
    {
        var content = await _httpClient.GetStringAsync("https://raw.githubusercontent.com/caunt/SupercellProxy/refs/heads/main/KEYS.md", cancellationToken);
        var hayDayMatch = HayDayPublicKeyRegex().Match(content);
        
        if (!hayDayMatch.Success) 
            throw new InvalidOperationException("Hay Day key not found.");

        return Convert.FromHexString(hayDayMatch.Groups[1].Value);
    }
    
    private static ClientHelloMessage CreateClientHello()
    {
        return new ClientHelloMessage
        {
            ProtocolVersion = 3,
            KeyVersion = 38,

            MajorVersion = 1,
            MinorVersion = 67,
            PatchVersion = 175,

            // 1.67.170 => be514e02b198d18287af1405089a0e72b849ac69
            // 1.67.175 => fdb648cea5e3494c3cafc32eca103331d85c5bfd
            FingerprintSha1 = "fdb648cea5e3494c3cafc32eca103331d85c5bfd",

            DeviceType = 1,
            AppStore = 1
        };
    }

    [GeneratedRegex(@"(?s)##\s*Hay Day.*?`([0-9A-Fa-f]{64})`")]
    private static partial Regex HayDayPublicKeyRegex();
}