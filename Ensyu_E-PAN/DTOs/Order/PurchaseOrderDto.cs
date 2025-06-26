using Ensyu_E_PAN.DTOs.Master;

namespace Ensyu_E_PAN.DTOs.Order
{
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Quotation { get; set; }
        public int? Tax { get; set; }
        public DateTime? Order_Date { get; set; }
        public DateTime? Delivery_Date { get; set; }
        public DateTime? Payment_Date { get; set; }
        public string Payment_Terms { get; set; }
        public bool Confirm_Flg { get; set; }
        public string Manager { get; set; }
        public string Other { get; set; }

        public CompanyDto Company { get; set; }
        public StoreDto Store { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }

    }
}
