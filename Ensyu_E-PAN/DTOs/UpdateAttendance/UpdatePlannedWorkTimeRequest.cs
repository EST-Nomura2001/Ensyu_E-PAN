namespace Ensyu_E_PAN.DTOs.UpdateAttendance
{
    public class UpdatePlannedWorkTimeRequest
    {
        public int Work_Roll_Id {  get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime? P_Start_WorkTime { get; set; }
        public DateTime? P_End_WorkTime { get; set; }

    }
}
