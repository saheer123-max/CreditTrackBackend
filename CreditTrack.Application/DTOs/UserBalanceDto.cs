using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Application.DTOs
{
   public class UserBalanceDto
    {
        public int UserId { get; set; }
        public decimal Balance { get; set; }
    }
}
