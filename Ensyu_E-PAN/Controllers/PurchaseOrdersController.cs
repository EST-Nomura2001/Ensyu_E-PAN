using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.Models.Order;
using System.Linq;
using System.Threading.Tasks;
using Ensyu_E_PAN.DTOs.Master;
using Ensyu_E_PAN.DTOs.Order;

namespace Ensyu_E_PAN.Controllers
{
    // PurchaseOrder（発注書）に関するAPIコントローラ
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrdersController : ControllerBase
    {
        // データベースへのアクセス用DbContext
        private readonly AnyDataDbContext _context;

        // コンストラクタ（DIでDbContextを受け取る）
        public PurchaseOrdersController(AnyDataDbContext context)
        {
            _context = context;
        }

        // =============================
        // 発注書1件取得API
        // GET: api/PurchaseOrders/{id}
        // =============================
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _context.Purchase_Orders
                .Include(o => o.OrderItemLists)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.Company)
                .Include(o => o.Store)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Not found" });
            }

            var dto = new PurchaseOrderDto
            {
                Id = order.Id,
                Title = order.Title,
                Quotation = order.Quotation,
                Tax = order.Tax,
                Order_Date = order.Order_Date,
                Delivery_Date = order.Delivery_Date,
                Payment_Date = order.Payment_Date,
                Payment_Terms = order.Payment_Terms,
                Confirm_Flg = order.Confirm_Flg,
                Manager = order.Manager,
                Other = order.Other,

                Company = new CompanyDto
                {
                    Id = order.Company.Id,
                    C_Name = order.Company.C_Name,
                    Address1 = order.Company.Address1,
                    Address2 = order.Company.Address2,
                    Post_Code = order.Company.Post_Code,
                    Mail = order.Company.Mail,
                    Tel = order.Company.Tel?.ToString(),
                    Fax = order.Company.Fax?.ToString()
                },

                Store = new StoreDto
                {
                    Id = order.Store.Id,
                    C_Name = order.Store.C_Name,
                    Address1 = order.Store.Address1,
                    Address2 = order.Store.Address2,
                    Post_Code = order.Store.Post_Code,
                    Mail = order.Store.Mail,
                    Tel = order.Store.Tel?.ToString(),
                    Fax = order.Store.Fax?.ToString()
                },

                OrderItems = order.OrderItemLists.Select(item => new OrderItemDto
                {
                    Item_Cd = item.Item_Cd,
                    Item_Name = item.Item?.Item_Name ?? "(未設定)",
                    Amount = item.Amount,
                    Other_ItemName = item.Other_ItemName
                }).ToList()
            };

            return Ok(new { success = true, data = dto });
        }

        // =============================
        // 発注書新規作成API
        // POST: api/PurchaseOrders
        // =============================
        [HttpPost("newOrder")]
        public async Task<IActionResult> PostNewOrder([FromBody] PurchaseOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "不正なデータです。" });

            // PurchaseOrderエンティティにマッピング
            var newOrder = new PurchaseOrder
            {
                Title = dto.Title,
                Quotation = dto.Quotation,
                Tax = dto.Tax,
                Order_Date = dto.Order_Date,
                Delivery_Date = dto.Delivery_Date,
                Payment_Date = dto.Payment_Date,
                Payment_Terms = dto.Payment_Terms,
                Confirm_Flg = dto.Confirm_Flg,
                Manager = dto.Manager,
                Other = dto.Other,
                Company_Cd = dto.Company.Id,
                Store_Cd = dto.Store.Id,
                OrderItemLists = new List<OrderItemList>() // 明細があればここで生成
            };

            // 明細（OrderItemList）も追加
            if (dto.OrderItems != null && dto.OrderItems.Count > 0)
            {
                foreach (var itemDto in dto.OrderItems)
                {
                    var item = new OrderItemList
                    {
                        Item_Cd = itemDto.Item_Cd,
                        Amount = itemDto.Amount,
                        Other_ItemName = itemDto.Other_ItemName
                    };
                    newOrder.OrderItemLists.Add(item);
                }
            }

            _context.Purchase_Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "発注書を登録しました", id = newOrder.Id });
        }

        // =============================
        // 発注書更新API
        // PUT: api/PurchaseOrders/{id}
        // =============================
        [HttpPut("updateOrder/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PurchaseOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "無効なデータです。" });

            var existing = await _context.Purchase_Orders
                .Include(po => po.OrderItemLists)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (existing == null)
                return NotFound(new { success = false, message = "発注書が見つかりません。" });

            // 発注書本体を更新
            existing.Title = dto.Title;
            existing.Quotation = dto.Quotation;
            existing.Tax = dto.Tax;
            existing.Order_Date = dto.Order_Date;
            existing.Delivery_Date = dto.Delivery_Date;
            existing.Payment_Date = dto.Payment_Date;
            existing.Payment_Terms = dto.Payment_Terms;
            existing.Confirm_Flg = dto.Confirm_Flg;
            existing.Manager = dto.Manager;
            existing.Other = dto.Other;
            existing.Company_Cd = dto.Company.Id;
            existing.Store_Cd = dto.Store.Id;

            // ========== 差分明細処理 ==========

            var dtoItems = dto.OrderItems ?? new List<OrderItemDto>();
            var dbItems = existing.OrderItemLists.ToList();

            // 削除：DTOに存在しないID
            var toRemove = dbItems
                .Where(dbItem => !dtoItems.Any(d => d.Id == dbItem.Id))
                .ToList();
            _context.Order_Item_Lists.RemoveRange(toRemove);

            // 更新または追加
            foreach (var itemDto in dtoItems)
            {
                var target = dbItems.FirstOrDefault(d => d.Id == itemDto.Id);
                if (target != null)
                {
                    // 更新
                    target.Item_Cd = itemDto.Item_Cd;
                    target.Amount = itemDto.Amount;
                    target.Other_ItemName = itemDto.Other_ItemName;
                }
                else
                {
                    // 追加
                    var newItem = new OrderItemList
                    {
                        P_Order_List_Id = existing.Id,
                        Item_Cd = itemDto.Item_Cd,
                        Amount = itemDto.Amount,
                        Other_ItemName = itemDto.Other_ItemName
                    };
                    existing.OrderItemLists.Add(newItem);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "発注書と明細を更新しました。" });
        }

        // =============================
        // 発注書一覧取得API
        // GET: /api/orders
        // =============================
        [HttpGet("/api/orders")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _context.Purchase_Orders
                .Include(o => o.Company)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            var dtoList = orders.Select(o => new PurchaseOrderDto
            {
                Id = o.Id,
                Title = o.Title,
                Quotation = o.Quotation,
                Tax = o.Tax,
                Order_Date = o.Order_Date,
                Delivery_Date = o.Delivery_Date,
                Payment_Date = o.Payment_Date,
                Payment_Terms = o.Payment_Terms,
                Confirm_Flg = o.Confirm_Flg,
                Manager = o.Manager,
                Other = o.Other,

                Company = new CompanyDto
                {
                    Id = o.Company.Id,
                    C_Name = o.Company.C_Name,
                    Address1 = o.Company.Address1,
                    Address2 = o.Company.Address2,
                    Post_Code = o.Company.Post_Code,
                    Mail = o.Company.Mail,
                    Tel = o.Company.Tel?.ToString(),
                    Fax = o.Company.Fax?.ToString()
                },

                Store = null,              // 一覧では不要な場合は null または省略
                OrderItems = null          // 詳細のみ必要なら null でOK
            }).ToList();

            return Ok(new { success = true, data = dtoList });
        }

        // =============================
        // 商品行追加API
        // POST: api/PurchaseOrders/{id}/items
        // =============================
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] OrderItemList item)
        {
            // 対象の発注書を取得
            var order = await _context.Purchase_Orders
                .Include(o => o.OrderItemLists)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // 商品行を追加
            order.OrderItemLists.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }
    }
}
