namespace SpotiHub.Core.Application.Options;

public class SpotifyOptions
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public Uri RedirectUrl { get; set; } = null!;
}