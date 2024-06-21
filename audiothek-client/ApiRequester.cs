using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace audiothek_client
{
    public class ApiRequester
    {
        private const string ApiUrl = "https://api.ardaudiothek.de/graphql";

        readonly GraphQLHttpClient _graphQlClient = new GraphQLHttpClient(ApiUrl, new SystemTextJsonSerializer());

        GraphQLRequest AllProgramSetsRequest = new GraphQLRequest
        {
            Query = @"
            {
              programSets {nodes {title, numberOfElements, nodeId, rowId, editorialCategory{title, id}, lastItemAdded}}
            }"
        };

        private GraphQLRequest ProgramSetByNodeIdRequest(string nodeId)
        {
            return new GraphQLRequest
            {
                Query =
                    $"{{ programSetByNodeId(nodeId:\"{nodeId}\") {{ rowId, items{{nodes{{ title, audios{{url, downloadUrl, allowDownload}}, assetId, isPublished, publishDate, episodeNumber, summary, description, duration}}}}}}}}"
            };
        }

        public async Task<IEnumerable<Node>> GetAllProgramSets()
        {
            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(AllProgramSetsRequest);
            return graphQlResponse.Data.programSets.nodes.Where(x => x.numberOfElements != null);
        }

        public async Task<IEnumerable<Node>> GetFilesByNodeId(string nodeId)
        {
            GraphQLRequest query = ProgramSetByNodeIdRequest(nodeId);
            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(query);
            return graphQlResponse.Data.programSetByNodeId.items.nodes;
        }

        public async Task DownloadAllFilesFromNodes(IEnumerable<Node> nodes, string parentTitle, string path)
        {
            string outputDir = Path.Combine(path, MakeValidFileName(parentTitle));
            foreach (var node in nodes)
            {
                await Download(node, outputDir);
            }
        }

        public async Task Download(Node node, string outputDir)
        {
            int i = 0;
            var audios = node.audios.Where(x => x.downloadUrl != null);
            foreach (var audio in audios)
            {
                i++;
                string downloadUrl = audio.downloadUrl!;
                if (string.IsNullOrEmpty(downloadUrl) || string.IsNullOrEmpty(node.title))
                    continue;
                string partNumberInFilename = audios.Count() > 1 ? $" ({i})" : string.Empty;
                string filename = $"{MakeValidFileName(node.title)}{partNumberInFilename}.mp3";
                await Download(downloadUrl, Path.Combine(outputDir, filename));
            }
        }

        private async Task Download(string? downloadUrl, string filePath)
        {
            string? dirPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(dirPath);
            var uri = new Uri(downloadUrl);
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(200);
            using (var s = await httpClient.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    await s.CopyToAsync(fs);
                }
            }
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars =
                System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "-");
        }
    }
}