using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.Models.Masters
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Item_Name { get; set; }

    }
}
