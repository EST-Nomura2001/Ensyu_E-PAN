namespace Ensyu_E_PAN.DTOs
{
    public class UpdateDateScheduleDto
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public int Work_Roll_Id { get; set; }
        public int Day_Shift_Id { get; set; }
        public DateTime? Start_WorkTime { get; set; }
        public DateTime? End_WorkTime { get; set; }
        public DateTime? Start_BreakTime { get; set; }
        public DateTime? End_BreakTime { get; set; }

    }
}
