using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet("workroll/{id}")]
        public async Task<IActionResult> GetWorkRollById(int id)
        {
            var workRoll = await _context.WorkRoll_Lists.FindAsync(id);
            if (workRoll == null) return NotFound();
            return Ok(workRoll);
        }
    }
}
