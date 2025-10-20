
using Microsoft.AspNetCore.Http;

namespace CreditTrack.Application.DTOs
{
  public   class ProductDto
    {


        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }



    }
}
