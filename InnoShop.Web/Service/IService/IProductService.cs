using InnoShop.Web.Models;

namespace InnoShop.Web.Service.IService
{
    public interface IProductService
    {
        Task<ResponseDTO?> GetAllProductsAsync();
        Task<ResponseDTO?> GetProductByIdAsync(int productId);
        Task<ResponseDTO?> GetFilteredProductsAsync(double? minPrice, double? maxPrice, bool? isAvailable);
        Task<ResponseDTO?> FindProductByNameAsync(string? productName);
        Task<ResponseDTO?> GetProductsByClientIdAsync(string clientId);
        Task<ResponseDTO?> CreateProductAsync(ProductDTO product);
        Task<ResponseDTO?> DeleteProductAsync(int productId);
        Task<ResponseDTO?> UpdateProductAsync(ProductDTO product);
    }
}
