using System.ComponentModel.DataAnnotations;
using Ensyu_E_PAN.Models.Order;

namespace Ensyu_E_PAN.Models.Masters
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Item_Name { get; set; }

        //ナビゲーション
        public ICollection<OrderItemList>? OrderItemLists { get; set; }
    }
}
