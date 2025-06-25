using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.DTOs.Accounts
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }

    }
}
