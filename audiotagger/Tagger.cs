using System.Drawing;
using System.Text.RegularExpressions;
using IdSharp.Tagging.ID3v2;
using audiotagger.Models;
using IdSharp.Tagging.ID3v2.Frames;

namespace audiotagger;

public static class Tagger
{
    public static void TagAlbumInDirectory(string directoryPath, AlbumMetadata albumMetadata)
    {
        IEnumerable<string> filePaths = Directory.GetFiles(directoryPath)
            .OrderBy(n => Regex.Replace(n, @"\d+", n => n.Value.PadLeft(4, '0')));
        foreach (string filePath in filePaths)
        {
            ID3v2Tag tag = new ID3v2Tag(filePath)
            {
                Album = albumMetadata.Album,
                Artist = albumMetadata.Artist,
                Genre = albumMetadata.Genre,
                Title = Path.GetFileNameWithoutExtension(filePath),
                AlbumArtist = albumMetadata.AlbumArtist,
                Year = albumMetadata.ReleaseDate?.Year.ToString()
            };
            tag.Save(filePath);
        }
    }

    public static void SetAlbumCover(ID3v2Tag tag, string imageString)
    {
        tag.PictureList.Clear();
        IAttachedPicture attachedPicture = tag.PictureList.AddNew() ??
                                           throw new InvalidOperationException();
        attachedPicture.PictureType = PictureType.CoverFront;
        attachedPicture.MimeType = "image/jpeg";
        using (Image image = new Bitmap(imageString))
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms,image.RawFormat);
                attachedPicture.PictureData = ms.ToArray();
            }
        }
    }
}