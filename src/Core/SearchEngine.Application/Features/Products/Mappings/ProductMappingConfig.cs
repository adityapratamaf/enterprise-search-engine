using SearchEngine.Application.Features.Products.DTOs;
using SearchEngine.Domain.Entities;
using Mapster;

namespace SearchEngine.Application.Features.Products.Mappings;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateProductRequest, Product>();

        config.NewConfig<UpdateProductRequest, Product>();

        config.NewConfig<Product, ReadProductResponse>();
    }
}
