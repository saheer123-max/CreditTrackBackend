using CreditTrack.Application.Commands.Products;
using CreditTrack.Application.DTOs;
using CreditTrack.Application.Queries.Products;
using CreditTrack.Domain.Model;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CreditTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }
      
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductDto dto)
        {
            var command = new CreateProductCommand(dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
      
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductDto dto)
        {
            var updatedProduct = await _mediator.Send(new UpdateProductCommand(id, dto));
            if (updatedProduct == null)
                return NotFound("Product not found!");

            return Ok(new { Message = "✅ Product Updated Successfully", updatedProduct });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _mediator.Send(new DeleteProductCommand(id));
            if (!success) return NotFound("Product not found!");
            return Ok(new { Message = "🗑️ Product Deleted Successfully" });
        }
     
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _mediator.Send(new GetProductByIdQuery(id));
            if (product == null) return NotFound("Product not found!");
            return Ok(product);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _mediator.Send(new GetProductsByCategoryQuery(categoryId));
            return Ok(products);
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _mediator.Send(new GetAllProductsQuery());
        }
    }
}
