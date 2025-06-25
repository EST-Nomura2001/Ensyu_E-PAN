using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.Models.Order;
using System.Linq;
using System.Threading.Tasks;

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
            // 発注書本体＋商品リスト＋会社・事業所情報も含めて取得
            var order = await _context.Purchase_Orders
                .Include(o => o.OrderItemLists) // 商品リストも一緒に取得
                .Include(o => o.Company)       // 会社情報も一緒に取得
                .Include(o => o.Store)         // 事業所情報も一緒に取得
                .FirstOrDefaultAsync(o => o.Id == id);

            // 見つからなければ404を返す
            if (order == null)
                return NotFound(new { success = false, message = "Not found" });

            // 成功時はデータを返す
            return Ok(new { success = true, data = order });
        }

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
            // 発注書一覧を取得（会社情報も含める）
            var list = await _context.Purchase_Orders
                .Include(o => o.Company)
                .OrderByDescending(o => o.Id)
                .Select(o => new
                {
                    Id = o.Id,
                    Quotation = o.Quotation,
                    Title = o.Title,
                    Company = o.Company, // 会社情報
                    Order_Date = o.Order_Date,
                    CreatedAt = o.Order_Date,
                    Confirm_Flg = o.Confirm_Flg
                })
                .ToListAsync();

            // 一覧を返す
            return Ok(list);
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
