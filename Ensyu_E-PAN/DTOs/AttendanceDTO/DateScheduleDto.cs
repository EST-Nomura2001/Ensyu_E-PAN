namespace Ensyu_E_PAN.DTOs.AttendanceDTO
{
    public class DateScheduleDto
    {
        public int Id { get; set; }
        public int StoreId { get; set; }//店舗ID
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
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int WorkRollId { get; set; }
        public string WorkRollName { get; set; }
        public int DayShiftId { get; set; }
        public bool? U_Confirm_Flg { get; set; }
        public int DayPrice { get; set; }
    }
}
