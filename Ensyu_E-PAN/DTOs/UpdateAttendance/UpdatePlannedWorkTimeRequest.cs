namespace Ensyu_E_PAN.DTOs.UpdateAttendance
{
    public class UpdatePlannedWorkTimeRequest
    {
        public DateTime TargetDate { get; set; }
        public DateTime? P_Start_WorkTime { get; set; }
        public DateTime? P_End_WorkTime { get; set; }

    }
}
