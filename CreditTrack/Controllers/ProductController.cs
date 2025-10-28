using CreditTrack.Application.DTOs;
using CreditTrack.Application.Service;
using CreditTrack.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace CreditTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductController(ProductService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] ProductDto dto)
        {
            var response = await _service.AddProductAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _service.GetAllAsync();
            return Ok(products);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto productDto)
        {
           

            var response = await _service.UpdateProductAsync(id, productDto);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }





        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDeleteProduct(int id)
        {
            var response = await _service.SoftDeleteProductAsync(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }




        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            if (!response.Success) return NotFound(response);
            return Ok(response);
        }




        [HttpGet("catogory")]
        public async Task<IActionResult> GetAlls()
        {
            var response = await _service.GetAllProductsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("category/{id}")]
        public async Task<IActionResult> GetByCategory(int id)
        {
            var response = await _service.GetProductsByCategoryAsync(id);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
};