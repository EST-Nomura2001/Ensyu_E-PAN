using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.DTOs.UpdateAccounts
{
    public class UserRequestDto
    {
        [Required]
        public string Login_Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Roll_Cd { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int Stores_Cd { get; set; }

        public int TimePrice_D { get; set; }
        public int TimePrice_N { get; set; }

    }
}
