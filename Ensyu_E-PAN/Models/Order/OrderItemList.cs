using Ensyu_E_PAN.Models.Masters;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.Models.Order
{
    public class OrderItemList
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("PurchaseOrder")]
        public int P_Order_List_Id { get; set; }

        [Required]
        [ForeignKey("Item")]
        public int Item_Cd { get; set; }

        [Required]
        public int Amount { get; set; }

        // ナビゲーションプロパティ
        public PurchaseOrder PurchaseOrder { get; set; }
        public Item Item { get; set; }

    }
}
