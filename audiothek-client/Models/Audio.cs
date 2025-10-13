namespace audiothek_client.Models;

public class Audio
{
    public string? downloadUrl { get; set; }
    public string url { get; set; }
    public bool allowDownload { get; set; }
}