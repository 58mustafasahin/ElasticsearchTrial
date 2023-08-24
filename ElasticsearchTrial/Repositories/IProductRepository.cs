using ElasticsearchTrial.Dtos;
using ElasticsearchTrial.Entities;
using Nest;
using System.Collections.Immutable;

namespace ElasticsearchTrial.Repositories;

public interface IProductRepository
{
    Task<Product?> SaveAsync(Product newProduct);

    /// <summary>
    /// No one can change this list.
    /// </summary>
    /// <returns></returns>
    Task<ImmutableList<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(string id);
    Task<bool> UpdateSynch(UpdateProductDto updateProductDto);

    /// <summary>
    /// This method is discussed for error handling.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<DeleteResponse> DeleteAsync(string id);
}
