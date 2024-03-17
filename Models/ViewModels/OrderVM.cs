using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class OrderVM
    {
       public IEnumerable< Test> TestList { get; set; }
   
       public Test Test{ get; set; }
       public Orders Orders{ get; set; }
      
       public ApplicationUser User { get; set; }


    }
}
