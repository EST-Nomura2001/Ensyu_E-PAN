using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.Models.Masters
{
    public class WorkRoll
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
