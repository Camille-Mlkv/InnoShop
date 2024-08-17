using InnoShop.Web.Models;

namespace InnoShop.Web.Service.IService
{
    public interface IProductService
    {
        Task<ResponseDTO?> GetAllProductsAsync();
        Task<ResponseDTO?> GetFilteredProductsAsync(double? minPrice, double? maxPrice, bool? isAvailable);
    }
}
