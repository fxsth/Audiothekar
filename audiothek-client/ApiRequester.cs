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
              programSets {nodes {title, numberOfElements, nodeId, rowId, editorialCategory{title}}}
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

        public async Task<IEnumerable<string>> GetAllProgramSets()
        {
            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(AllProgramSetsRequest);
            return graphQlResponse.Data.programSets.nodes
                .Where(s => s?.editorialCategory?.title?.Contains("Hörspiel")==true).Select(x => x.title);
        }

        public async Task DownloadAllFilesByTitle(string title, string path)
        {
            string outputDir =
                Path.Combine(path, MakeValidFileName(title));
            GraphQLRequest query = ProgramSetByNodeIdRequest(await TryGetNodeIdByTitle(title));
            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(query);
            foreach (var node in graphQlResponse.Data.programSetByNodeId.items.nodes)
            {
                string? downloadUrl = node.audios.FirstOrDefault()?.downloadUrl;
                string filename = node.title;
                if(string.IsNullOrEmpty(downloadUrl) || string.IsNullOrEmpty(filename))
                    continue;
                await Download(downloadUrl, Path.Combine(outputDir, MakeValidFileName(title) + ".mp3"));
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
        
        private static string MakeValidFileName( string name )
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape( new string( System.IO.Path.GetInvalidFileNameChars() ) );
            string invalidRegStr = string.Format( @"([{0}]*\.+$)|([{0}]+)", invalidChars );

            return System.Text.RegularExpressions.Regex.Replace( name, invalidRegStr, "-" );
        }
    }
}