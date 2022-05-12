namespace SpotiHub.Core.Entity;

public class Options
{
    public bool Enabled { get; set; }
    public bool LimitedAvailability { get; set; }
    public bool DefaultEmoji { get; set; }
    public bool GenreEmojis { get; set; }
    public string? FallbackMessage { get; set; }
    public string? FallbackEmoji { get; set; }
    public TimeSpan ClearAfter { get; set; }
}