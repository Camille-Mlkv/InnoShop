using AutoMapper;
using InnoShop.Services.ProductAPI.Models;
using InnoShop.Services.ProductAPI.Models.DTO;

namespace InnoShop.Services.ProductAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Product, ProductDTO>();
                config.CreateMap<ProductDTO, Product>();
            });
            return mappingConfig;
        }
    }
}
