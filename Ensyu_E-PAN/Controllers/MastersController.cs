using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.DTOs.Master;
using Ensyu_E_PAN.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ensyu_E_PAN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MastersController : ControllerBase
    {
        private readonly AnyDataDbContext _context; // データコンテキストを後で定義

        public MastersController(AnyDataDbContext context)
        {
            _context = context;
        }
        [HttpGet("company/{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();
            return Ok(company);
        }
        [HttpGet("item/{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
        [HttpGet("items")]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _context.Items
                .Select(item => new ItemDto
                {
                    Id = item.Id,
                    Item_Name = item.Item_Name
                })
                .ToListAsync();

            return Ok(items);
        }


        [HttpGet("roll/{id}")]
        public async Task<IActionResult> GetRollById(int id)
        {
            var roll = await _context.Roll_Lists.FindAsync(id);
            if (roll == null) return NotFound();
            return Ok(roll);
        }
        [HttpGet("store/{id}")]
        public async Task<IActionResult> GetStoreById(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return NotFound();
            return Ok(store);
        }

        [HttpGet("stores")]
        public async Task<IActionResult> GetAllStores()
        {
            var stores = await _context.Stores
                .Select(s => new StoreDto
                {
                    Id = s.Id,
                    C_Name = s.C_Name,
                    Address1 = s.Address1,
                    Address2 = s.Address2,
                    Post_Code = s.Post_Code,
                    Mail = s.Mail,
                    Tel = s.Tel,
                    Fax = s.Fax
                })
                .ToListAsync();

            return Ok(stores);
        }

        [HttpGet("workroll/{id}")]
        public async Task<IActionResult> GetWorkRollById(int id)
        {
            var workRoll = await _context.WorkRoll_Lists.FindAsync(id);
            if (workRoll == null) return NotFound();
            return Ok(workRoll);
        }
    }
}
