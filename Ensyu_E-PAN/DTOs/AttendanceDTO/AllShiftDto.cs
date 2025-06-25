namespace Ensyu_E_PAN.DTOs.AttendanceDTO
{
    public class AllShiftDto
    {
        public int Id { get; set; }
        public int Store_ID { get; set; }
        public DateTime Date { get; set; }
        public bool Confirm_Flg { get; set; }
        public DateTime Fixed_Date { get; set; }
        public bool Sending_Flg { get; set; }
        public int Cost { get; set; }
        public TimeSpan Sum_WorkTime { get; set; }
        public bool Rec_Flg { get; set; }

        public List<UserShiftDto> UserShifts { get; set; }

    }
}
