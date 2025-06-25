namespace Ensyu_E_PAN.DTOs.AttendanceDTO
{
    public class UserShiftDto
    {

        public int Id { get; set; }
        public int User_Id { get; set; }
        public string UserName { get; set; }
        public bool List_Status { get; set; }
        public TimeSpan Total_WorkTime { get; set; }
        public int Month_Price { get; set; }
        public bool U_Confirm_Flg { get; set; }

        public List<UserDateShiftDto> UserDateShifts { get; set; }
    }
}
