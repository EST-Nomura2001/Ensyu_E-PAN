using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensyu_E_PAN.Models.Attendance
{
    public class AllShift
    {

        [Key]
        public int Id { get; set; } // 月別シフトID（主キー）

        [Required]
        public DateTime Date { get; set; } // 年月（yyyy-MM）

        [Required]
        public bool ConfirmFlg { get; set; } // シフト確定フラグ

        public DateTime FixedDate { get; set; } // シフト確定期日

        public bool SendingFlg { get; set; } // 本部への送付フラグ

        public int Cost { get; set; } // 人件費合計（初期値: 0）

        public TimeSpan SumWorkTime { get; set; } // 総労働時間（TimeSpan型）

        public bool RecFlg { get; set; } // シフト希望収集中フラグ（TRUE＝受付中）

        // ナビゲーションプロパティ（必要に応じて）
        // public ICollection<UserShift> UserShifts { get; set; }
        // public ICollection<DayShift> DayShifts { get; set; }

    }
}
