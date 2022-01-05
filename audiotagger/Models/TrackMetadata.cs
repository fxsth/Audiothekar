namespace audiotagger.Models;

public class TrackMetadata
{
    public string Title { get; set; }
    public uint? TrackNumber { get; set; }
    public AlbumMetadata AlbumMetadata { get; set; }
}