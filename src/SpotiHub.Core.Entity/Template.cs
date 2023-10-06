namespace SpotiHub.Core.Entity;

public class Template
{
    public Template()
    {
        MessageParameters = new List<EMessageParameter>();
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool Private { get; set; }
    public string? Emoji { get; set; }
    public string Message { get; set; } = default!;
    public List<EMessageParameter> MessageParameters { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public ApplicationUser? LastUpdatedBy { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    
    public enum EMessageParameter
    {
        Track,
        Artist,
        Playlist,
        Genre
    }
}