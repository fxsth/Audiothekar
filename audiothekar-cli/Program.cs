using audiothek_client;
using Spectre.Console;

namespace audiothekar_cli;

public static class Program
{
    public static async Task Main(string[] args)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Audiothekar").Color(Color.Blue));
        ApiRequester apiRequester = new ApiRequester();
        IEnumerable<Node> nodesOfAllCategories = (await apiRequester.GetAllProgramSets()).ToList();
        IEnumerable<IGrouping<string, Node>> categories = nodesOfAllCategories.Select(x =>
        {
            x.editorialCategory ??= new EditorialCategory() { id = "", title = "Andere" };
            return x;
        }).GroupBy(x => x.editorialCategory!.title).ToList();
        bool downloadSelected = false;
        Node? selectedNode = null;
        List<Node> children = new List<Node>();
        while (!downloadSelected)
        {
            string selectedCategory = SelectCategory(categories);

            selectedNode = SelectSeries(categories.Single(x => x.Key == selectedCategory));

            var items = await apiRequester.GetFilesByNodeId(selectedNode.nodeId);
            children = FilterDownloadableNodes(items);
            if (!children.Any())
            {
                AnsiConsole.Write("No downloads available");
                Console.ReadKey();
            }
            else
            {
                AnsiConsole.Write(CreateTreeFromNodes(selectedNode, children));
                downloadSelected = AnsiConsole.Confirm("Download starten?");
            }

            if (!downloadSelected)
                AnsiConsole.Clear();
        }

        await DownloadAllFilesFromNodes(apiRequester, children, selectedNode!.title);
    }

    private static string SelectCategory(IEnumerable<IGrouping<string, Node>> categories)
    {
        string selectedCategory = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Rubrik:")
                .PageSize(15)
                .AddChoices(categories.Select(x => x.Key).Order()));
        return selectedCategory;
    }

    private static Node SelectSeries(IEnumerable<Node> nodes)
    {
        Node selectedNode = AnsiConsole.Prompt(
            new SelectionPrompt<Node>()
                .Title("Reihe:")
                .PageSize(15)
                .AddChoices(nodes.OrderByDescending(x => x.lastItemAdded)));
        return selectedNode;
    }

    private static List<Node> FilterDownloadableNodes(IEnumerable<Node> nodes)
    {
        return nodes
            .Where(x => x.audios.Any(audio => audio.downloadUrl != null))
            .GroupBy(x => x.assetId)
            .Select(y => y.OrderByDescending(a => a.publishDate).First())
            .OrderBy(x => x.episodeNumber).ToList();
    }

    private static Tree CreateTreeFromNodes(Node root, IReadOnlyCollection<Node> children)
    {
        var tree = new Tree(root.editorialCategory!.title);

        var item = tree.AddNode(root.title);
        item.AddNode(new Text($"{children.Count} Items"));
        var table = new Table()
            .RoundedBorder()
            .AddColumn("Name")
            .AddColumn("Dauer");
        item.AddNode(table);
        foreach (var child in children)
        {
            table.AddRow(child.title, TimeSpan.FromSeconds(child.duration).ToString());
        }

        return tree;
    }

    public static async Task DownloadAllFilesFromNodes(ApiRequester apiRequester, IEnumerable<Node> nodes,
        string parentTitle)
    {
        string outputRootDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        string outputDir = Path.Combine(outputRootDir, MakeValidFileName(parentTitle));
        Console.WriteLine($"Download {parentTitle} nach {outputDir}");
        await AnsiConsole.Status().StartAsync("Downloading...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Star);
            ctx.SpinnerStyle(Style.Parse("green"));
            foreach (var node in nodes)
            {
                try
                {
                    ctx.Status = $"Downloading '{node.title}'";
                    await apiRequester.Download(node, outputDir);
                }
                catch (Exception e)
                {
                    AnsiConsole.Markup($"Download von '{node.title}' schlug fehl: {e.Message}");
                    if (!node.isPublished)
                        AnsiConsole.Markup($"Mögliche Ursache: Diese Resource ist (noch) nicht veröffentlicht.");
                    AnsiConsole.Markup($"Setze restliche Downloads fort...");
                }
            }
        });
        AnsiConsole.Markup(":check_mark_button: Completed.");
    }

    private static string MakeValidFileName(string name)
    {
        string invalidChars =
            System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "-");
    }
}