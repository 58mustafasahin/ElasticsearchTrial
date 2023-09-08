namespace ElasticsearchTrial.Utilities.Elasticsearch.Models;

public class SearchParameters
{
    public string IndexName { get; set; }
    public int From { get; set; } = 1;
    public int Size { get; set; } = 10;
}
