using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using ElasticsearchTrial.Utilities.Elasticsearch.Models;
using ElasticsearchTrial.Utilities.Paging;
using ElasticsearchTrial.Utilities.Results;

namespace ElasticsearchTrial.Utilities.Elasticsearch;

public class ElasticSearchManager : IElasticSearch
{
    private readonly ElasticsearchClientSettings _elasticsearchClientSettings;

    public ElasticSearchManager(IConfiguration configuration)
    {
        var elasticConfig = configuration.GetSection("ElasticSearchConfig").Get<ElasticSearchConfig>();
        _elasticsearchClientSettings = new ElasticsearchClientSettings(new Uri(elasticConfig.Url!)).Authentication(new BasicAuthentication(elasticConfig.UserName!, elasticConfig.Password!));
    }

    public async Task<IReadOnlyDictionary<IndexName, IndexState>> GetIndexList()
    {
        var client = new ElasticsearchClient(_elasticsearchClientSettings);
        var result = await client.Indices.GetAsync(new GetIndexRequest(Indices.All));
        return result.Indices;
    }

    public async Task<List<string>> GetIndexNameList()
    {
        var client = new ElasticsearchClient(_elasticsearchClientSettings);
        var result = await client.Indices.GetAsync(new GetIndexRequest(Indices.All));
        return result.Indices.Select(x => x.Key.ToString()).ToList();
    }

    public ElasticsearchClient GetElasticClient(string indexName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(indexName, "Index name cannot be null or empty ");
        }
        _elasticsearchClientSettings.DefaultIndex(indexName);
        return new ElasticsearchClient(_elasticsearchClientSettings);
    }

    public async Task<Results.IResult> CreateIndexAsync(IndexModel indexModel)
    {
        var client = GetElasticClient(indexModel.IndexName);
        var indices = await client.Indices.ExistsAsync(indexModel.IndexName);
        if (indices.Exists)
        {
            return new Results.Result(success: false, message: "Index already exists");
        }

        var response = await client.Indices.CreateAsync(indexModel.IndexName, se => se
              .Settings(a => a.NumberOfReplicas(indexModel.NumberOfReplicas).NumberOfShards(indexModel.NumberOfShards))
        );

        return new Results.Result(response.IsSuccess(), response.IsSuccess() ? "Success" : response.ElasticsearchServerError.Error.Reason);
    }

    public async Task<Results.IResult> DeleteIndexAsync(string indexName)
    {
        var client = GetElasticClient(indexName);

        var indices = await client.Indices.ExistsAsync(indexName);
        if (!indices.Exists)
        {
            return new Results.Result(success: false, message: "Index not found");
        }
        await client.Indices.DeleteAsync(indexName);
        return new Results.Result(success: true, message: "Index deleted");
    }

    public async Task<Results.IResult> AddAsync<T>(string indexName, T entity)
    {
        var client = GetElasticClient(indexName);

        var result = await client.IndexAsync<T>(entity);
        if (!result.IsSuccess())
        {
            return new Results.Result(result.IsSuccess());
        }
        return new Results.Result(result.IsSuccess());
    }

    public async Task<Results.IResult> DeleteAsync(string indexName, Id id)
    {
        var client = GetElasticClient(indexName);

        var response = await client.DeleteAsync(indexName, id);
        if (!response.IsSuccess())
        {
            return new Results.Result(success: false, message: "Document not deleted");
        }
        return new Results.Result(success: true, message: "Document deleted");
    }

    public async Task<Results.IResult> BulkAddAsync<T>(string indexName, List<T> list)
    {
        var client = GetElasticClient(indexName);

        var result = await client.BulkAsync(x => x.Index(indexName).IndexMany(list));
        if (!result.IsSuccess())
        {
            return new Results.Result(result.IsSuccess());
        }
        return new Results.Result(result.IsSuccess());
    }

    public async Task<Results.IResult> BulkDeleteAsync<T>(string indexName, List<T> list)
    {
        var client = GetElasticClient(indexName);

        var response = await client.BulkAsync(x => x.Index(indexName).DeleteMany(list));
        if (!response.IsSuccess())
        {
            return new Results.Result(success: false, message: "Document not deleted");
        }
        return new Results.Result(success: true, message: "Document deleted");
    }

    public async Task<Results.IResult> BulkUpdateAsync<T, TDto>(string indexName, string field, string searchValue, TDto dto) where T : class where TDto : class
    {
        var client = GetElasticClient(indexName);
        var met = await GetIdListAsync<T>(indexName, field, searchValue);
        if (met.Success)
        {
            foreach (var item in met.Data)
            {
                await client.UpdateAsync<T, TDto>(indexName, item, x => x.Doc(dto));
            }
        }
        return new Results.Result(success: true);
    }

    public async Task<Results.IResult> BulkUpdateAllIndexAsync<T, TDto>(string field, string searchValue, TDto dto) where T : class where TDto : class
    {
        var indexList = await GetIndexNameList();
        foreach (var indexName in indexList)
        {
            var client = GetElasticClient(indexName);
            var met = await GetIdListAsync<T>(indexName, field, searchValue);
            if (met.Success)
            {
                foreach (var item in met.Data)
                {
                    await client.UpdateAsync<T, TDto>(indexName, item, x => x.Doc(dto));
                }
            }
        }
        return new Results.Result(true);
    }

    public async Task<IDataResult<List<string>>> GetIdListAsync<T>(string indexName, string field, string searchValue) where T : class
    {
        var client = GetElasticClient(indexName);
        var searchReponse = await client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Match(m => m
                        .Field(field)
                        .Query(searchValue)
                )));

        var result = searchReponse.Hits.Select(d => d.Id.ToString()).ToList();
        if (result.Count == 0)
            return new ErrorDataResult<List<string>>("not found");

        return new SuccessDataResult<List<string>>(result);
    }

    public async Task<IDataResult<PagedList<ElasticSearchGetModel<T>>>> GetSearch<T>(SearchByQueryParameters queryParameters) where T : class
    {
        var client = GetElasticClient(queryParameters.IndexName);

        var pageFrom = (queryParameters.From == 0 ? 1 : queryParameters.From - 1) * queryParameters.Size;

        var searchResponse = await client.SearchAsync<T>(s => s
            .Index(queryParameters.IndexName)
            .From(pageFrom)
            .Size(queryParameters.Size)
            .Query(q => q
                .Bool(b => b
                    .Should(s => s
                        .MultiMatch(mm => mm
                            .Query(queryParameters.Query)
                            .Fields(queryParameters.Fields)
                            .Type(TextQueryType.PhrasePrefix))
                ))));

        var list = searchResponse.Hits.Select(x => new ElasticSearchGetModel<T>()
        {
            ElasticId = x.Id,
            Score = x.Score,
            Item = x.Source
        }).ToList();

        var pageList = new PagedList<ElasticSearchGetModel<T>>(list, (int)searchResponse.Total, pageFrom, queryParameters.Size);

        return new SuccessDataResult<PagedList<ElasticSearchGetModel<T>>>(pageList);
    }
}