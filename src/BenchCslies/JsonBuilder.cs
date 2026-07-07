using System.Diagnostics.CodeAnalysis;

namespace BenchCslies;

public class JsonBuilder
{
    private static readonly Random _random = new();

    public static string BuildJson(int length, int depth, int width)
    {

        var items = Enumerable.Repeat(1, length)
            .Select<int, string>(_ => BuildObject(depth, width));
        return "["+string.Join("," , items)+"]";
    }

    private static string BuildObject(int depth, int width)
    {
        if (depth == 0)
        {
            return RandomString(6);
        }
        var properties = Enumerable.Repeat(1, width)
            .Select(_ => $"{RandomString(5)} : {BuildObject(depth - 1, width)}");

        return "{"+string.Join("," + Environment.NewLine, properties)+Environment.NewLine+"}";
    }

    [SuppressMessage("security", "CA5394:Use cryptographically secure random number generators", Justification = "Test code")]
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return "\""+new string(
            Enumerable
                .Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)])
                .ToArray()
        )+"\"";
    }
}