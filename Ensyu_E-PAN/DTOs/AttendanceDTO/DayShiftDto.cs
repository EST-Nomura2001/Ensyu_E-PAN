namespace Ensyu_E_PAN.DTOs.AttendanceDTO
{
    public class DayShiftDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Sum_WorkTime { get; set; }
        public int Sum_TotalCost { get; set; }
        public List<DateScheduleDto> DateSchedules { get; set; }

    }
}
