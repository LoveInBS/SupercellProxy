namespace SupercellProxy.PublicKeyExtractor.Extensions;

public static class StringExtensions
{
    public static async ValueTask<byte[]> ReadContentAsync(this string input)
    {
        if (Uri.TryCreate(input, UriKind.Absolute, out var parsedUri))
        {
            var scheme = parsedUri.Scheme;

            if (string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetByteArrayAsync(parsedUri);
            }

            if (string.Equals(scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                return await File.ReadAllBytesAsync(parsedUri.LocalPath);
            }
        }

        return await File.ReadAllBytesAsync(input);
    }
}
