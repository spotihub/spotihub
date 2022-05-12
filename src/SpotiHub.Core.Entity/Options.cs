namespace SpotiHub.Core.Entity;

public class Options
{
    public Options()
    {
        Enabled = true;
        DefaultEmoji = "🎶";
    }

    public bool Enabled { get; set; }
    public string DefaultEmoji { get; set; }
    public bool LimitedAvailability { get; set; }
    public bool GenreEmojis { get; set; }
    public string? FallbackMessage { get; set; }
    public string? FallbackEmoji { get; set; }
    public TimeSpan? ClearAfter { get; set; }
}