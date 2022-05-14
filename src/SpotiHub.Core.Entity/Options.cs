namespace SpotiHub.Core.Entity;

public class Options
{
    public bool Enabled { get; set; }
    public Template Template { get; set; } = default!;
    public bool LimitedAvailability { get; set; }
    public bool GenreEmojis { get; set; }
    public Template? FallbackTemplate { get; set; }
    public TimeSpan? ClearAfter { get; set; }
}