using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace PlaywrightAutomation.Utils;

/// <summary>
/// Provides text normalization and comparison utilities for test validation
/// </summary>
public static class TextNormalizer
{
    /// <summary>
    /// Removes HTML tags from text using HtmlAgilityPack
    /// </summary>
    /// <param name="html">HTML text to clean</param>
    /// <returns>Text with HTML tags removed</returns>
    public static string RemoveHtmlTags(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        
        return htmlDoc.DocumentNode.InnerText;
    }

    /// <summary>
    /// Removes punctuation from text using regex pattern [^\w\s]
    /// </summary>
    /// <param name="text">Text to clean</param>
    /// <returns>Text with punctuation removed</returns>
    public static string RemovePunctuation(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return Regex.Replace(text, @"[^\w\s]", string.Empty);
    }

    /// <summary>
    /// Removes excessive whitespace from text using regex pattern \s+
    /// </summary>
    /// <param name="text">Text to clean</param>
    /// <returns>Text with normalized whitespace</returns>
    public static string RemoveExcessiveWhitespace(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    /// <summary>
    /// Normalizes text by removing HTML tags, punctuation, excessive whitespace,
    /// converting to lowercase, and applying Unicode normalization
    /// </summary>
    /// <param name="text">Text to normalize</param>
    /// <returns>Normalized text</returns>
    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Step 1: Remove HTML tags
        var cleaned = RemoveHtmlTags(text);

        // Step 2: Apply Unicode normalization (FormD - canonical decomposition)
        cleaned = cleaned.Normalize(NormalizationForm.FormD);

        // Step 3: Convert to lowercase
        cleaned = cleaned.ToLowerInvariant();

        // Step 4: Remove punctuation
        cleaned = RemovePunctuation(cleaned);

        // Step 5: Remove excessive whitespace
        cleaned = RemoveExcessiveWhitespace(cleaned);

        return cleaned;
    }

    /// <summary>
    /// Extracts unique words from text
    /// </summary>
    /// <param name="text">Text to process</param>
    /// <returns>HashSet of unique words</returns>
    public static HashSet<string> GetUniqueWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new HashSet<string>();

        var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Counts unique words in text
    /// </summary>
    /// <param name="text">Text to process</param>
    /// <returns>Count of unique words</returns>
    public static int CountUniqueWords(string text)
    {
        return GetUniqueWords(text).Count;
    }

    /// <summary>
    /// Compares two texts and returns detailed diff results using DiffPlex
    /// </summary>
    /// <param name="text1">First text to compare</param>
    /// <param name="text2">Second text to compare</param>
    /// <returns>Side-by-side diff model showing differences</returns>
    public static SideBySideDiffModel CompareTexts(string text1, string text2)
    {
        var diffBuilder = new SideBySideDiffBuilder(new Differ());
        return diffBuilder.BuildDiffModel(text1 ?? string.Empty, text2 ?? string.Empty);
    }

    /// <summary>
    /// Formats diff results into a readable string for test output
    /// </summary>
    /// <param name="diffModel">Diff model from CompareTexts</param>
    /// <returns>Formatted string showing differences</returns>
    public static string FormatDiffResults(SideBySideDiffModel diffModel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n=== TEXT COMPARISON RESULTS ===");
        sb.AppendLine("\n--- OLD TEXT ---");
        
        foreach (var line in diffModel.OldText.Lines)
        {
            var prefix = line.Type switch
            {
                ChangeType.Deleted => "- ",
                ChangeType.Modified => "~ ",
                _ => "  "
            };
            sb.AppendLine($"{prefix}{line.Text}");
        }

        sb.AppendLine("\n--- NEW TEXT ---");
        
        foreach (var line in diffModel.NewText.Lines)
        {
            var prefix = line.Type switch
            {
                ChangeType.Inserted => "+ ",
                ChangeType.Modified => "~ ",
                _ => "  "
            };
            sb.AppendLine($"{prefix}{line.Text}");
        }

        return sb.ToString();
    }
}
