using audiothek_client;
using audiothekar_cli.Models;
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
        Node? selectedNode = null;
        while (true)
        {
            var selectedCategory = SelectModeAndCategory(categories.Select(x => x.Key));

            if (selectedCategory.Value == Mode.Search)
                selectedNode = await LiveSearchAsync(nodesOfAllCategories);
            else
                selectedNode = SelectSeries(categories.Single(x => x.Key == selectedCategory.DisplayName));
            
            await SelectAndDownloadNodesBySeries(apiRequester, selectedNode);
            var keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.Q)
                break;
            AnsiConsole.Clear();
        }
    }

    private static async Task<bool> SelectAndDownloadNodesBySeries(ApiRequester apiRequester, Node selectedNode)
    {
        DownloadSelection downloadSelection = DownloadSelection.Cancel;
        var items = await apiRequester.GetFilesByNodeId(selectedNode.nodeId);
        var children = FilterDownloadableNodes(items);
        if (!children.Any())
        {
            AnsiConsole.Write("No downloads available");
            Console.ReadKey();
        }
        else
        {
            AnsiConsole.Write(CreateTreeFromNodes(selectedNode, children));
            var prompt = new SelectionPrompt<DownloadSelection>()
                .Title("Download Auswahl:")
                .AddChoices(new[] { DownloadSelection.All, DownloadSelection.Single, DownloadSelection.Cancel });
            prompt.Converter = selection =>
            {
                return selection switch
                {
                    DownloadSelection.All => $"Alle {children.Count} Folgen herunterladen",
                    DownloadSelection.Single => "Einzelne Folge herunterladen",
                    DownloadSelection.Cancel => "Abbrechen",
                    _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, null)
                };
            };
            downloadSelection = AnsiConsole.Prompt(prompt);
        }

        if (downloadSelection == DownloadSelection.Cancel)
        {
            AnsiConsole.Clear();
            return false;
        }

        if (downloadSelection == DownloadSelection.All)
        {
            await DownloadAllFilesFromNodes(apiRequester, children, selectedNode.title);
        }
        else if (downloadSelection == DownloadSelection.Single)
        {
            var selectionPrompt = new SelectionPrompt<Node>()
                .Title("Folge:")
                .PageSize(15)
                .AddChoices(children);
            selectionPrompt.Converter = selection => selection.title;
            var selectedChild = AnsiConsole.Prompt(selectionPrompt);
            await DownloadAllFilesFromNodes(apiRequester, new List<Node> { selectedChild }, selectedNode.title);
        }

        return true;
    }

    private static ValueRepresentation<Mode> SelectModeAndCategory(IEnumerable<string> categories)
    {
        var prompt = new SelectionPrompt<ValueRepresentation<Mode>>()
            .Title("Rubrik:")
            .PageSize(15);
        prompt.Converter = x => x.DisplayName;
        prompt.AddChoiceGroup(new ValueRepresentation<Mode>("Suchen", Mode.Search),
            new ValueRepresentation<Mode>("Suchen", Mode.Search));
        prompt.AddChoiceGroup(new ValueRepresentation<Mode>("Kategorien durchstöbern", Mode.Browse),
            categories.Select(x => new ValueRepresentation<Mode>(x, Mode.Browse)).Order().ToList());
        var selectedCategory = AnsiConsole.Prompt(prompt);
        return selectedCategory;
    }

    public static async Task<Node?> LiveSearchAsync(IEnumerable<Node> nodes)
    {
        var allNodes = nodes.ToList();
        string searchTerm = "";
        Node? selected = null;
        int selectedIndex = 0;

        await AnsiConsole.Live(new Table().AddColumn("Titel").AddColumn("Dauer"))
            .StartAsync(async ctx =>
            {
                while (selected == null)
                {
                    var filtered = allNodes
                        .Where(x => x.title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Auswahl-Index anpassen, falls nötig
                    if (selectedIndex >= filtered.Count)
                        selectedIndex = Math.Max(0, filtered.Count - 1);

                    var table = new Table().AddColumn("Titel").AddColumn("Dauer");
                    for (int i = 0; i < filtered.Count; i++)
                    {
                        var node = filtered[i];
                        if (i == selectedIndex)
                            table.AddRow($"[bold yellow]{node.title}[/]",
                                $"[bold yellow]{TimeSpan.FromSeconds(node.duration)}[/]");
                        else
                            table.AddRow(node.title, TimeSpan.FromSeconds(node.duration).ToString());
                    }

                    ctx.UpdateTarget(table);

                    AnsiConsole.MarkupLine($"[grey]Suchbegriff: {searchTerm}[/]");
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Backspace && searchTerm.Length > 0)
                    {
                        searchTerm = searchTerm[..^1];
                        selectedIndex = 0;
                    }
                    else if (key.Key == ConsoleKey.Enter && filtered.Any())
                    {
                        selected = filtered[selectedIndex];
                    }
                    else if (key.Key == ConsoleKey.UpArrow && selectedIndex > 0)
                    {
                        selectedIndex--;
                    }

                    else if (key.Key == ConsoleKey.DownArrow && selectedIndex < filtered.Count - 1)
                    {
                        selectedIndex++;
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        searchTerm += key.KeyChar;
                        selectedIndex = 0;
                    }

                    ctx.Refresh();
                    await Task.Delay(50);
                }
            });

        return selected;
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
        AnsiConsole.MarkupLine(":check_mark_button: Completed.");
    }

    private static string MakeValidFileName(string name)
    {
        string invalidChars =
            System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "-");
    }
}