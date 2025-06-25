using System;
using System.Collections.Generic;
using System.Linq;
using Ensyu_E_PAN.Models.Attendance;
using Ensyu_E_PAN.Models.Masters;

namespace Ensyu_E_PAN.Services
{
    public class AttendanceCalculationService
    {
        /// <summary>
        /// DateScheduleの各種労働時間・日給計算
        /// </summary>
        /// <param name="ds">計算対象のDateSchedule（1日分の勤怠データ）</param>
        /// <param name="user">該当ユーザー（時給情報を持つ）</param>
        public void CalculateDateSchedule(DateSchedule ds, User user)
        {
            // 出勤・退勤時刻が未入力の場合は何もしない
            if (ds.Start_WorkTime == null || ds.End_WorkTime == null)
                return;

            var start = ds.Start_WorkTime.Value;
            var end = ds.End_WorkTime.Value;

            //休憩時間の算出
            int breakDayMinutes = 0;
            int breakNightMinutes = 0;

            if (ds.Start_BreakTime != null && ds.End_BreakTime != null)
            {
                (breakDayMinutes, breakNightMinutes) =
                    SplitBreakByTimeOfDay(ds.Start_BreakTime.Value, ds.End_BreakTime.Value);
            }


            // 日中労働時間（5:00～22:00の範囲で働いた時間）
            ds.T_WorkTime_D = TimeSpan.FromHours(CalculateDayWorkTime(start, end, breakDayMinutes));
            // 夜間労働時間（22:00～翌5:00の範囲で働いた時間）
            ds.T_WorkTime_N = TimeSpan.FromHours(CalculateNightWorkTime(start, end, breakNightMinutes));
            // 総労働時間（日中＋夜間）
            ds.T_WorkTime_All = ds.T_WorkTime_D + ds.T_WorkTime_N;

            // 日中日給（日中労働時間×日中時給）
            ds.D_DayPrice = (int)Math.Round(ds.T_WorkTime_D.Value.TotalHours * user.TimePrice_D);
            // 夜間日給（夜間労働時間×夜間時給）
            ds.N_DayPrice = (int)Math.Round(ds.T_WorkTime_N.Value.TotalHours * user.TimePrice_N);
            // 日給合計（日中日給＋夜間日給）
            ds.T_DayPrice = ds.D_DayPrice + ds.N_DayPrice;
        }

        /// <summary>
        /// UserShift（月集計）計算
        /// </summary>
        /// <param name="userShift">計算結果を格納するUserShift（月単位の集計）</param>
        /// <param name="schedules">その月の全DateScheduleリスト</param>
        public void CalculateUserShift(UserShift userShift, List<DateSchedule> schedules)
        {
            // その月の全日分の総労働時間（TimeSpanの合計）
            var totalWork = schedules
                .Where(s => s.T_WorkTime_All.HasValue)
                .Select(s => s.T_WorkTime_All.Value)
                .Aggregate(TimeSpan.Zero, (sum, t) => sum + t);
            userShift.Total_WorkTime = totalWork;
            // その月の全日分の日給合計
            var totalPrice = schedules
                .Where(s => s.T_DayPrice.HasValue)
                .Sum(s => s.T_DayPrice.Value);
            userShift.Month_Price = totalPrice;
        }

        /// <summary>
        /// DayShift（日集計）計算
        /// </summary>
        /// <param name="dayShift">計算結果を格納するDayShift（1日分の集計）</param>
        /// <param name="schedules">その日の全DateScheduleリスト</param>
        public void CalculateDayShift(DayShift dayShift, List<DateSchedule> schedules)
        {
            // その日の全従業員分の総労働時間
            var sumWork = schedules
                .Where(s => s.T_WorkTime_All.HasValue)
                .Select(s => s.T_WorkTime_All.Value)
                .Aggregate(TimeSpan.Zero, (sum, t) => sum + t);
            dayShift.Sum_WorkTime = sumWork;
            // その日の全従業員分の日給合計
            var sumCost = schedules
                .Where(s => s.T_DayPrice.HasValue)
                .Sum(s => s.T_DayPrice.Value);
            dayShift.Sum_TotalCost = sumCost;
        }

        /// <summary>
        /// AllShift（全体集計）計算
        /// </summary>
        /// <param name="allShift">計算結果を格納するAllShift（全体集計）</param>
        /// <param name="userShifts">全従業員分のUserShiftリスト</param>
        public void CalculateAllShift(AllShift allShift, List<UserShift> userShifts)
        {
            // 全従業員分の月給合計（人件費）
            allShift.Cost = userShifts.Sum(u => u.Month_Price);
            // 全従業員分の月労働時間合計
            allShift.Sum_WorkTime = userShifts
                .Select(u => u.Total_WorkTime)
                .Aggregate(TimeSpan.Zero, (sum, t) => sum + t);
        }

        // --- 以下、日中・夜間労働時間計算ロジック ---

        /// <summary>
        /// 日中労働時間（5:00～22:00の範囲で働いた時間）を計算します。
        /// 休憩時間は全体労働時間に対する割合で日中・夜間に按分して減算します。
        /// </summary>
        /// <param name="startTime">出勤時刻</param>
        /// <param name="endTime">退勤時刻</param>
        /// <param name="breakMinutes">休憩時間（分）</param>
        /// <returns>日中労働時間（時間単位）</returns>
        private double CalculateDayWorkTime(DateTime startTime, DateTime endTime, int breakMinutes)
        {
            var dayStart = new TimeSpan(5, 0, 0);   // 5:00
            var dayEnd = new TimeSpan(22, 0, 0);    // 22:00
            var workStart = startTime.TimeOfDay;
            var workEnd = endTime.TimeOfDay;
            // 退勤が翌日（例：22:00～翌6:00など）の場合は24時間加算
            if (workEnd <= workStart)
                workEnd = workEnd.Add(TimeSpan.FromDays(1));
            double dayWorkMinutes = 0;
            var currentTime = workStart;
            // 1時間ごとにループして日中帯か判定
            while (currentTime < workEnd)
            {
                var currentHour = currentTime.Hours;
                var nextHour = currentTime.Add(TimeSpan.FromHours(1));
                // 5:00～22:00の範囲なら日中労働時間に加算
                if (currentHour >= 5 && currentHour < 22)
                {
                    var workDuration = Math.Min(60, (workEnd - currentTime).TotalMinutes);
                    dayWorkMinutes += Math.Max(0, workDuration);
                }
                currentTime = nextHour;
            }
            // 休憩時間を全体労働時間で按分して日中分を減算
            dayWorkMinutes = Math.Max(0, dayWorkMinutes - breakMinutes);
            // 分→時間に変換して返す
            return dayWorkMinutes / 60.0;
        }

        /// <summary>
        /// 夜間労働時間（22:00～翌5:00の範囲で働いた時間）を計算します。
        /// 休憩時間は全体労働時間に対する割合で日中・夜間に按分して減算します。
        /// </summary>
        /// <param name="startTime">出勤時刻</param>
        /// <param name="endTime">退勤時刻</param>
        /// <param name="breakMinutes">休憩時間（分）</param>
        /// <returns>夜間労働時間（時間単位）</returns>
        private double CalculateNightWorkTime(DateTime startTime, DateTime endTime, int breakMinutes)
        {
            var workStart = startTime.TimeOfDay;
            var workEnd = endTime.TimeOfDay;
            // 退勤が翌日（例：22:00～翌6:00など）の場合は24時間加算
            if (workEnd <= workStart)
                workEnd = workEnd.Add(TimeSpan.FromDays(1));
            double nightWorkMinutes = 0;
            var currentTime = workStart;
            // 1時間ごとにループして夜間帯か判定
            while (currentTime < workEnd)
            {
                var currentHour = currentTime.Hours % 24;
                // 22:00～翌5:00の範囲なら夜間労働時間に加算
                if (currentHour >= 22 || currentHour < 5)
                {
                    var workDuration = Math.Min(60, (workEnd - currentTime).TotalMinutes);
                    nightWorkMinutes += Math.Max(0, workDuration);
                }
                currentTime = currentTime.Add(TimeSpan.FromHours(1));
            }
            // 休憩時間を全体労働時間で按分して夜間分を減算
            nightWorkMinutes = Math.Max(0, nightWorkMinutes - breakMinutes);
            // 分→時間に変換して返す
            return nightWorkMinutes / 60.0;
        }

        //休憩時間を算出するメソッド
        private (int dayMinutes, int nightMinutes) SplitBreakByTimeOfDay(DateTime breakStart, DateTime breakEnd)
        {
            TimeSpan DayStart = TimeSpan.FromHours(5);
            TimeSpan DayEnd = TimeSpan.FromHours(22);
            // 日中帯：5:00～22:00、夜間帯：22:00〜翌5:00
            if (breakEnd <= breakStart) breakEnd = breakEnd.AddDays(1);

            var dayStartDt = breakStart.Date.Add(DayStart);
            var dayEndDt = breakStart.Date.Add(DayEnd);
            if (dayEndDt <= dayStartDt) dayEndDt = dayEndDt.AddDays(1);

            var night1Start = breakStart.Date;
            var night1End = breakStart.Date.Add(DayStart);
            var night2Start = breakStart.Date.Add(DayEnd);
            var night2End = night2Start.AddHours(7); // 22:00〜翌5:00

            double dayMinutes = 0;
            double nightMinutes = 0;

            // 日中帯重複
            var dayOverlapStart = breakStart < dayStartDt ? dayStartDt : breakStart;
            var dayOverlapEnd = breakEnd > dayEndDt ? dayEndDt : breakEnd;
            if (dayOverlapStart < dayOverlapEnd)
                dayMinutes += (dayOverlapEnd - dayOverlapStart).TotalMinutes;

            // 夜間帯（0:00〜5:00）
            var night1OverlapStart = breakStart < night1Start ? night1Start : breakStart;
            var night1OverlapEnd = breakEnd > night1End ? night1End : breakEnd;
            if (night1OverlapStart < night1OverlapEnd)
                nightMinutes += (night1OverlapEnd - night1OverlapStart).TotalMinutes;

            // 夜間帯（22:00〜24:00以降）
            var night2OverlapStart = breakStart < night2Start ? night2Start : breakStart;
            var night2OverlapEnd = breakEnd > night2End ? night2End : breakEnd;
            if (night2OverlapStart < night2OverlapEnd)
                nightMinutes += (night2OverlapEnd - night2OverlapStart).TotalMinutes;

            return ((int)Math.Round(dayMinutes), (int)Math.Round(nightMinutes));

        }

    }
} 