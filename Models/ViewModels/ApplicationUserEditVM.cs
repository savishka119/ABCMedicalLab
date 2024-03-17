using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class ApplicationUserEditVM
    {
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        [Display(Name = "User Groups")]
        public List<int> UserGroups { get; set; }

        public IEnumerable<SelectListItem> GroupList { get; set; }
    }
}