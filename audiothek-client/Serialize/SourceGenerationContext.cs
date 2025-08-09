using System.Text.Json.Serialization;
using GraphQL;

namespace audiothek_client.Serialize;

// Define a context for source generation
[JsonSerializable(typeof(Root))]
[JsonSerializable(typeof(ProgramSetByNodeId))]
[JsonSerializable(typeof(Items))]
[JsonSerializable(typeof(Audio))]
[JsonSerializable(typeof(List<Audio>))]
[JsonSerializable(typeof(ProgramSets))]
[JsonSerializable(typeof(Node))]
[JsonSerializable(typeof(List<Node>))]
[JsonSerializable(typeof(EditorialCategory))]
[JsonSerializable(typeof(Data))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(GraphQLRequest))]
[JsonSerializable(typeof(GraphQLResponse<Data>))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}