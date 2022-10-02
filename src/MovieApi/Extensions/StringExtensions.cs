namespace MovieApi.Extensions;

public static class StringExtensions
{
    public static int? ToInt(this string? s)
    {
        return int.TryParse(s, out var i) ? (int?)i : null;
    }
}