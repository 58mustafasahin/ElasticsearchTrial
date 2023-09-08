namespace ElasticsearchTrial.Utilities.Elasticsearch.Models;

public class ElasticSearchGetModel<T>
{
    public string ElasticId { get; set; }
    public double? Score { get; set; }
    public T Item { get; set; }
}
