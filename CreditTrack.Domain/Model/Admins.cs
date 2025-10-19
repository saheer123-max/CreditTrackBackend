using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Domain.Model
{
 public   class Admin
    {
   public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "Admin";
        public int FailedAttempts { get; set; }
        public DateTime? LockoutUntil { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
