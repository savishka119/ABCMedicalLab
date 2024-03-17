using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class ApplicationGroupVM
    {
        //public ApplicationGroup ApplicationGroup { get; set; }

        [Required]
        [Display(Name = "Group Permissions")]
        public List<string> UserPermissions { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; }

    }
}