namespace Ensyu_E_PAN.DTOs.AttendanceDTO
{
    public class DateScheduleDto
    {
        public int Id { get; set; }
        public DateTime Today { get; set; }

        public DateTime? P_Start_WorkTime { get; set; }
        public DateTime? P_End_WorkTime { get; set; }
        public DateTime? U_Start_WorkTime { get; set; }
        public DateTime? U_End_WorkTime { get; set; }
        public DateTime? Start_WorkTime { get; set; }
        public DateTime? End_WorkTime { get; set; }
        public DateTime? Start_BreakTime { get; set; }
        public DateTime? End_BreakTime { get; set; }

        public TimeSpan? T_WorkTime_D { get; set; }
        public TimeSpan? T_WorkTime_N { get; set; }
        public TimeSpan? T_WorkTime_All { get; set; }

        public int? D_DayPrice { get; set; }
        public int? N_DayPrice { get; set; }
        public int? T_DayPrice { get; set; }

        // 参照情報
        public string UserName { get; set; }
        public string WorkRollName { get; set; }
        public DateTime? DayShiftDate { get; set; }

        public bool? U_Confirm_Flg { get; set; }
    }
}
