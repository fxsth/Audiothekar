namespace audiotagger.Models;

public class AlbumMetadata
{
    public string Artist { get; set; }
    public string Album { get; set; }
    public string AlbumArtist { get; set; }
    public string Genre { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string AlbumCover { get; set; }
}