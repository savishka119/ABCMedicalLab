using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models
{
    public class Orders
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime AppoinmentDateTime { get; set; }
        public double TotAmount { get; set; }
        public double TotPaid { get; set; }
        public string CurStatus { get; set; } = SD.Active;
        public string OrderStatus { get; set; }
        public string DoctorName { get; set; }
        public string PatientAge { get; set; }
        public int TestId { get; set; }
        [ForeignKey("TestId")]
        public Test Test { get; set; }
    }
}
