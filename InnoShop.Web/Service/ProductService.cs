using InnoShop.Web.Models;
using InnoShop.Web.Service.IService;
using InnoShop.Web.Utility;

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

			//https://localhost:7002/api/product/GetFilteredData?minPrice=30&maxPrice=500
			//{https://localhost:7002/api/product/GetFilteredData?minPrice=30&maxPrice=500}
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
	}
}
