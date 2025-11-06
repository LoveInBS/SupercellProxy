namespace SupercellProxy.Playground.Extensions;

public static class StringExtensions
{
    public static string? ToStringPadLeft<T>(this T value, int width, char @char = '.') where T : struct
    {
        return value.ToString()?.PadLeft(width, @char);
    }
}