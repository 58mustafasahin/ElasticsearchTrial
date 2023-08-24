using ElasticsearchTrial.Entities;
using ElasticsearchTrial.Enums;

namespace ElasticsearchTrial.Dtos;

public record CreateProductDto(string Name, decimal Price, int Stock, ProductFeatureDto Feature)
{
    public Product CreateProduct()
    {
        return new Product
        {
            Name = Name,
            Price = Price,
            Stock = Stock,
            Feature = new ProductFeature()
            {
                Width = Feature.Width,
                Height = Feature.Height,
                Color = (EColor)int.Parse(Feature.Color)
            }
        };
    }
}
