using Elastic.Clients.Elasticsearch;
using ElasticsearchTrial.Constants;
using ElasticsearchTrial.Dtos;
using ElasticsearchTrial.Repositories;
using System.Net;

namespace ElasticsearchTrial.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;
    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ResponseDto<List<ProductDto>>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var productListDto = new List<ProductDto>();

        foreach (var x in products)
        {
            if (x.Feature is null)
            {
                productListDto.Add(new ProductDto(x.Id, x.Name, x.Price, x.Stock, null));

                continue;
            }
            productListDto.Add(new ProductDto(x.Id, x.Name, x.Price, x.Stock, new ProductFeatureDto(x.Feature.Width, x.Feature.Height, x.Feature.Color.ToString())));
        }
        return ResponseDto<List<ProductDto>>.Success(productListDto, HttpStatusCode.OK);
    }

    public async Task<ResponseDto<ProductDto>> GetByIdAsync(string id)
    {
        var hasProduct = await _productRepository.GetByIdAsync(id);

        if (hasProduct is null)
        {
            return ResponseDto<ProductDto>.Fail(Messages.ProductNotFound, HttpStatusCode.NotFound);
        }
        var productDto = hasProduct.CreateDto();
        return ResponseDto<ProductDto>.Success(productDto, HttpStatusCode.OK);
    }

    public async Task<ResponseDto<ProductDto>> SaveAsync(CreateProductDto createProductDto)
    {
        var responseProduct = await _productRepository.SaveAsync(createProductDto.CreateProduct());
        if (responseProduct is null)
        {
            return ResponseDto<ProductDto>.Fail(new List<string> { Messages.AnErrorOccured }, HttpStatusCode.InternalServerError);
        }
        return ResponseDto<ProductDto>.Success(responseProduct.CreateDto(), HttpStatusCode.Created);
    }

    public async Task<ResponseDto<bool>> UpdateAsync(UpdateProductDto updateProductDto)
    {
        var isSuccess = await _productRepository.UpdateSynch(updateProductDto);

        if (!isSuccess)
        {
            return ResponseDto<bool>.Fail(new List<string> { Messages.AnErrorOccured }, HttpStatusCode.InternalServerError);
        }
        return ResponseDto<bool>.Success(true, HttpStatusCode.NoContent);
    }

    public async Task<ResponseDto<bool>> DeleteAsync(string id)
    {
        var deleteResponse = await _productRepository.DeleteAsync(id);

        if (!deleteResponse.IsValidResponse && deleteResponse.Result == Result.NotFound)
        {
            return ResponseDto<bool>.Fail(new List<string> { Messages.ProductNotFound }, HttpStatusCode.NotFound);
        }

        if (!deleteResponse.IsValidResponse)
        {
            deleteResponse.TryGetOriginalException(out Exception? exception);
            _logger.LogError(exception, deleteResponse.ElasticsearchServerError?.Error.ToString());

            return ResponseDto<bool>.Fail(new List<string> { Messages.AnErrorOccured }, HttpStatusCode.InternalServerError);
        }
        return ResponseDto<bool>.Success(true, HttpStatusCode.NoContent);
    }
}
