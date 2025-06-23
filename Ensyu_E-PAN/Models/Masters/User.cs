using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ensyu_E_PAN.Models.Attendance;

namespace Ensyu_E_PAN.Models.Masters
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Login_Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [ForeignKey("Roll")]
        public int Roll_Cd { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [ForeignKey("Store")]
        public int Stores_Cd { get; set; }

        [Required]
        public int TimePrice_D { get; set; }

        [Required]
        public int TimePrice_N { get; set; }

        // ナビゲーションプロパティ（必要に応じて）
        public Roll Roll { get; set; }
        public Store Store { get; set; }
    }
}
