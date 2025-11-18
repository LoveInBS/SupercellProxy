using System.Text.RegularExpressions;

namespace SupercellProxy.Playground.Supercell;

public static partial class HayDayApi
{
    private static readonly HttpClient HttpClient = new();

    public static async ValueTask<byte[]> GetServerPublicKeyAsync(CancellationToken cancellationToken = default)
    {
        var content = await HttpClient.GetStringAsync("https://raw.githubusercontent.com/caunt/SupercellProxy/refs/heads/main/KEYS.md", cancellationToken);
        var hayDayMatch = HayDayPublicKeyRegex().Match(content);

        if (!hayDayMatch.Success)
            throw new InvalidOperationException("Hay Day key not found.");

        return Convert.FromHexString(hayDayMatch.Groups[1].Value);
    }

    [GeneratedRegex(@"(?ms)^##[^\r\n]*Hay Day[^\r\n]*\r?\n.*?`([0-9A-Fa-f]{64})`")]
    private static partial Regex HayDayPublicKeyRegex();
}