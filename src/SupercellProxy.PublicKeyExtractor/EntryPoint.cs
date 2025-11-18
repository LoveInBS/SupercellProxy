using SupercellProxy.PublicKeyExtractor;
using SupercellProxy.PublicKeyExtractor.Extensions;

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
