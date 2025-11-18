using Blake2Fast;
using SupercellProxy.Crypto.Piranha.Wrappers;
using SupercellProxy.Playground.Crypto.NaCl;
using SupercellProxy.Playground.Network.Messages;
using SupercellProxy.Playground.Network.Messages.Clientbound;
using SupercellProxy.Playground.Network.Messages.Serverbound;
using SupercellProxy.Playground.Network.Streams;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace SupercellProxy.Playground.Network.Sides;

public partial class Client(string upstreamHost, int upstreamPort)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
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
            var serverPublicKey = Convert.FromHexString("FF9C8D567D78F6DE1BDB27F7B4E4EE8D3F359292149F5EF3C46D59C1404DC91D");
            // await HayDayApi.GetServerPublicKeyAsync(cancellationToken)
            // serverPublicKey = Convert.FromHexString("F3EF2947524B5E4D8F92F9A69328517D6383388B6C72F786C2749BBE1CD9E07D");
            var loginContainer = CreateLoginMessageContainer(serverPublicKey, serverHello.SessionKey.Span);
            Console.WriteLine(loginContainer);
            await supercellStream.WriteMessageAsync(loginContainer, cancellationToken);

            // any
            var anyContainer = await supercellStream.ReadMessageAsync(cancellationToken);
            Console.WriteLine(anyContainer);
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine("Connection closed by remote host.");
        }
    }

    private static MessageContainer CreateLoginMessageContainer(Span<byte> serverPublicKey, Span<byte> sessionToken)
    {
        var loginMessageStream = SupercellStream.Create();

        // Account/Session ID - Low/High
        loginMessageStream.WriteUInt32(0);
        loginMessageStream.WriteUInt32(0);

        // pass token
        loginMessageStream.WriteOptionalString("");

        // resource sha
        loginMessageStream.WriteOptionalString("fdb648cea5e3494c3cafc32eca103331d85c5bfd"); // required 20 bytes sha1

        // unknown flag (crc or id?)
        loginMessageStream.WriteUInt32(1117359); // sub_1002C9648

        // device id triplet - udid, openudid, macaddress
        loginMessageStream.WriteOptionalString("e4b7c2a9f13d58be");
        loginMessageStream.WriteOptionalString("9f0c3a7e2b1d4c56a8e0f23b5d79c1ab");
        loginMessageStream.WriteOptionalString("01:5a:40:c7:c2:c3");

        // device
        loginMessageStream.WriteOptionalString("iPad12.1");

        // adid
        loginMessageStream.WriteOptionalString("97AFD440-1537-47F2-A2D4-0ACCC1C78BE1");

        // add tacking enabled
        loginMessageStream.WriteBoolean(true);

        // os version
        loginMessageStream.WriteOptionalString("18.2");

        // model
        loginMessageStream.WriteString("iPad");

        // manufacturer
        loginMessageStream.WriteString("Apple");

        // preferred language id
        loginMessageStream.WriteOptionalString("en-US");

        // region
        loginMessageStream.WriteString("US");

        // unknown flag
        loginMessageStream.WriteBoolean(true);

        // app version
        loginMessageStream.WriteString("1.67.175");

        // build
        loginMessageStream.WriteUInt32(1);

        // unknown pair
        loginMessageStream.WriteUInt64(0);
        loginMessageStream.WriteUInt32(0xFFFFFFFF);

        // unknown trailing strings
        loginMessageStream.WriteString("");
        loginMessageStream.WriteString("");

        var loginMessageBuffer = loginMessageStream.ToArray();

        var clientPrivateKey = RandomNumberGenerator.GetBytes(count: 32);
        var clientPublicKey = TweetNaCl.CryptoScalarmultBase(clientPrivateKey);
        var clientNonce = RandomNumberGenerator.GetBytes(count: 24);

        var hasher = Blake2b.CreateIncrementalHasher(digestLength: 24);
        hasher.Update(clientPublicKey);
        hasher.Update(serverPublicKey);
        var tempNonce = hasher.Finish();

        var encrypted = CustomNaCl.CreatePublicBox([.. sessionToken, .. clientNonce, .. loginMessageBuffer], tempNonce, clientPrivateKey, serverPublicKey.ToArray());
        var encryptedWorking = TweetNaCl.CryptoBox([.. sessionToken, .. clientNonce, .. loginMessageBuffer], tempNonce, serverPublicKey.ToArray(), clientPrivateKey);

        return new MessageContainer(10101, 3247, new SupercellStream(new MemoryStream([.. clientPublicKey, .. encrypted])));

        // FF9C8D567D78F6DE 1BDB27F7B4E4EE8D3F359292149F5EF3C46D59C1404DC91D
        // 8602D5C784329E3E 9A763F079F247156C1649B2D0E36B1C73CCB3AE5FE3CEC54A8CE22FA4D2C026730BCE46EAE4205DE0ED6432B0D0BF8C2052F884CF1650E37F5C20E65C2AC882F4AC1F80BC00743D6CAD10542606DE4BCD92D022C713D22CE3C32EC3C2BBC3ACBC258B136BFAF9B64C80C7124DB983F7684309E32BD3ED502
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

            // Squad Busters: ProtocolVersion = 1,
            // Squad Busters: KeyVersion = 57,
            // Squad Busters: 
            // Squad Busters: MajorVersion = 13,
            // Squad Busters: MinorVersion = 807,
            // Squad Busters: PatchVersion = 7,
            // Squad Busters: 
            // Squad Busters: FingerprintSha1 = "a0bcd279dbf934648bca39d48cd609e65623a9d9",
            // Squad Busters: 
            // Squad Busters: DeviceType = 1,
            // Squad Busters: AppStore = 1
        };
    }
}