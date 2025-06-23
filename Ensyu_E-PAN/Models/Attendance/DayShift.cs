using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.Models.Attendance
{
    public class DayShift
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [ForeignKey("AllShift")]
        public int All_Shift_Id { get; set; }

        [Required]
        public TimeSpan Sum_WorkTime { get; set; }

        [Required]
        public int Sum_TotalCost { get; set; }

        // ナビゲーションプロパティ（必要に応じて）
        public AllShift AllShift { get; set; }
        public ICollection<UserShift> UserShifts { get; set; }
        public ICollection<DateSchedule> DateSchedules { get; set; }

    }
}
