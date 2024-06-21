using System.Globalization;

public class Data
{
    public ProgramSets programSets { get; set; }
    public ProgramSetByNodeId programSetByNodeId { get; set; }
}

public class EditorialCategory
{
    public string title { get; set; }
    public string id { get; set; }
}

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
    public override string ToString()
    {
        if (lastItemAdded != null)
            return $"{title} ({lastItemAdded.Value.Date.ToString("dd.M.yyyy", CultureInfo.InvariantCulture)})";
        return title;
    }
}

public class ProgramSets
{
    public List<Node> nodes { get; set; }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Audio
{
    public string? downloadUrl { get; set; }
    public string url { get; set; }
    public bool allowDownload { get; set; }
}


public class Items
{
    public List<Node> nodes { get; set; }
}

public class ProgramSetByNodeId
{
    public int rowId { get; set; }
    public Items items { get; set; }
}

public class Root
{
    public Data data { get; set; }
}

