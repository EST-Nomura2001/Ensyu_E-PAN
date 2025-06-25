using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.Models.Masters;
using Ensyu_E_PAN.DTOs.Accounts;
using Microsoft.AspNetCore.Identity.Data;


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

        [HttpGet("login")]
        public IActionResult Login([FromQuery] string loginId, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(loginId) || string.IsNullOrWhiteSpace(password))
                return BadRequest("ログインIDまたはパスワードが空です");

            var user = _context.Users
                .Include(u => u.Roll)
                .Include(u => u.Store)
                .FirstOrDefault(u => u.Login_Id == loginId);

            if (user == null || user.Password != password)
                return Unauthorized("ログインIDまたはパスワードが正しくありません");

            var dto = new UserWithFullRoleLoginResponseDto
            {
                Id = user.Id,
                Login_Id = user.Login_Id,
                Name = user.Name,
                Role = new RoleDto
                {
                    Id = user.Roll.Id,
                    Name = user.Roll.Name,
                    IsAdmin = user.Roll.IsAdmin
                },
                StoreName = user.Store?.C_Name,
                StoreAddress1 = user.Store?.Address1,
                StoreAddress2 = user.Store?.Address2,
                StorePostCode = user.Store?.Post_Code,
                StoreTel = user.Store?.Tel,
                StoreFax = user.Store?.Fax,
                StoreMail = user.Store?.Mail,
                TimePrice_D = user.TimePrice_D,
                TimePrice_N = user.TimePrice_N
            };

            return Ok(dto);
        }

        [HttpGet("users/all")]
        public IActionResult Get()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

    }
}
