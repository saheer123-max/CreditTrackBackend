using CreditTrack.Application.DTOs;
using CreditTrack.Application.Service;
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
    }
};