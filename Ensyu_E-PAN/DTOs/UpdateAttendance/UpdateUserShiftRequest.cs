
namespace Ensyu_E_PAN.DTOs.UpDateAttendance
{
	public class UpdateUserShiftRequest
	{
		public DateTime TargetDate { get; set; } // どの日付の勤務予定を対象にするか
		public DateTime? U_Start_WorkTime { get; set; }
		public DateTime? U_End_WorkTime { get; set; }
		public bool U_Confirm_Flg { get; set; }
	}
}