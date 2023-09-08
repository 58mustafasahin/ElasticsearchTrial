namespace ElasticsearchTrial.Utilities.Elasticsearch.Models;

public class IndexModel
{
    public string IndexName { get; set; } = null!;
    public int NumberOfReplicas { get; set; } = 3;
    public int NumberOfShards { get; set; } = 3;
}