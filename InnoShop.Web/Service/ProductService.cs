using InnoShop.Web.Models;
using InnoShop.Web.Service.IService;
using InnoShop.Web.Utility;
using System.Xml.Linq;

namespace InnoShop.Web.Service
{
    public class ProductService : IProductService
    {
        private readonly IBaseService _baseService;

        public ProductService(IBaseService baseService)
        {
            _baseService = baseService;
        }

		public async Task<ResponseDTO?> GetAllProductsAsync()
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                ApiType=SD.ApiType.GET,
                Url=SD.ProductAPIBase+"/api/product"

            });
        }

		public async Task<ResponseDTO?> GetFilteredProductsAsync(double? minPrice, double? maxPrice, bool? isAvailable)
		{
			var queryParameters = new List<string>();

			if (minPrice.HasValue)
			{
				queryParameters.Add($"minPrice={minPrice.Value}");
			}
			if (maxPrice.HasValue)
			{
				queryParameters.Add($"maxPrice={maxPrice.Value}");
			}
			if (isAvailable.HasValue)
			{
				queryParameters.Add($"isAvailable={isAvailable.Value}");
			}

			var queryString = string.Join("&", queryParameters);

			var url = SD.ProductAPIBase + "/api/product/GetFilteredData";
			if (!string.IsNullOrEmpty(queryString))
			{
				url += "?" + queryString;
			}

			return await _baseService.SendAsync(new RequestDTO
			{
				ApiType = SD.ApiType.GET,
				Url = url
			});
		}

		public async Task<ResponseDTO?> FindProductByNameAsync(string? productName)
		{
			var queryParameters = new List<string>();

			if (!string.IsNullOrEmpty(productName))
			{
				queryParameters.Add($"productName={productName}");
			}
			var queryString = string.Join("&", queryParameters);

			var url = SD.ProductAPIBase + "/api/product/FindByName";
			if (!string.IsNullOrEmpty(queryString))
			{
				url += "?" + queryString;
			}

			return await _baseService.SendAsync(new RequestDTO
			{
				ApiType = SD.ApiType.GET,
				Url = url

			});
		}

        public async Task<ResponseDTO?> GetProductsByClientIdAsync(string clientId)
        {
            var url = $"{SD.ProductAPIBase}/api/product/{clientId}";

            return await _baseService.SendAsync(new RequestDTO
            {
                ApiType = SD.ApiType.GET,
                Url = url
            });

        }
    }
}
