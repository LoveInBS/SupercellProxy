using SupercellProxy.PublicKeyExtractor;
using SupercellProxy.PublicKeyExtractor.Extensions;

// should be 5c344b84451436796b735cb62ee38df813a31798d21294f8c05e0f2b4ca4c047
Console.WriteLine(Convert.ToHexString(PublicKeyCodec.Decode2(Convert.FromHexString("47ff1e97c3c79c5b26aacf464ec7034b4ce4ffad21ba29f25d0c7c65be244e7e32e0ba1d6c65f0679c9c48e155ba02d577fed286d314e70206770663de9773acdce07397161506779753e7141054d2fe67c002ba40ec489caf52f06555a7bae013fd4e240aa67c0cfbaf29ba1de8ffe4885703c74eb4cfaaba349cc73afa1eff"))));

// should be 2FE57DA347CD62431528DAAC5FBB290730FFF684AFC4CFC2ED90995F58CB3B74
Console.WriteLine(Convert.ToHexString(PublicKeyCodec.Decode2(Convert.FromHexString("FCA5000068DF0000E6A3000043620000502A0000B36A0000DAFD0000729000001FE60000A13D0000E2570000C2CF000021DB00007E6500005AC6000043B7000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"))));

byte[] binary;

if (args.Length < 1)
{
    Console.WriteLine("Please provide the path or URL to IPA file or libg.so dump.\nAPK files are not supported.");
    return 1;
}

try
{
    var input = args[0];
    binary = await input.ReadContentAsync();
}
catch (Exception exception)
{
    Console.WriteLine($"Could not read content: {exception.Message}");
    return 2;
}

try
{
    if (binary.HasZipArchiveHeader()) // Only for .ipa files
        binary = binary.GetIpaAppEntry();
}
catch (Exception exception)
{
    Console.WriteLine($"Could not get binary from IPA: {exception.Message}");
    return 3;
}

try
{
    var serverPublicKey = GetServerPublicKey(binary);
    Console.WriteLine(Convert.ToHexString(serverPublicKey));
}
catch (Exception exception)
{
    Console.WriteLine($"Could not extract server public key:\n{exception}");
    return 4;
}

return 0;

static Span<byte> GetServerPublicKey(ReadOnlySpan<byte> binary)
{
    const int KeyLength = 128;
    const int ZeroesBeforeKey = 64;

    var foundIndex = -1;

    foreach (var index in binary.IndexesOf([0x1A, 0xD5, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]))
    {
        if (!binary.SliceBefore(index - KeyLength, ZeroesBeforeKey).IsAllZeros())
            continue;

        if (foundIndex is not -1)
            throw new InvalidOperationException($"Multiple possible server public keys found in the binary (expected 1):\n" +
                $"[{foundIndex}]:{Convert.ToHexString(binary.SliceBefore(foundIndex, KeyLength))}\n" +
                $"[{index}]:{Convert.ToHexString(binary.SliceBefore(index, KeyLength))}");

        foundIndex = index;
    }

    if (foundIndex is -1)
        throw new InvalidOperationException("Could not find server public key in the binary.");

    return PublicKeyCodec.Decode(binary.SliceBefore(foundIndex, KeyLength));
}
