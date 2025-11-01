using System.Text.RegularExpressions;

namespace JustWatchProxy.Helpers;

public static class LoggingHelper
{
    public static string SanitizeForLogging(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // Remove control characters and newlines to prevent log injection
        return Regex.Replace(input, @"[\r\n\t\f\v\u0000-\u001F\u007F-\u009F]", "", RegexOptions.None, TimeSpan.FromMilliseconds(100));
    }
}
