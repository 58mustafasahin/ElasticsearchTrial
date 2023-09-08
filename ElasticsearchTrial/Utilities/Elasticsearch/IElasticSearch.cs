using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using ElasticsearchTrial.Utilities.Elasticsearch.Models;
using ElasticsearchTrial.Utilities.Paging;
using ElasticsearchTrial.Utilities.Results;

namespace ElasticsearchTrial.Utilities.Elasticsearch;

public interface IElasticSearch
{
    Task<IReadOnlyDictionary<IndexName, IndexState>> GetIndexList();
    Task<List<string>> GetIndexNameList();
    ElasticsearchClient GetElasticClient(string indexName);

    Task<Results.IResult> CreateIndexAsync(IndexModel indexModel);
    Task<Results.IResult> DeleteIndexAsync(string indexName);
    Task<Results.IResult> AddAsync<T>(string indexName, T entity);
    Task<Results.IResult> DeleteAsync(string indexName, Id id);

    Task<Results.IResult> BulkAddAsync<T>(string indexName, List<T> list);
    Task<Results.IResult> BulkDeleteAsync<T>(string indexName, List<T> list);

    Task<Results.IResult> BulkUpdateAsync<T, TDto>(string indexName, string field, string searchValue, TDto dto) where T : class where TDto : class;
    Task<Results.IResult> BulkUpdateAllIndexAsync<T, TDto>(string field, string searchValue, TDto dto) where T : class where TDto : class;

    Task<IDataResult<PagedList<ElasticSearchGetModel<T>>>> GetSearch<T>(SearchByQueryParameters queryParameters)
        where T : class;
}