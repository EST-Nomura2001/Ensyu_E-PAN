using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ensyu_E_PAN.Models.Order;
using Ensyu_E_PAN.Models.Attendance;

namespace Ensyu_E_PAN.Models.Masters
{
    public class Store
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string C_Name { get; set; }

        [Required]
        public string Post_Code { get; set; }

        [Required]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string? Tel { get; set; }

        public string? Fax { get; set; }

        public string Mail { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<AllShift> AllShifts { get; set; }
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
