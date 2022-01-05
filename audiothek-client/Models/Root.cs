namespace audiothek_client.Models
{
    public class EditorialCategory
    {
        public string title { get; set; }
    }

    public class Image
    {
        public string url1X1 { get; set; }
    }

    public class Node
    {
        public string title { get; set; }
        public List<Audio> audios { get; set; }
        public string numberOfElements { get; set; }
        public string nodeId { get; set; }
        public int rowId { get; set; }
        public EditorialCategory editorialCategory { get; set; }
        public Image image { get; set; }
    }

    public class ProgramSets
    {
        public List<Node> nodes { get; set; }
    }

    public class Query
    {
        public ProgramSets programSets { get; set; }
    }

    public class Data
    {
        public Query query { get; set; }
        public ProgramSetByNodeId programSetByNodeId { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
    }
    
    public class Audio
    {
        public string? downloadUrl { get; set; }
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

}