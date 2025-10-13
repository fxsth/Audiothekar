using audiothek_client.Models;

public class Data
{
    public ProgramSets programSets { get; set; }
    public ProgramSetByNodeId programSetByNodeId { get; set; }
}



public class ProgramSets
{
    public List<Node> nodes { get; set; }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);



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