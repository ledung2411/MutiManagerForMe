namespace MutiManagerForMe.App.Models;

public sealed class NoteEntry
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string Preview
    {
        get
        {
            var text = Content.ReplaceLineEndings(" ").Trim();
            return text.Length > 90 ? $"{text[..90]}…" : text;
        }
    }
}
