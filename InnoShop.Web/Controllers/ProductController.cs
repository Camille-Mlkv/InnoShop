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

        public IActionResult CreateProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductDTO product)
        {
            
            product.UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            product.Date = DateTime.Now;
            var response=await _productService.CreateProductAsync(product);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product created successfully";
                return RedirectToAction("ClientProductIndex", new { clientId = product.UserId });
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View();
        }

        public async Task<IActionResult> DeleteProduct(int productId)
        {

            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var response=await _productService.DeleteProductAsync(productId);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product deleted successfully";
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return RedirectToAction("ClientProductIndex", new { clientId = userId});
        }

        public async Task<IActionResult> EditProduct(int productId)
        {
            var response=await _productService.GetProductByIdAsync(productId);
            if (response != null && response.IsSuccess)
            {
                ProductDTO? model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductDTO product)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            product.UserId = userId;
            product.Date = DateTime.Now;

            var response=await _productService.UpdateProductAsync(product);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("ClientProductIndex", new { clientId = userId });
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(product);
        }
    }
}
