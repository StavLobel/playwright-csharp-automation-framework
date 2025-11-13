using System.Text.Json.Serialization;

namespace PlaywrightAutomation.Models;

public class MediaWikiResponse
{
    [JsonPropertyName("parse")]
    public ParseResult? Parse { get; set; }
}

public class ParseResult
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("pageid")]
    public int PageId { get; set; }

    [JsonPropertyName("sections")]
    public List<Section>? Sections { get; set; }

    [JsonPropertyName("text")]
    public TextContent? Text { get; set; }
}

public class Section
{
    [JsonPropertyName("toclevel")]
    public int TocLevel { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("line")]
    public string Line { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public string Index { get; set; } = string.Empty;

    [JsonPropertyName("fromtitle")]
    public string FromTitle { get; set; } = string.Empty;

    [JsonPropertyName("byteoffset")]
    public int? ByteOffset { get; set; }

    [JsonPropertyName("anchor")]
    public string Anchor { get; set; } = string.Empty;
}

public class TextContent
{
    [JsonPropertyName("*")]
    public string Content { get; set; } = string.Empty;
}
