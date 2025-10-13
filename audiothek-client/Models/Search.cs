namespace audiothek_client.Models;

public class RootSearch
{
    public DataSearch data { get; set; }
}

public class DataSearch
{
    public Search search { get; set; }
}

public class Search
{
    public ItemsSearch items { get; set; }
}

public class ItemsSearch
{
    public int? totalCount { get; set; }
    public List<Nodes> nodes { get; set; } = new();
}

public class Nodes
{
    public string id { get; set; }
    public string coreId { get; set; }
    public string title { get; set; }
    public string path { get; set; }
    public string publishDate { get; set; }
    public int? duration { get; set; }
    public string itemType { get; set; }
    public List<Audio> audios { get; set; } = new();
    public ProgramSet? programSet { get; set; }
    
}


public class PublicationService
{
    public string id { get; set; }
    public string coreId { get; set; }
    public string title { get; set; }
    public string path { get; set; }
    public string organizationName { get; set; }
}