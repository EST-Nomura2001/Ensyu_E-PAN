using Ensyu_E_PAN.Models.Masters;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.Models.Attendance
{
    public class UserShift
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int User_Id { get; set; }

        [Required]
        [ForeignKey("AllShift")]
        public int Shift_Id { get; set; }

        public DateTime Date_Ym { get; set; }

        [Required]
        public bool List_Status { get; set; }

        [Required]
        public TimeSpan Total_WorkTime { get; set; }

        [Required]
        public int Month_Price { get; set; }

        [Required]
        public bool U_Confirm_Flg { get; set; }

        // ナビゲーションプロパティ（必要に応じて）
        public User? User { get; set; }
        public AllShift? AllShift { get; set; }
        public ICollection<UserDateShift>? UserDateShifts { get; set; }

    }
}
