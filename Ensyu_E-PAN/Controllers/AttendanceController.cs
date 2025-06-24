using Microsoft.AspNetCore.Mvc;
using Ensyu_E_PAN.Models; // データコンテキストやモデルへの参照
using System.Linq;
using System;
using Ensyu_E_PAN.Data;
using Microsoft.EntityFrameworkCore; // Includeメソッドのため

namespace Ensyu_E_PAN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AnyDataDbContext _context; // データコンテキストを後で定義

        public AttendanceController(AnyDataDbContext context)
        {
            _context = context;
        }

        //本日分のスケジュールを渡す
        [HttpGet("Date_Schedules/{today}")]
        public async Task<IActionResult> GetTodayAttendance(DateTime today)
        {
            var records = await _context.Date_Schedules
             .Where(ds => ds.Today.Date == today.Date) // ここで日付だけを比較
             // DateSchedule 直下の関係
            .Include(ds => ds.User)
            .Include(ds => ds.WorkRoll)
            .Include(ds => ds.DayShift)

            // UserDateShifts → UserShift → User
            .Include(ds => ds.UserDateShifts)
                .ThenInclude(uds => uds.UserShift)
                    .ThenInclude(us => us.User)
            .ToListAsync();
            return Ok(records);
        }

        //当月分のスケジュールを渡す
        [HttpGet("AllShiftSchedules/{year}/{month}")]
        public async Task<IActionResult> GetAllShiftSchedules(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var dateSchedules = await _context.All_Shifts
                .Where(a => a.Date >= startDate && a.Date < endDate)
                .SelectMany(a => a.UserShifts)
                    .SelectMany(us => us.UserDateShifts)
                        .Include(uds => uds.DateSchedule)
                            .ThenInclude(ds => ds.User)
                        .Include(uds => uds.DateSchedule)
                            .ThenInclude(ds => ds.WorkRoll)
                        .Include(uds => uds.DateSchedule)
                            .ThenInclude(ds => ds.DayShift)
                .Select(uds => uds.DateSchedule)
                .ToListAsync();

            return Ok(dateSchedules);
        }



        /*[HttpGet("today")]
        public IActionResult GetTodayAttendance()
        {
            // --- バックエンド実装が完了するまで、ダミーデータを返却 ---
            var dummyData = new[]
            {
                new { userId = 1, userName = "アルバイトA", taskName = "レジ", p_start_worktime = "09:00", p_end_worktime = "17:00", start_worktime = "09:02", end_worktime = "", start_break_time = "12:00", end_break_time = "12:30" },
                new { userId = 2, userName = "アルバイトB", taskName = "品出し", p_start_worktime = "10:00", p_end_worktime = "18:00", start_worktime = "10:05", end_worktime = "", start_break_time = "", end_break_time = "" },
                new { userId = 3, userName = "アルバイトC", taskName = "清掃", p_start_worktime = "12:00", p_end_worktime = "20:00", start_worktime = "", end_worktime = "", start_break_time = "", end_break_time = "" },
            };
            return Ok(dummyData);
            // --- ここまでダミーデータ ---


            /* --- 本来の実装 (データベースコンテキスト設定後に有効化) ---
            try
            {
                var today = DateTime.Today;
                var attendanceData = _context.DateSchedules
                    .Where(ds => ds.Today.Date == today)
                    .Include(ds => ds.User)
                    .Include(ds => ds.WorkRoll)
                    .Select(ds => new {
                        userId = ds.User.Id,
                        userName = ds.User.Name,
                        taskName = ds.WorkRoll.Name,
                        p_start_worktime = ds.P_Start_WorkTime,
                        p_end_worktime = ds.P_End_WorkTime,
                        start_worktime = ds.Start_WorkTime,
                        end_worktime = ds.End_WorkTime,
                        start_break_time = ds.Start_BreakTime,
                        end_break_time = ds.End_BreakTime
                    })
                    .ToList();

                return Ok(attendanceData);
            }
            catch (Exception ex)
            {
                // ログ記録
                return StatusCode(500, "Internal server error");
            }
        }
        */

    }
} 