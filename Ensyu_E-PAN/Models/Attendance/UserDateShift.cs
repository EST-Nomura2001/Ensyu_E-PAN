using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.Models.Attendance
{
    public class UserDateShift
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("UserShift")]
        public int User_Shift_Id { get; set; }

        [Required]
        public DateTime Shift_Date { get; set; }

        [Required]
        [ForeignKey("DateSchedule")]
        public int Date_Schedule_Id { get; set; }

        // ナビゲーションプロパティ（必要に応じて）
        public UserShift UserShift { get; set; }
        public DateSchedule DateSchedule { get; set; }

    }
}
