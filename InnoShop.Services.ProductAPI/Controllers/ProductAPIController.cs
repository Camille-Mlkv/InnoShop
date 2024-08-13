﻿using InnoShop.Services.ProductAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InnoShop.Services.ProductAPI.Models;
using InnoShop.Services.ProductAPI.Models.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace InnoShop.Services.ProductAPI.Controllers
{
    [Route("api/[controller]")]
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
                IEnumerable<Product> objList= _db.Products.ToList();
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

        [HttpPut]
        public ResponseDTO Put([FromBody] ProductDTO productDTO)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDTO);
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

        [HttpDelete]
        public ResponseDTO Delete(int id)
        {
            try
            {
                Product obj=_db.Products.First(p=>p.ProductId == id);
                _db.Products.Remove(obj);
                _db.SaveChanges();

               
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
