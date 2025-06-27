using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.Models.Masters;
using Ensyu_E_PAN.DTOs.Accounts;
using Microsoft.AspNetCore.Identity.Data;
using Ensyu_E_PAN.DTOs.UpdateAccounts;


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

        [HttpPost("users/register")]
        public async Task<IActionResult> CreateUser([FromBody] UserRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Roll・Storeの存在チェック
            var role = await _context.Roll_Lists.FindAsync(dto.Roll_Cd);
            var store = await _context.Stores.FindAsync(dto.Stores_Cd);

            if (role == null || store == null)
                return NotFound("指定されたロールまたは店舗が存在しません");

            var user = new User
            {
                Login_Id = dto.Login_Id,
                Name = dto.Name,
                Roll_Cd = dto.Roll_Cd,
                Password = dto.Password, // 本番ならハッシュ推奨！
                Stores_Cd = dto.Stores_Cd,
                TimePrice_D = dto.TimePrice_D,
                TimePrice_N = dto.TimePrice_N
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserWithFullRoleLoginResponseDto
            {
                Id = user.Id,
                Login_Id = user.Login_Id,
                Name = user.Name,
                Role = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    IsAdmin = role.IsAdmin
                },
                StoreName = store.C_Name,
                StoreAddress1 = store.Address1,
                StoreAddress2 = store.Address2,
                StorePostCode = store.Post_Code,
                StoreTel = store.Tel,
                StoreFax = store.Fax,
                StoreMail = store.Mail,
                TimePrice_D = user.TimePrice_D,
                TimePrice_N = user.TimePrice_N
            };

            return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, response);
        }
        //アカウント更新
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("ユーザーが見つかりません");

            // 変更内容を更新
            user.Name = dto.Name;
            user.Password = dto.Password; // 実運用では必ずハッシュ化を！
            user.TimePrice_D = dto.TimePrice_D;
            user.TimePrice_N = dto.TimePrice_N;
            user.Roll_Cd = dto.Roll_Cd;
            user.Stores_Cd = dto.Stores_Cd;

            await _context.SaveChangesAsync();

            return NoContent(); // 204：成功だが返すデータなし
        }
        //アカウント削除
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("対象のユーザーが見つかりません");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent(); // 204: 削除成功、返却する内容なし
        }
    }
}
