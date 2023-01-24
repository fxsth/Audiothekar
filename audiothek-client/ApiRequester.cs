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
              programSets {nodes {title, numberOfElements, nodeId, rowId, editorialCategory{title, id}}}
            }"
        };

        private GraphQLRequest ProgramSetByNodeIdRequest(string nodeId)
        {
            return new GraphQLRequest
            {
                Query =
                    $"{{ programSetByNodeId(nodeId:\"{nodeId}\") {{ rowId, items{{nodes{{ title, audios{{downloadUrl}}}}}}}}}}"
            };
        }

        public async Task<IEnumerable<Node>> GetAllProgramSets()
        {
            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(AllProgramSetsRequest);
            return graphQlResponse.Data.programSets.nodes.Where(x=>x.numberOfElements != null);
        }

        public async Task<IEnumerable<Node>> GetFilesByNodeId(string nodeId)
        {
            GraphQLRequest query = ProgramSetByNodeIdRequest(nodeId);
            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(query);
            return graphQlResponse.Data.programSetByNodeId.items.nodes;
        }

        public async Task DownloadAllFilesFromNode(Node parentNode, string path)
        {
            string outputDir = Path.Combine(path, MakeValidFileName(parentNode.title));
            GraphQLRequest query = ProgramSetByNodeIdRequest(parentNode.nodeId);
            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(query);
            foreach (var node in graphQlResponse.Data.programSetByNodeId.items.nodes)
            {
                int i = 0;
                foreach (var audio in node.audios)
                {
                    i++;
                    string downloadUrl = audio.downloadUrl;
                    if(string.IsNullOrEmpty(downloadUrl) || string.IsNullOrEmpty(node.title))
                        continue;
                    string partNumberInFilename = node.audios.Count > 1 ? $" ({i})" : string.Empty;
                    string filename = $"{MakeValidFileName(node.title)}{partNumberInFilename}.mp3";
                    await Download(downloadUrl, Path.Combine(outputDir, filename));
                }
            }
        }

        private async Task<string> TryGetNodeIdByTitle(string title)
        {
            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(AllProgramSetsRequest);
            return graphQlResponse.Data.programSets.nodes.Where(x => x.title == title).Select(x => x.nodeId)
                .First();
        }

        private async Task Download(string? downloadUrl, string filePath)
        {
            try
            {
                string dirPath = Path.GetDirectoryName(filePath);
                string filename = Path.GetFileName(downloadUrl);
                string localFilename = Path.Combine(dirPath, filename);
                Directory.CreateDirectory(dirPath);
                var url = new Uri(downloadUrl);
                var httpClient = new HttpClient();
                await httpClient.GetByteArrayAsync(url).ContinueWith(data =>
                {
                    File.WriteAllBytes(localFilename, data.Result);
                });
            }
            catch (Exception e)
            {
                //ignored
            }
        }
        
        private static string MakeValidFileName( string name )
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape( new string( System.IO.Path.GetInvalidFileNameChars() ) );
            string invalidRegStr = string.Format( @"([{0}]*\.+$)|([{0}]+)", invalidChars );

            return System.Text.RegularExpressions.Regex.Replace( name, invalidRegStr, "-" );
        }
    }
}