using audiothek_client;
using Spectre.Console;

Console.WriteLine("Starting...");
ApiRequester apiRequester = new ApiRequester();
IEnumerable<Node> nodes = await apiRequester.GetAllProgramSets();
bool downloadSelected = false;
Node selectedNode = null;
while (!downloadSelected)
{
    selectedNode = AnsiConsole.Prompt(
        new SelectionPrompt<Node>()
            .Title("Which one to download?")
            .AddChoices(nodes));

    var nodesByNodeId = await apiRequester.GetFilesByNodeId(selectedNode.nodeId);
    AnsiConsole.Write(new Rows(nodesByNodeId.Select(x => new Text(x.title))));

    downloadSelected = AnsiConsole.Confirm("Start downloading?");
    if(!downloadSelected)
        AnsiConsole.Clear();
}

string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
Task downloadTask = apiRequester.DownloadAllFilesByTitle(selectedNode.title, path);
Console.WriteLine("Downloading...");
await downloadTask;
Console.WriteLine("Finished.");