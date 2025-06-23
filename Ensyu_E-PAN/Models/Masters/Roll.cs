using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensyu_E_PAN.Models.Masters
{
    public class Roll
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        //ナビゲーションプロパティ
        public ICollection<User> Users { get; set; }
    }
}
