using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensyu_E_PAN.Models.Masters
{
    public class Store
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string C_Name { get; set; }

        [Required]
        public int Post_Code { get; set; }

        [Required]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public int? Tel { get; set; }

        public int? Fax { get; set; }

        public string Mail { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
