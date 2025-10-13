using System.Globalization;

namespace audiothek_client.Models;

public class Node
{
    public string title { get; set; }
    public int? numberOfElements { get; set; }
    public string nodeId { get; set; }
    public int rowId { get; set; }
    public string summary { get; set; }
    public string description { get; set; }
    public string assetId { get; set; }
    public int duration { get; set; }
    public bool isPublished { get; set; }
    public int? episodeNumber { get; set; }
    public DateTimeOffset? publishDate { get; set; }
    public DateTimeOffset? lastItemAdded { get; set; }
    public EditorialCategory? editorialCategory { get; set; }
    public List<Audio> audios { get; set; }
    public string itemType { get; set; }
    public Node? programSet { get; set; }

    public override string ToString()
    {
        if (lastItemAdded != null)
            return $"{title} ({lastItemAdded.Value.Date.ToString("dd.M.yyyy", CultureInfo.InvariantCulture)})";
        return title;
    }
}