namespace Ensyu_E_PAN.DTOs.AttendanceDTO
{
    public class UserDateShiftDto
    {
        public int Id { get; set; }
        public DateTime Shift_Date { get; set; }

        public DateScheduleDto DateSchedule { get; set; }
    }
}
