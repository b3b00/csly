namespace sly.sourceGenerator.generated.extractor;

public static class StringExtensions
{
    public static string Capitalize(this string str)
    {
        return str.Substring(0, 1).ToUpper() + str.Substring(1);
    }
}