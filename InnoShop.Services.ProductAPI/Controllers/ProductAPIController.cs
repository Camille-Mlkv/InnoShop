using InnoShop.Services.ProductAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InnoShop.Services.ProductAPI.Models;
using InnoShop.Services.ProductAPI.Models.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InnoShop.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDTO _response;
        private IMapper _mapper;
        public ProductAPIController(AppDbContext db,IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ResponseDTO();
        }

        [HttpGet]
        public ResponseDTO Get()
        {
            try
            {
                IEnumerable<Product> objList = _db.Products.ToList();
                _response.Result = _mapper.Map<IEnumerable<ProductDTO>>(objList);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;

            }
            return _response;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDTO Get(int id)
        {
            try 
            {
                Product obj = _db.Products.First(p=>p.ProductId==id);
                _response.Result = _mapper.Map<ProductDTO>(obj); //conversion
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("{userId}")]
        public ResponseDTO Get(string userId)
        {
            try
            {
                IEnumerable<Product> objList = _db.Products.Where(p => p.UserId == userId).ToList();
                _response.Result = _mapper.Map<IEnumerable<ProductDTO>>(objList);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;

            }
            return _response;
        }


        [HttpGet("GetFilteredData")]
        public ResponseDTO GetFilteredData([FromQuery] double? minPrice, [FromQuery] double? maxPrice, [FromQuery] bool? isAvailable)
        {
            try
            {
                var query = _db.Products.AsQueryable();

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                if (isAvailable.HasValue)
                {
                    query = query.Where(p => p.IsAvailable == isAvailable.Value);
                }

                IEnumerable<Product> objList = query.ToList();
                _response.Result = _mapper.Map<IEnumerable<ProductDTO>>(objList);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [Authorize]
        [HttpPost]
        public ResponseDTO Post([FromBody] ProductDTO productDTO)
        {
            try
            {
                Product product=_mapper.Map<Product>(productDTO);
                _db.Products.Add(product);
                _db.SaveChanges();

                _response.Result = _mapper.Map<ProductDTO>(product); //conversion
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [Authorize]
        [HttpPut]
        public ResponseDTO Put([FromBody] ProductDTO productDTO)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Product product = _mapper.Map<Product>(productDTO);

                if (product.UserId != userId)
                {
                    _response.IsSuccess = false;
                    _response.Message = "You do not have permission to modify this product.";
                    return _response;
                }

                _db.Products.Update(product);
                _db.SaveChanges();

                _response.Result = _mapper.Map<ProductDTO>(product); //conversion
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [Authorize]
        [HttpDelete]
        public ResponseDTO Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Получаем ID текущего пользователя
                var product = _db.Products.FirstOrDefault(p => p.ProductId == id);

                if (product == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Product not found";
                    return _response;
                }

                if (product.UserId != userId)
                {
                    _response.IsSuccess = false;
                    _response.Message = "You do not have permission to delete this product.";
                    return _response;
                }

                _db.Products.Remove(product);
                _db.SaveChanges();

                _response.IsSuccess = true;
                _response.Message = "Product deleted successfully.";

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        
    }
}
