using Microsoft.AspNetCore.Mvc;
using Ensyu_E_PAN.Models; // データコンテキストやモデルへの参照
using System.Linq;
using System;
using Ensyu_E_PAN.Data;
using Ensyu_E_PAN.Models.Attendance;
using Ensyu_E_PAN.Services;
using Ensyu_E_PAN.DTOs;
using Ensyu_E_PAN.DTOs.UpDateAttendance;
using Ensyu_E_PAN.DTOs.AttendanceDTO;
using Microsoft.EntityFrameworkCore;
using Ensyu_E_PAN.DTOs.UpdateAttendance; // Includeメソッドのため

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

        [HttpGet("DateSchedules/{today}")]
        public async Task<IActionResult> GetTodayAttendance(DateTime today)
        {
            var dateSchedules = await _context.Date_Schedules
                .Where(ds => ds.Today.Date == today.Date)
                .Include(ds => ds.User)
                .Include(ds => ds.WorkRoll)
                .Include(ds => ds.DayShift)
                .Include(ds => ds.UserDateShifts)
                    .ThenInclude(uds => uds.UserShift)
                        .ThenInclude(us => us.User)
                .ToListAsync();

            // DayShift単位でグループ化
            var grouped = dateSchedules
                .Where(ds => ds.DayShift != null)
                .GroupBy(ds => ds.DayShift.Id)
                .Select(g =>
                {
                    var dayShift = g.First().DayShift;

                    return new DayShiftDto
                    {
                        Id = dayShift.Id,
                        Date = dayShift.Date,
                        Sum_WorkTime = dayShift.Sum_WorkTime,
                        Sum_TotalCost = dayShift.Sum_TotalCost,
                        DateSchedules = g.Select(ds => new DateScheduleDto
                        {
                            Id = ds.Id,
                            Today = ds.Today,
                            P_Start_WorkTime = ds.P_Start_WorkTime,
                            P_End_WorkTime = ds.P_End_WorkTime,
                            U_Start_WorkTime = ds.U_Start_WorkTime,
                            U_End_WorkTime = ds.U_End_WorkTime,
                            Start_WorkTime = ds.Start_WorkTime,
                            End_WorkTime = ds.End_WorkTime,
                            Start_BreakTime = ds.Start_BreakTime,
                            End_BreakTime = ds.End_BreakTime,
                            T_WorkTime_D = ds.T_WorkTime_D,
                            T_WorkTime_N = ds.T_WorkTime_N,
                            T_WorkTime_All = ds.T_WorkTime_All,
                            D_DayPrice = ds.D_DayPrice,
                            N_DayPrice = ds.N_DayPrice,
                            T_DayPrice = ds.T_DayPrice,
                            UserId = ds.User_Id,
                            UserName = ds.User?.Name,
                            WorkRollId = ds.Work_Roll_Id,
                            WorkRollName = ds.WorkRoll?.Name,
                            DayShiftId = ds.Day_Shift_Id,
                            U_Confirm_Flg = ds.UserDateShifts
                                .FirstOrDefault()?.UserShift?.U_Confirm_Flg
                        }).ToList()
                    };
                })
                .ToList();

            return Ok(grouped);
        }

        //全体の当月分のスケジュールを渡す

        [HttpGet("store/{storeId}/allshifts/{year}/{month}")]
        public async Task<IActionResult> GetStoreMonthlyShifts(int storeId, int year, int month)
        {
            var allShifts = await _context.All_Shifts
                .Where(a => a.Store_ID == storeId && a.Date.Year == year && a.Date.Month == month)
                .Include(a => a.UserShifts)
                    .ThenInclude(us => us.User)
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

            var dtoList = allShifts.Select(a => new AllShiftDto
            {
                Id = a.Id,
                Store_ID = a.Store_ID,
                Date = a.Date,
                Confirm_Flg = a.Confirm_Flg,
                Fixed_Date = a.Fixed_Date,
                Sending_Flg = a.Sending_Flg,
                Cost = a.Cost,
                Sum_WorkTime = a.Sum_WorkTime,
                Rec_Flg = a.Rec_Flg,
                UserShifts = a.UserShifts.Select(us => new UserShiftDto
                {
                    Id = us.Id,
                    User_Id = us.User_Id,
                    UserName = us.User?.Name,
                    List_Status = us.List_Status,
                    Total_WorkTime = us.Total_WorkTime,
                    Month_Price = us.Month_Price,
                    U_Confirm_Flg = us.U_Confirm_Flg,
                    UserDateShifts = us.UserDateShifts.Select(uds => new UserDateShiftDto
                    {
                        Id = uds.Id,
                        Shift_Date = uds.Shift_Date,
                        DateSchedule = new DateScheduleDto
                        {
                            Id = uds.DateSchedule.Id,
                            Today = uds.DateSchedule.Today,
                            P_Start_WorkTime = uds.DateSchedule.P_Start_WorkTime,
                            P_End_WorkTime = uds.DateSchedule.P_End_WorkTime,
                            U_Start_WorkTime = uds.DateSchedule.U_Start_WorkTime,
                            U_End_WorkTime = uds.DateSchedule.U_End_WorkTime,
                            Start_WorkTime = uds.DateSchedule.Start_WorkTime,
                            End_WorkTime = uds.DateSchedule.End_WorkTime,
                            Start_BreakTime = uds.DateSchedule.Start_BreakTime,
                            End_BreakTime = uds.DateSchedule.End_BreakTime,
                            T_WorkTime_D = uds.DateSchedule.T_WorkTime_D,
                            T_WorkTime_N = uds.DateSchedule.T_WorkTime_N,
                            T_WorkTime_All = uds.DateSchedule.T_WorkTime_All,
                            D_DayPrice = uds.DateSchedule.D_DayPrice,
                            N_DayPrice = uds.DateSchedule.N_DayPrice,
                            T_DayPrice = uds.DateSchedule.T_DayPrice,
                            UserId = uds.DateSchedule.User_Id,
                            UserName = uds.DateSchedule.User?.Name,
                            WorkRollId = uds.DateSchedule.Work_Roll_Id,
                            WorkRollName = uds.DateSchedule.WorkRoll?.Name,
                            DayShiftId = uds.DateSchedule.Day_Shift_Id,
                            U_Confirm_Flg = uds.UserShift?.U_Confirm_Flg
                        }
                    }).ToList()
                }).ToList()
            }).ToList();

            return Ok(dtoList);
        }


        [HttpGet("UserSchedule/Day/{userId}/{date}")]
        public async Task<IActionResult> GetUserDailySchedule(int userId, DateTime date)
        {
            var schedule = await _context.Date_Schedules
                .Where(ds => ds.User_Id == userId && ds.Today.Date == date.Date)
                .Include(ds => ds.WorkRoll)
                .Include(ds => ds.DayShift)
                .Include(ds => ds.User)
                .Include(ds => ds.UserDateShifts)
                    .ThenInclude(uds => uds.UserShift)
                        .ThenInclude(us => us.AllShift)
                .FirstOrDefaultAsync();

            if (schedule == null)
                return NotFound();

            var userShift = schedule.UserDateShifts?
                .FirstOrDefault()?.UserShift;

            var dto = new DateScheduleDto
            {
                Id = schedule.Id,
                Today = schedule.Today,
                P_Start_WorkTime = schedule.P_Start_WorkTime,
                P_End_WorkTime = schedule.P_End_WorkTime,
                U_Start_WorkTime = schedule.U_Start_WorkTime,
                U_End_WorkTime = schedule.U_End_WorkTime,
                Start_WorkTime = schedule.Start_WorkTime,
                End_WorkTime = schedule.End_WorkTime,
                Start_BreakTime = schedule.Start_BreakTime,
                End_BreakTime = schedule.End_BreakTime,
                T_WorkTime_D = schedule.T_WorkTime_D,
                T_WorkTime_N = schedule.T_WorkTime_N,
                T_WorkTime_All = schedule.T_WorkTime_All,
                D_DayPrice = schedule.D_DayPrice,
                N_DayPrice = schedule.N_DayPrice,
                T_DayPrice = schedule.T_DayPrice,
                UserId = schedule.User_Id,
                UserName = schedule.User?.Name,
                WorkRollId = schedule.Work_Roll_Id,                         // ✅ 追加済み
                WorkRollName = schedule.WorkRoll?.Name,
                DayShiftId = schedule.Day_Shift_Id,
                U_Confirm_Flg = userShift?.U_Confirm_Flg                  // 確認フラグ
            };

            return Ok(dto);
        }



        [HttpGet("UserSchedule/Month/{userId}/{year}/{month}")]
        public async Task<IActionResult> GetUserMonthlySchedules(int userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var schedules = await _context.Date_Schedules
                .Where(ds => ds.User_Id == userId && ds.Today >= startDate && ds.Today < endDate)
                .Include(ds => ds.WorkRoll)
                .Include(ds => ds.User)
                .Include(ds => ds.DayShift)
                .Include(ds => ds.UserDateShifts)
                    .ThenInclude(uds => uds.UserShift)
                        .ThenInclude(us => us.AllShift)
                .ToListAsync();

            // Dayごとにグループ化
            var dayShiftGroupedDtos = schedules
                .GroupBy(ds => ds.DayShift?.Id ?? 0)
                .Select(g =>
                {
                    var anyDay = g.FirstOrDefault()?.DayShift;

                    return new DayShiftDto
                    {
                        Id = anyDay?.Id ?? 0,
                        Date = anyDay?.Date ?? DateTime.MinValue,
                        Sum_WorkTime = anyDay?.Sum_WorkTime ?? TimeSpan.Zero,
                        Sum_TotalCost = anyDay?.Sum_TotalCost ?? 0,
                        DateSchedules = g.Select(ds => new DateScheduleDto
                        {
                            Id = ds.Id,
                            Today = ds.Today,
                            P_Start_WorkTime = ds.P_Start_WorkTime,
                            P_End_WorkTime = ds.P_End_WorkTime,
                            U_Start_WorkTime = ds.U_Start_WorkTime,
                            U_End_WorkTime = ds.U_End_WorkTime,
                            Start_WorkTime = ds.Start_WorkTime,
                            End_WorkTime = ds.End_WorkTime,
                            Start_BreakTime = ds.Start_BreakTime,
                            End_BreakTime = ds.End_BreakTime,
                            T_WorkTime_D = ds.T_WorkTime_D,
                            T_WorkTime_N = ds.T_WorkTime_N,
                            T_WorkTime_All = ds.T_WorkTime_All,
                            D_DayPrice = ds.D_DayPrice,
                            N_DayPrice = ds.N_DayPrice,
                            T_DayPrice = ds.T_DayPrice,
                            UserId = ds.User_Id,
                            UserName = ds.User?.Name,
                            WorkRollId = ds.Work_Roll_Id,
                            WorkRollName = ds.WorkRoll?.Name,
                            DayShiftId = ds.Day_Shift_Id,
                            U_Confirm_Flg = ds.UserDateShifts?.FirstOrDefault()?.UserShift?.U_Confirm_Flg
                        }).ToList()
                    };
                })
                .OrderBy(ds => ds.Date)
                .ToList();

            return Ok(dayShiftGroupedDtos);
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
                            Work_Roll_Id = 0,//初期値は0で固定
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
        //希望シフトの提出
        [HttpPut("user/{userId}/schedule-update")]
        public async Task<IActionResult> UpdateUserShiftAndSchedule(int userId, [FromBody] UpdateUserShiftRequest request)
        {
            // 該当ユーザーの DateSchedule を取得
            var dateSchedule = await _context.Date_Schedules
                .FirstOrDefaultAsync(ds => ds.User_Id == userId && ds.Today.Date == request.TargetDate.Date);

            if (dateSchedule == null)
            {
                return NotFound("DateSchedule not found.");
            }

            // 勤務希望時間を更新
            dateSchedule.U_Start_WorkTime = request.U_Start_WorkTime;
            dateSchedule.U_End_WorkTime = request.U_End_WorkTime;

            // 該当ユーザーのUserShiftを取得
            var userShift = await _context.User_Shifts
                .FirstOrDefaultAsync(us => us.User_Id == userId && us.Date_Ym.Month == request.TargetDate.Month && us.Date_Ym.Year == request.TargetDate.Year);

            if (userShift == null)
            {
                return NotFound("UserShift not found.");
            }

            // 確認フラグを更新
            userShift.U_Confirm_Flg = request.U_Confirm_Flg;

            // DB保存
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //予定シフトの設定
        [HttpPut("user/{userId}/schedule-plan")]
        public async Task<IActionResult> UpdatePlannedWorkTime(int userId, [FromBody] UpdatePlannedWorkTimeRequest request)
        {
            var dateSchedule = await _context.Date_Schedules
                .FirstOrDefaultAsync(ds => ds.User_Id == userId && ds.Today.Date == request.TargetDate.Date);

            if (dateSchedule == null)
            {
                return NotFound("DateSchedule not found.");
            }

            dateSchedule.Work_Roll_Id = request.Work_Roll_Id;
            dateSchedule.P_Start_WorkTime = request.P_Start_WorkTime;
            dateSchedule.P_End_WorkTime = request.P_End_WorkTime;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        //AllShiftsの各フラグ更新
        //シフト確定フラグ
        [HttpPut("allshift/{id}/confirm-flag")]
        public async Task<IActionResult> UpdateConfirmFlag(int id, [FromBody] bool confirmFlg)
        {
            var allShift = await _context.All_Shifts.FindAsync(id);
            if (allShift == null)
                return NotFound("AllShift not found.");

            allShift.Confirm_Flg = confirmFlg;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //送付フラグ
        [HttpPut("allshift/{id}/sending-flag")]
        public async Task<IActionResult> UpdateSendingFlag(int id, [FromBody] bool sendingFlg)
        {
            var allShift = await _context.All_Shifts.FindAsync(id);
            if (allShift == null)
                return NotFound("AllShift not found.");

            allShift.Sending_Flg = sendingFlg;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //シフト希望募集中フラグ
        [HttpPut("allshift/{id}/rec-flag")]
        public async Task<IActionResult> UpdateRecFlag(int id, [FromBody] bool recFlg)
        {
            var allShift = await _context.All_Shifts.FindAsync(id);
            if (allShift == null)
                return NotFound("AllShift not found.");

            allShift.Rec_Flg = recFlg;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //Put処理
    }
} 