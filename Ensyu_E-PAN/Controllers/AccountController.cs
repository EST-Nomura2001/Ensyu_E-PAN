using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.Models.Masters;


namespace Ensyu_E_PAN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public readonly AnyDataDbContext _context;
        public AccountController(AnyDataDbContext context)
        {
            _context = context;
        }

        //[HttpGet("users/all")]
        //public IActionResult GetAllUsers()
        //{
        //    var users = _context.Users.ToList();
        //    return Ok(users);
        //}
        //[HttpGet("users/all")]
        //public IActionResult Get()
        //{
        //    var users = _context.Users.ToList();
        //    return Ok(users);
        //}

    }
}
