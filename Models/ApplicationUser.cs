using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public string Role { get; set; }
        public string Address { get; set; }
        public string CurStatus { get; set; } = SD.Active;
        [Required]
        public DateTime CreatedOnUTC { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOnUTC { get; set; }
    }
}
