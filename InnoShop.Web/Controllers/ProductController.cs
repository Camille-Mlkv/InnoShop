using InnoShop.Web.Models;
using InnoShop.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace InnoShop.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        public async Task<IActionResult> ProductIndex()
        {
            List<ProductDTO>? list = new();

            ResponseDTO? response=await _productService.GetAllProductsAsync();
            if(response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(list);
        }

        public async Task<IActionResult> ProductFilter(double? minPrice, double? maxPrice, bool? isAvailable)
        {
			List<ProductDTO>? list = new();

			ResponseDTO? response = await _productService.GetFilteredProductsAsync(minPrice, maxPrice, isAvailable);
			if (response != null && response.IsSuccess)
			{
				list = JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Result));
			}
			else
			{
				TempData["error"] = response?.Message;
			}
			return View("ProductIndex", list);
		}

        public async Task<IActionResult> FindProduct(string? productName)
        {
            List<ProductDTO?> list = new();
			if (!string.IsNullOrEmpty(productName))
			{
				ResponseDTO? response = await _productService.FindProductByNameAsync(productName);
				if (response != null && response.IsSuccess)
				{
					list = JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Result));
				}
				else
				{
					TempData["error"] = response?.Message;
				}
			}
			
			return View("ProductIndex", list);
		}

        public async Task<IActionResult> ClientProductIndex(string clientId)
        {
            List<ProductDTO?> list = new();
            ResponseDTO? response = await _productService.GetProductsByClientIdAsync(clientId);
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            
            return View("ClientProductIndex",list);
        }
    }
}
