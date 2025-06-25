using Microsoft.AspNetCore.Mvc;
using Ensyu_E_PAN.Models; // データコンテキストやモデルへの参照
using System.Linq;
using System;
using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.Models.Attendance;
using Ensyu_E_PAN.Services;
using Ensyu_E_PAN.DTOs;
using Microsoft.EntityFrameworkCore; // Includeメソッドのため

namespace Ensyu_E_PAN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AnyDataDbContext _context; // データコンテキストを後で定義
        private readonly AttendanceCalculationService _calcService;//計算用メソッド

        public AttendanceController(AnyDataDbContext context)
        {
            _context = context;
            _calcService = new AttendanceCalculationService();
        }
        //Get処理↓
        //全体の本日分のスケジュールを渡す
        [HttpGet("DateSchedules/{today}")]
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

        //全体の当月分のスケジュールを渡す
        [HttpGet("AllShiftSchedules/{year}/{month}")]
        public async Task<IActionResult> GetAllShiftSchedules(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var allShifts = await _context.All_Shifts
                .Where(a => a.Date.Year == year && a.Date.Month == month)
                .Include(a => a.UserShifts)
                    .ThenInclude(us => us.UserDateShifts)
                        .ThenInclude(uds => uds.DateSchedule)
                            .ThenInclude(ds => ds.User)
                .Include(a => a.UserShifts)
                    .ThenInclude(us => us.UserDateShifts)
                        .ThenInclude(uds => uds.DateSchedule)
                            .ThenInclude(ds => ds.WorkRoll)
                .Include(a => a.UserShifts)
                    .ThenInclude(us => us.UserDateShifts)
                        .ThenInclude(uds => uds.DateSchedule)
                            .ThenInclude(ds => ds.DayShift)
                .ToListAsync();

            var dateSchedules = allShifts
                .SelectMany(a => a.UserShifts)
                .SelectMany(us => us.UserDateShifts)
                .Select(uds => uds.DateSchedule)
                .Distinct() // 重複を防ぐ
                .ToList();

            return Ok(dateSchedules);
        }

        //個人の本日のスケジュール
        [HttpGet("UserSchedule/Day/{userId}/{date}")]
        public async Task<IActionResult> GetUserDailySchedule(int userId, DateTime date)
        {
            var schedule = await _context.Date_Schedules
                .Where(ds => ds.User_Id == userId && ds.Today.Date == date.Date)
                .Include(ds => ds.WorkRoll)
                .Include(ds => ds.DayShift)
                .Include(ds => ds.UserDateShifts)
                    .ThenInclude(uds => uds.UserShift)
                        .ThenInclude(us => us.AllShift)
                .FirstOrDefaultAsync();

            return schedule != null ? Ok(schedule) : NotFound();
        }


        //個人の当月のスケジュール
        [HttpGet("UserSchedule/Month/{userId}/{year}/{month}")]
        public async Task<IActionResult> GetUserMonthlySchedules(int userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var schedules = await _context.Date_Schedules
                .Where(ds => ds.User_Id == userId && ds.Today >= startDate && ds.Today < endDate)
                .Include(ds => ds.WorkRoll)
                .Include(ds => ds.DayShift)
                .Include(ds => ds.UserDateShifts)
                    .ThenInclude(uds => uds.UserShift)
                        .ThenInclude(us => us.AllShift)
                .ToListAsync();

            return Ok(schedules);
        }
        //Get処理↑

        //Post処理↓
        //シフト表作成
        [HttpPost("generate-monthly/{postDate}")]
        public async Task<IActionResult> GenerateNextMonthShifts(DateTime postDate)
        {
            var now = DateTime.Now;
            var firstDay = new DateTime(postDate.Year,postDate.Month,1).AddMonths(1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var stores = await _context.Stores.Include(s => s.Users).ToListAsync();
            var workRoll = await _context.WorkRoll_Lists.FirstOrDefaultAsync();
            if (workRoll == null)
                return BadRequest("業務ロールが存在しません");

            foreach (var store in stores)
            {
                // 店舗ごとに AllShift の重複チェック
                bool exists = await _context.All_Shifts.AnyAsync(a =>
                    a.Store_ID == store.Id &&
                    a.Date.Year == firstDay.Year &&
                    a.Date.Month == firstDay.Month);

                if (exists)
                    continue; // skip if already exists

                var allShift = new AllShift
                {
                    Store_ID = store.Id,
                    Date = firstDay,
                    Confirm_Flg = false,
                    Fixed_Date = firstDay.AddDays(20),
                    Sending_Flg = false,
                    Cost = 0,
                    Sum_WorkTime = TimeSpan.Zero,
                    Rec_Flg = true
                };
                _context.All_Shifts.Add(allShift);
                await _context.SaveChangesAsync();

                // DayShifts 作成
                var dayShifts = new List<DayShift>();
                for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
                {
                    var dayShift = new DayShift
                    {
                        Date = date,
                        All_Shift_Id = allShift.Id,
                        Sum_WorkTime = TimeSpan.Zero,
                        Sum_TotalCost = 0
                    };
                    dayShifts.Add(dayShift);
                    _context.Day_Shifts.Add(dayShift);
                }
                await _context.SaveChangesAsync();

                // 各ユーザー分の UserShift 作成
                foreach (var user in store.Users)
                {
                    var userShift = new UserShift
                    {
                        User_Id = user.Id,
                        Shift_Id = allShift.Id,
                        Date_Ym = firstDay,
                        List_Status = false,
                        Total_WorkTime = TimeSpan.Zero,
                        Month_Price = 0,
                        U_Confirm_Flg = false
                    };
                    _context.User_Shifts.Add(userShift);
                    await _context.SaveChangesAsync();

                    foreach (var dayShift in dayShifts)
                    {
                        var schedule = new DateSchedule
                        {
                            Today = dayShift.Date,
                            User_Id = user.Id,
                            Work_Roll_Id = workRoll.Id,
                            Day_Shift_Id = dayShift.Id
                        };
                        _context.Date_Schedules.Add(schedule);
                        await _context.SaveChangesAsync();

                        var link = new UserDateShift
                        {
                            User_Shift_Id = userShift.Id,
                            Shift_Date = dayShift.Date,
                            Date_Schedule_Id = schedule.Id
                        };
                        _context.User_Date_Shifts.Add(link);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok("翌月分のシフト表を事業所単位で作成しました。");
        }
        //Post処理↑

        //Put処理
        [HttpPut("users/{userId}/schedules/{scheduleId}")]
        public async Task<IActionResult> UpdateUserDateSchedule(int userId, int scheduleId,
            [FromBody] UpdateDateScheduleDto dto)

        {
            if (userId != dto.User_Id || scheduleId != dto.Id)
            {
                return BadRequest("パラメータと送信データの不一致があります。");
            }

            var schedule = await _context.Date_Schedules
                .Include(ds => ds.UserDateShifts)
                .FirstOrDefaultAsync(ds => ds.Id == scheduleId && ds.User_Id == userId);

            if (schedule == null)
            {
                return NotFound("指定されたスケジュールが見つかりません。");
            }

            // 更新対象フィールド
            schedule.Work_Roll_Id = dto.Work_Roll_Id;
            schedule.Start_WorkTime = dto.Start_WorkTime;
            schedule.End_WorkTime = dto.End_WorkTime;
            schedule.Start_BreakTime = dto.Start_BreakTime;
            schedule.End_BreakTime = dto.End_BreakTime;

            // ユーザー取得（時給など計算に必要）
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return BadRequest("ユーザーが存在しません");
            // 該当DayShiftの日付の全スケジュール取得
            var allDateSchedulesInDay = await _context.Date_Schedules
                .Where(ds => ds.Day_Shift_Id == schedule.Day_Shift_Id)
                .ToListAsync();

            _calcService.CalculateDateSchedule(schedule, user);

            var link = schedule.UserDateShifts.FirstOrDefault();
            if (link == null)
                return BadRequest("UserDateShift に紐づくデータが見つかりませんでした。");

            // 日単位の再集計
            var dayShift = await _context.Day_Shifts.FirstOrDefaultAsync(d => d.Id == schedule.Day_Shift_Id);
            _calcService.CalculateDayShift(dayShift, allDateSchedulesInDay);

            // ユーザーの月間全スケジュール取得
            var userDateShiftIds = await _context.User_Date_Shifts
                .Where(uds => uds.User_Shift_Id == link.User_Shift_Id)
                .Select(uds => uds.Date_Schedule_Id)
                .ToListAsync();

            var userSchedules = await _context.Date_Schedules
                .Where(ds => userDateShiftIds.Contains(ds.Id))
                .ToListAsync();

            var userShift = await _context.User_Shifts
                .FirstOrDefaultAsync(us => us.Id == link.User_Shift_Id);
            _calcService.CalculateUserShift(userShift, userSchedules);

            // 全体集計（AllShift）
            var allUserShifts = await _context.User_Shifts
                .Where(us => us.Shift_Id == userShift.Shift_Id)
                .ToListAsync();
            var allShift = await _context.All_Shifts
                .FirstOrDefaultAsync(a => a.Id == userShift.Shift_Id);
            _calcService.CalculateAllShift(allShift, allUserShifts);
            await _context.SaveChangesAsync();
            return Ok("スケジュールを更新しました。");
        }

        //Put処理
    }
} 