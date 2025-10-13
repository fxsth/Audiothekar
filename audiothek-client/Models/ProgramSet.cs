namespace audiothek_client.Models;

public class ProgramSet
{
    public string id { get; set; }
    public string nodeId { get; set; }
    public string coreId { get; set; }
    public int? numberOfElements { get; set; }
    public EditorialCategory editorialCategory { get; set; }
    public string title { get; set; }
    public PublicationService publicationService { get; set; }
}

