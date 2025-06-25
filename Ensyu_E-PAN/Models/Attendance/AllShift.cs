using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ensyu_E_PAN.Models.Masters;

namespace Ensyu_E_PAN.Models.Attendance
{
    public class AllShift
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Store")]
        public int Store_ID { get; set; }

        [Required]
        public DateTime Date { get; set; }//年月

        [Required]
        public bool Confirm_Flg { get; set; }//シフト確定フラグ

        [Required]
        public DateTime Fixed_Date { get; set; }//シフト確定期日

        [Required]
        public bool Sending_Flg { get; set; }//本部への送付フラグ

        [Required]
        public int Cost { get; set; }//人件費

        [Required]
        public TimeSpan Sum_WorkTime { get; set; }//各従業員の労働時間計

        [Required]
        public bool Rec_Flg { get; set; }//シフト希望収集中フラグ

        //ナビゲーション
        public Store? Store { get; set; }
        public ICollection<DayShift>? DayShifts { get; set; }
        public ICollection<UserShift>? UserShifts { get; set; }
    }
}
