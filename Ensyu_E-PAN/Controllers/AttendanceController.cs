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
    }
} 