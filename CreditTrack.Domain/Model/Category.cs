using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Domain.Model
{
  public  class Category
    {

        public int Id { get; set; }           // Primary Key
        public string Name { get; set; }      // Category Name

        // Navigation property (Products in this category)
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
