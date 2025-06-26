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


        //[HttpGet("{id}")]
        //public async Task<IActionResult> Get(int id)
        //{
        //    // 発注書本体＋商品リスト＋会社・事業所情報も含めて取得
        //    var order = await _context.Purchase_Orders
        //        .Include(o => o.OrderItemLists) // 商品リストも一緒に取得
        //        .Include(o => o.Company)       // 会社情報も一緒に取得
        //        .Include(o => o.Store)         // 事業所情報も一緒に取得
        //        .FirstOrDefaultAsync(o => o.Id == id);

        //    // 見つからなければ404を返す
        //    if (order == null)
        //        return NotFound(new { success = false, message = "Not found" });

        //    // 成功時はデータを返す
        //    return Ok(new { success = true, data = order });
        //}

        // =============================
        // 発注書新規作成・更新API
        // POST: api/PurchaseOrders
        // =============================
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseOrder order)
        {
            // Idが0なら新規作成、0以外なら更新
            if (order.Id == 0)
            {
                // 新規作成
                _context.Purchase_Orders.Add(order);
            }
            else
            {
                // 更新処理
                var existing = await _context.Purchase_Orders
                    .Include(o => o.OrderItemLists)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (existing == null)
                    return NotFound(new { success = false, message = "Not found" });

                // 発注書本体のプロパティを更新
                _context.Entry(existing).CurrentValues.SetValues(order);

                // 商品リストを一度クリアしてから再登録
                existing.OrderItemLists.Clear();
                foreach (var item in order.OrderItemLists)
                {
                    existing.OrderItemLists.Add(item);
                }
            }
            // DBに保存
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = order });
        }

        // =============================
        // 発注書更新API
        // PUT: api/PurchaseOrders/{id}
        // =============================
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PurchaseOrder order)
        {
            // 既存データを取得
            var existing = await _context.Purchase_Orders
                .Include(o => o.OrderItemLists)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existing == null)
                return NotFound(new { success = false, message = "Not found" });

            // 発注書本体のプロパティを更新
            _context.Entry(existing).CurrentValues.SetValues(order);

            // 商品リストを一度クリアしてから再登録
            existing.OrderItemLists.Clear();
            foreach (var item in order.OrderItemLists)
            {
                existing.OrderItemLists.Add(item);
            }

            // DBに保存
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = existing });
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
