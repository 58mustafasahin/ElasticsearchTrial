using ElasticsearchTrial.Dtos;
using ElasticsearchTrial.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchTrial.Controllers;

public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return CreateActionResult(await _productService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        return CreateActionResult(await _productService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Save(CreateProductDto request)
    {
        return CreateActionResult(await _productService.SaveAsync(request));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProductDto request)
    {
        return CreateActionResult(await _productService.UpdateAsync(request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        return CreateActionResult(await _productService.DeleteAsync(id));
    }
}