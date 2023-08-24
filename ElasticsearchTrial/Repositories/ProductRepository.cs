using ElasticsearchTrial.Constants;
using ElasticsearchTrial.Dtos;
using ElasticsearchTrial.Entities;
using Nest;
using System.Collections.Immutable;

namespace ElasticsearchTrial.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ElasticClient _client;
    public ProductRepository(ElasticClient client)
    {
        _client = client;
    }

    public async Task<ImmutableList<Product>> GetAllAsync()
    {
        var result = await _client.SearchAsync<Product>(s => s.Index(Messages.ProductIndexName).Query(q => q.MatchAll()));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;
        return result.Documents.ToImmutableList();
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        var response = await _client.GetAsync<Product>(id, x => x.Index(Messages.ProductIndexName));

        if (!response.IsValid)
        {
            return null;
        }
        response.Source.Id = response.Id;
        return response.Source;
    }

    public async Task<Product?> SaveAsync(Product newProduct)
    {
        newProduct.Created = DateTime.Now;

        var response = await _client.IndexAsync(newProduct, x => x.Index(Messages.ProductIndexName));

        //If you want to define id, you should add Id in the query as below.
        //var response = await _client.IndexAsync(newProduct, x => x.Index(indexName).Id(Guid.NewGuid().ToString()));

        if (!response.IsValid) return null;

        newProduct.Id = response.Id;

        return newProduct;
    }

    public async Task<bool> UpdateSynch(UpdateProductDto updateProductDto)
    {
        var response = await _client.UpdateAsync<Product, UpdateProductDto>(updateProductDto.Id, x =>
        x.Index(Messages.ProductIndexName).Doc(updateProductDto));

        return response.IsValid;
    }

    public async Task<DeleteResponse> DeleteAsync(string id)
    {
        var response = await _client.DeleteAsync<Product>(id, x => x.Index(Messages.ProductIndexName));
        return response;
    }
}
