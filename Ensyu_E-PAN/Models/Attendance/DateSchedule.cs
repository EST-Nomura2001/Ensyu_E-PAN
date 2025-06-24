using Ensyu_E_PAN.Models.Masters;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ensyu_E_PAN.Models.Attendance
{
    public class DateSchedule
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Today { get; set; }

        [Required]
        [ForeignKey("User")]
        public int User_Id { get; set; }

        [Required]
        [ForeignKey("WorkRoll")]
        public int Work_Roll_Id { get; set; }

        [Required]
        [ForeignKey("DayShift")]
        public int Day_Shift_Id { get; set; }

        //予定勤務時間
        public DateTime? P_Start_WorkTime { get; set; }
        public DateTime? P_End_WorkTime { get; set; }

        //ユーザーごとの希望勤務時間
        public DateTime? U_Start_WorkTime { get; set; }
        public DateTime? U_End_WorkTime { get; set; }

        //実勤務時間
        public DateTime? Start_WorkTime { get; set; }
        public DateTime? End_WorkTime { get; set; }

        //休憩時間
        public DateTime? Start_BreakTime { get; set; }
        public DateTime? End_BreakTime { get; set; }

        public TimeSpan? T_WorkTime_D { get; set; }//日中労働時間
        public TimeSpan? T_WorkTime_N { get; set; }//夜間労働時間
        public TimeSpan? T_WorkTime_All { get; set; }//総労働時間

        public int? D_DayPrice { get; set; }//日中日給
        public int? N_DayPrice { get; set; }//夜間日給
        public int? T_DayPrice { get; set; }//日給計

        // ナビゲーションプロパティ
        public User? User { get; set; }
        public WorkRoll? WorkRoll { get; set; }
        public DayShift? DayShift { get; set; }
        public ICollection<UserDateShift>? UserDateShifts { get; set; }  


    }
}
