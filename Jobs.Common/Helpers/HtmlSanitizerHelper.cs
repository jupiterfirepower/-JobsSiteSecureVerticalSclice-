using System.Text.RegularExpressions;

namespace Jobs.Common.Helpers;

public static class HtmlSanitizerHelper
{
    private static readonly string[] BlackListTags = { "script", "iframe", "object", "embed", "form" };
    private static readonly string[] BlackListAttributes = { "onload", "onclick", "onerror", "href", "src" };

    public static string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove blacklisted tags
        Array.ForEach(BlackListTags, tag => 
        {
            var tagRegex = new Regex($"<\\/?\\s*{tag}\\s*[^>]*>", RegexOptions.IgnoreCase);
            input = tagRegex.Replace(input, string.Empty);
        });
        
        // Remove blacklisted attributes
        Array.ForEach(BlackListAttributes, attr => 
        {
            var attrRegex = new Regex($"{attr}\\s*=\\s*['\"].*?['\"]", RegexOptions.IgnoreCase);
            input = attrRegex.Replace(input, string.Empty);
        });

        // Remove javascript: links
        var jsLinkRegex = new Regex(@"href\s*=\s*['""]javascript:[^'""]*['""]", RegexOptions.IgnoreCase);
        input = jsLinkRegex.Replace(input, string.Empty);

        // Remove plain http and https links
        // var plainLinkRegex = new Regex(@"(http|https):\/\/[^\s<>]+", RegexOptions.IgnoreCase);
        // input = plainLinkRegex.Replace(input, string.Empty);

        // Encode any remaining HTML
        //return HttpUtility.HtmlEncode(input);
        return input;
    }
}