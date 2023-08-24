using ElasticsearchTrial.Dtos;

namespace ElasticsearchTrial.Services;

public interface IProductService
{
    Task<ResponseDto<ProductDto>> SaveAsync(CreateProductDto createProductDto);
    Task<ResponseDto<List<ProductDto>>> GetAllAsync();
    Task<ResponseDto<ProductDto>> GetByIdAsync(string id);
    Task<ResponseDto<bool>> UpdateAsync(UpdateProductDto updateProductDto);
    Task<ResponseDto<bool>> DeleteAsync(string id);
}
