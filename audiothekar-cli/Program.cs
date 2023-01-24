using audiothek_client;
using Spectre.Console;

AnsiConsole.Clear();
AnsiConsole.Write(new FigletText("Audiothekar").Color(Color.Blue));
ApiRequester apiRequester = new ApiRequester();
IEnumerable<Node> nodesOfAllCategories = (await apiRequester.GetAllProgramSets()).ToList();
IEnumerable<IGrouping<string, Node>> categories = nodesOfAllCategories.Select(x =>
{
    if (x.editorialCategory == null)
        x.editorialCategory = new EditorialCategory() { id = "", title = "Andere" };
    return x;
}).GroupBy(x => x.editorialCategory.title);
bool downloadSelected = false;
Node selectedNode = null;
string selectedCategory = null;
while (!downloadSelected)
{
    selectedCategory = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Rubrik:")
            .PageSize(15)
            .AddChoices(categories.Select(x => x.Key)));

    selectedNode = AnsiConsole.Prompt(
        new SelectionPrompt<Node>()
            .Title("Reihe:")
            .PageSize(15)
            .AddChoices(categories.Single(x => x.Key == selectedCategory).OrderBy(x => x.title)));

    var nodesByNodeId = await apiRequester.GetFilesByNodeId(selectedNode.nodeId);
    var nodesWithDownloadUrl = nodesByNodeId.Where(x => x.audios.FirstOrDefault()?.downloadUrl != null).ToList();
    if (!nodesWithDownloadUrl.Any())
    {
        AnsiConsole.Write("No downloads available");
        Console.ReadKey();
    }
    else
    {
        AnsiConsole.Write(new Rows(nodesWithDownloadUrl.Select(x => new Text(x.title))));
        downloadSelected = AnsiConsole.Confirm("Download starten?");
    }

    if (!downloadSelected)
        AnsiConsole.Clear();
}

string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
Task downloadTask = apiRequester.DownloadAllFilesFromNode(selectedNode, path);
Console.WriteLine($"Download {selectedNode.title} nach {path}");
await AnsiConsole.Status().StartAsync("Downloading...", async ctx =>
{
    ctx.Spinner(Spinner.Known.Star);
    ctx.SpinnerStyle(Style.Parse("green"));
    await downloadTask;
});
Console.WriteLine("-------------");