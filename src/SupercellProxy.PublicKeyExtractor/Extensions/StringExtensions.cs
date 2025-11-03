namespace SupercellProxy.PublicKeyExtractor.Extensions;

public static class StringExtensions
{
    public static async ValueTask<byte[]> ReadContentAsync(this string input, CancellationToken cancellationToken = default)
    {
        if (Uri.TryCreate(input, UriKind.Absolute, out var parsedUri))
        {
            var scheme = parsedUri.Scheme;

            if (string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                using var httpClient = new HttpClient();

                if (parsedUri.Host is "temp.sh")
                {
                    var response = await httpClient.PostAsync(parsedUri, content: null, cancellationToken);
                    return await response.Content.ReadAsByteArrayAsync(cancellationToken);
                }

                return await httpClient.GetByteArrayAsync(parsedUri, cancellationToken);
            }

            if (string.Equals(scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                return await File.ReadAllBytesAsync(parsedUri.LocalPath, cancellationToken);
            }
        }

        return await File.ReadAllBytesAsync(input);
    }
}
