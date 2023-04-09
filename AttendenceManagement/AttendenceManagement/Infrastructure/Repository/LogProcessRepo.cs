using AttendenceManagement.Infrastructure.IRepository;
using AttendenceManagement.Models;
using AttendenceManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AttendenceManagement.Infrastructure.Repository
{
    public class LogProcessRepo : ILogProcess
    {
        private readonly AttendanceManagement001Context db;
        private readonly IConfiguration configuration;

        public LogProcessRepo(AttendanceManagement001Context db, IConfiguration configuration)
        {
            this.db = db;
            this.configuration = configuration;
        }

        public async Task<int> LogProcess(string month, string year)
        {
            TimeSpan userLogIn = TimeSpan.Zero;
            TimeSpan userLogOut = TimeSpan.Zero;
            int rawsAffected = 0;

            var attAttendanceLogsData = await db.AttAttendanceLogs.Where(x => x.AttDate.Month.ToString() == month && x.AttDate.Year.ToString() == year).ToListAsync();
            foreach (var dataInLogs in attAttendanceLogsData)
            {
                var sameDataOfUserAtSomeDate = attAttendanceLogsData.Where(x => x.UserId == dataInLogs.UserId && x.AttDate == dataInLogs.AttDate).ToList();
                var checkIfUserExist = db.AttAttendanceRegisters.Any(x => x.UserId == dataInLogs.UserId && x.AttDate == dataInLogs.AttDate);
                if (!checkIfUserExist)
                {
                    var logTimes = sameDataOfUserAtSomeDate.Select(x => x.LogTime.TimeOfDay).ToList();
                    logTimes.Sort();
                    userLogIn = logTimes.First();
                    userLogOut = logTimes.Last();

                    var lateTime = (userLogIn - Convert.ToDateTime("10:00:00 AM").TimeOfDay).TotalMinutes;

                    var workTime = Convert.ToDecimal((userLogOut - userLogIn).TotalMinutes);
                    var hour = workTime / 60;
                    var overtime = Convert.ToDecimal(workTime - 540);
                    var earlyout = (Convert.ToDateTime("07:00:00 PM").TimeOfDay - userLogOut).Minutes;

                    var wH = (userLogOut.Subtract(userLogIn)).ToString().Split(':');
                    var WorkHours = Convert.ToDecimal(wH[0].ToString() + "." + wH[1].ToString());

                    await db.AttAttendanceRegisters.AddAsync(new AttAttendanceRegister
                    {
                        UserId = dataInLogs.UserId,
                        UserName = dataInLogs.Username,
                        Processdate = DateTime.Now,
                        AttDate = dataInLogs.AttDate.Date,
                        Processby = configuration["CreatedBy"],
                        LateInMinute = Convert.ToDecimal(lateTime > 0 ? lateTime : 0),
                        Workhour = WorkHours,
                        OverTimeMinute = overtime > 0 ? overtime : 0,
                        EartlyOutMinute = earlyout > 0 ? earlyout : 0,
                    });
                    rawsAffected += await db.SaveChangesAsync();
                }
            }
            return rawsAffected;
        }

        //public async Task<List<AttAttendanceRegister>> Log(string month, string year)
        //{
        //    var data = await (from x in db.AttAttendanceLogs
        //                      where x.AttDate.Month.ToString() == month && x.AttDate.Year.ToString() == year
        //                      select x).ToListAsync();
        //    //user Login, lunch, lunchEnd, Departure Time
        //    TimeSpan defTime = Convert.ToDateTime("00:00:00").TimeOfDay;
        //    TimeSpan logINTime = defTime;
        //    TimeSpan UserDeptTime = defTime;
        //    for (int j = 0; j < data.Count; j++)
        //    {
        //        var data1 = (from z in data
        //                     where z.UserId == data[j].UserId && z.AttDate == data[j].AttDate
        //                     select z).ToList();

        //        List<DateTime> logTimes = new List<DateTime>();
        //        for (int k = 0; k < data1.Count; k++)
        //        {

        //            for (int l = 0; l < data1.Count; l++)
        //            {
        //                DateTime logTime = data1[l].LogTime;
        //                logTimes.Add(logTime);
        //            }
        //            logTimes.Sort();
        //            logINTime = logTimes.First().TimeOfDay;
        //            UserDeptTime = logTimes.Last().TimeOfDay;


        //            if (logINTime == defTime || UserDeptTime == defTime)
        //            {
        //                continue;
        //            }
        //            else
        //            {
        //                var check = await db.AttAttendanceRegisters.AnyAsync(x => x.UserId == data1[k].UserId && x.AttDate == data1[k].AttDate);
        //                if (!check)
        //                {
        //                    var latetime = defTime.TotalMinutes;
        //                    if (logINTime > Convert.ToDateTime("10:00:00 AM").TimeOfDay)
        //                    {
        //                        latetime = (logINTime - Convert.ToDateTime("10:00:00 AM").TimeOfDay).TotalMinutes;
        //                    }
        //                    else
        //                    {
        //                        latetime = 0;
        //                    }
        //                    var workTime = Convert.ToDecimal((UserDeptTime.TotalMinutes - logINTime.TotalMinutes));
        //                    var hour = workTime / 60;
        //                    // var workHour = Math.Round((workTime / 60), 2);
        //                    var overtime = Convert.ToDecimal(workTime - 540);
        //                    var earlyout = (Convert.ToDateTime("07:00:00 PM").TimeOfDay - UserDeptTime).Minutes;

        //                    var b = (UserDeptTime.Subtract(logINTime)).ToString().Split(':');
        //                    var h1 = Convert.ToDecimal(b[0].ToString() + "." + b[1].ToString());

        //                    AttAttendanceRegister register = new AttAttendanceRegister();
        //                    register.UserId = data1[k].UserId;
        //                    register.UserName = data1[k].Username;
        //                    register.Processdate = DateTime.Now;
        //                    register.AttDate = data1[k].AttDate.Date;
        //                    register.Processby = configuration["CreatedBy"];
        //                    register.LateInMinute = Convert.ToDecimal(latetime);
        //                    register.Workhour = h1;

        //                    if (overtime < 0) { register.OverTimeMinute = 0; }
        //                    else { register.OverTimeMinute = overtime; }

        //                    if (earlyout < 0) { register.EartlyOutMinute = 0; }
        //                    else { register.EartlyOutMinute = earlyout; }
        //                    // isLeave, isWeekEndOff, isHoliday 

        //                    db.AttAttendanceRegisters.Add(register);
        //                    db.SaveChanges();

        //                }
        //                logINTime = defTime;
        //                UserDeptTime = defTime;
        //            }
        //        }
        //    }
        //    var list = db.AttAttendanceRegisters.ToList();
        //    return list;
        //}

        public async Task<List<ReportViewModel>> ReportsAsync()
        {
            TimeSpan userLogIn = TimeSpan.Zero;
            TimeSpan userLogOut = TimeSpan.Zero;
            List<ReportViewModel> reportsDataList = new List<ReportViewModel>();
            var data = await db.AttAttendanceRegisters.ToListAsync();
            foreach (var item in data)
            {
                var checkUserInList = reportsDataList.Any(x => x.UserID == item.UserId && x.AttDate == item.AttDate);
                if (!checkUserInList)
                {
                    var takeLogTimes = await (from x in db.AttAttendanceLogs where x.UserId == item.UserId && x.AttDate == item.AttDate select x).ToListAsync();
                    var logTimes = takeLogTimes.Select(x => x.LogTime.TimeOfDay).ToList();
                    logTimes.Sort();
                    userLogIn = logTimes.First();
                    userLogOut = logTimes.Last();
                    var userData = await db.AttAttendanceRegisters.Where(x => x.UserId == item.UserId && x.AttDate == item.AttDate).FirstOrDefaultAsync();
                    if (userData != null)
                    {
                        reportsDataList.Add(new ReportViewModel
                        {
                            UserID = userData.UserId,
                            Username = userData.UserName,
                            AttDate = userData.AttDate,
                            LoginTime = userLogIn,
                            LogOutTIme = userLogOut,
                            LateInMinute = userData.LateInMinute,
                            EartlyOutMinute = userData.EartlyOutMinute,
                            Workhour = userData.Workhour,
                            OverTimeMinute = userData.OverTimeMinute,
                        });
                    }
                }
                userLogIn = TimeSpan.Zero;
                userLogOut = TimeSpan.Zero;
            }
            return reportsDataList;
        }
    }
}
