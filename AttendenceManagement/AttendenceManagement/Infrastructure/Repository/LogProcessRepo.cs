using AttendenceManagement.Infrastructure.IRepository;
using AttendenceManagement.Models;
using AttendenceManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Reflection.PortableExecutable;

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
        public async Task<List<AttAttendanceRegister>> Log(string month, string year)
        {
            var data = await (from x in db.AttAttendanceLogs
                              where x.AttDate.Month.ToString() == month && x.AttDate.Year.ToString() == year
                              select x).ToListAsync();
            //user Login, lunch, lunchEnd, Departure Time
            TimeSpan defTime = Convert.ToDateTime("00:00:00").TimeOfDay;
            TimeSpan logINTime = defTime;
            TimeSpan UserDeptTime = defTime;
            for (int j = 0; j < data.Count; j++)
            {
                var data1 = (from z in data
                             where z.UserId == data[j].UserId && z.AttDate == data[j].AttDate
                             select z).ToList();

                List<DateTime> logTimes = new List<DateTime>();
                for (int k = 0; k < data1.Count; k++)
                {
                    for (int l = 0; l < data1.Count; l++)
                    {
                        DateTime logTime = data1[l].LogTime;
                        logTimes.Add(logTime);
                    }
                    logTimes.Sort();
                    logINTime = logTimes.First().TimeOfDay;
                    UserDeptTime = logTimes.Last().TimeOfDay;
                    

                    if (logINTime == defTime || UserDeptTime == defTime)
                    {
                        continue;
                    }
                    else
                    {
                        var check = await db.AttAttendanceRegisters.AnyAsync(x => x.UserId == data1[k].UserId && x.AttDate == data1[k].AttDate);
                        if (!check)
                        {
                            var latetime = defTime.TotalMinutes;
                            if (logINTime > Convert.ToDateTime("10:00:00 AM").TimeOfDay)
                            {
                                latetime = (logINTime - Convert.ToDateTime("10:00:00 AM").TimeOfDay).TotalMinutes;
                            }
                            else
                            {
                                latetime = 0;
                            }
                            var workTime = Convert.ToDecimal((UserDeptTime.TotalMinutes - logINTime.TotalMinutes));
                            var hour = workTime / 60;
                            // var workHour = Math.Round((workTime / 60), 2);
                            var overtime = Convert.ToDecimal(workTime - 540);
                            var earlyout = (Convert.ToDateTime("07:00:00 PM").TimeOfDay - UserDeptTime).Minutes;

                            var b = (UserDeptTime.Subtract(logINTime)).ToString().Split(':');
                            var h1 = Convert.ToDecimal(b[0].ToString() + "." + b[1].ToString());

                            AttAttendanceRegister register = new AttAttendanceRegister();
                            register.UserId = data1[k].UserId;
                            register.UserName = data1[k].Username;
                            register.Processdate = DateTime.Now;
                            register.AttDate = data1[k].AttDate.Date;
                            register.Processby = configuration["CreatedBy"];
                            register.LateInMinute = Convert.ToDecimal(latetime);
                            register.Workhour = h1;

                            if (overtime < 0) { register.OverTimeMinute = 0; }
                            else { register.OverTimeMinute = overtime; }

                            if (earlyout < 0) { register.EartlyOutMinute = 0; }
                            else { register.EartlyOutMinute = earlyout; }
                            // isLeave, isWeekEndOff, isHoliday 

                            db.AttAttendanceRegisters.Add(register);
                            db.SaveChanges();

                        }
                        logINTime = defTime;
                        UserDeptTime = defTime;
                    }
                }
            }
            var list = db.AttAttendanceRegisters.ToList();
            return list;
        }

        public async Task<List<ReportViewModel>> WorkHourReport()
        {
            try
            {
                var data = await OutputList();
                return data;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ReportViewModel>> OutputList()
        {
            List<ReportViewModel> list = new List<ReportViewModel>();
            var data = await db.AttAttendanceRegisters.ToListAsync();

            //official time of office
            TimeSpan ArrivalTime = Convert.ToDateTime("10:00:00").TimeOfDay;
            //user Login, lunch, lunchEnd, Departure Time
            TimeSpan defTime = Convert.ToDateTime("00:00:00").TimeOfDay;
            TimeSpan logINTime = defTime;

            var UserDeptTime = defTime;

            for (int i = 0; i < data.Count; i++)
            {
                var fatchdata = (from x in data where x.UserId == data[i].UserId && x.AttDate == data[i].AttDate select x).ToList();
                for (int k = 0; k < fatchdata.Count; k++)
                {
                    var dataintbl = (from x in db.AttAttendanceLogs where x.UserId == fatchdata[k].UserId && x.AttDate == fatchdata[k].AttDate select x).ToList();
                    List<DateTime> logTimes = new List<DateTime>();
                    foreach (var item in fatchdata)
                    {
                        for (int j = 0; j < dataintbl.Count; j++)
                        {
                            DateTime logTime = dataintbl[j].LogTime;
                            logTimes.Add(logTime);
                        }
                        logTimes.Sort();
                        logINTime = logTimes.First().TimeOfDay;
                        UserDeptTime = logTimes.Last().TimeOfDay;
                        
                        var latetime = defTime.TotalMinutes;
                        if (logINTime > Convert.ToDateTime("10:00:00 AM").TimeOfDay)
                        {
                            latetime = (logINTime - Convert.ToDateTime("10:00:00 AM").TimeOfDay).TotalMinutes;
                        }
                        else
                        {
                            latetime = 0;
                        }
                        var workTime = Convert.ToDecimal((UserDeptTime.TotalMinutes - logINTime.TotalMinutes));
                        var b = (UserDeptTime.Subtract(logINTime)).ToString().Split(':');
                        var h1 = Convert.ToDecimal(b[0].ToString() + "." + b[1].ToString());
                        // var workHour = Math.Round((workTime / 60), 2);
                        var overtime = Convert.ToDecimal(workTime - 540);
                        var earlyout = (Convert.ToDateTime("07:00:00 PM").TimeOfDay - UserDeptTime).Minutes;
                        ReportViewModel reportView = new ReportViewModel();
                        reportView.UserID = item.UserId;
                        reportView.Username = item.UserName;
                        reportView.AttDate = item.AttDate;
                        reportView.LoginTime = logINTime;
                        reportView.LogOutTIme = UserDeptTime;
                        reportView.LateInMinute = Convert.ToDecimal(latetime);
                        reportView.Workhour = h1;
                        if (overtime < 0) { reportView.OverTimeMinute = 0; }
                        else { reportView.OverTimeMinute = overtime; }
                        if (earlyout < 0) { reportView.EartlyOutMinute = 0; }
                        else { reportView.EartlyOutMinute = earlyout; }
                        // isLeave, isWeekEndOff, isHoliday 
                        list.Add(reportView);
                    }
                    logINTime = defTime;
                    UserDeptTime = defTime;
                }
            }
            return list;
        }

        //public async Task<List<ReportViewModel>> OutputTimings()
        //{
        //    try
        //    {
        //        List<ReportViewModel> list = new List<ReportViewModel>();
        //        var data = db.AttAttendanceLogs.ToList();
        //        var ArrivalTime = Convert.ToDateTime("09:30:00 AM");
        //        var ArrivalEndTime = Convert.ToDateTime("10:30:00 AM");
        //        var DeptTime = Convert.ToDateTime("06:30:00 PM");
        //        var DeptEndTime = Convert.ToDateTime("07:30:00 PM");
        //        var LunchTime = Convert.ToDateTime("01:00:00 PM");
        //        var LunchEndTime = Convert.ToDateTime("03:15:00 PM");

        //        //user Login, lunch, lunchEnd, Departure Time
        //        TimeSpan defTime = Convert.ToDateTime("00:00:00").TimeOfDay;

        //        TimeSpan logINTime = defTime;
        //        var lunchTime = defTime;
        //        var UserLunchEndTime = defTime;
        //        var UserDeptTime = defTime;

        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            var fatchdata = (from x in data where x.UserId == data[i].UserId && x.AttDate == data[i].AttDate select x).ToList();


        //            for (int k = 0; k < fatchdata.Count; k++)
        //            {
        //                if ((fatchdata[k].LogTime.TimeOfDay >= ArrivalTime.TimeOfDay && fatchdata[k].LogTime.TimeOfDay < ArrivalEndTime.TimeOfDay)) { logINTime = fatchdata[k].LogTime.TimeOfDay; }

        //                // if ((fatchdata[k].LogTime.TimeOfDay >= LunchTime.TimeOfDay && fatchdata[k].LogTime.TimeOfDay < Convert.ToDateTime("02:00:00").TimeOfDay)) { lunchTime = fatchdata[k].LogTime.TimeOfDay; }

        //                //  if ((fatchdata[k].LogTime.TimeOfDay >= Convert.ToDateTime("02:15:00 PM").TimeOfDay && fatchdata[k].LogTime.TimeOfDay < LunchEndTime.TimeOfDay)) { UserLunchEndTime = fatchdata[k].LogTime.TimeOfDay; }

        //                if (fatchdata[k].LogTime.TimeOfDay >= DeptTime.TimeOfDay && fatchdata[k].LogTime.TimeOfDay < DeptEndTime.TimeOfDay) { UserDeptTime = fatchdata[k].LogTime.TimeOfDay; }

        //                if (logINTime == defTime || UserDeptTime == defTime)
        //                {
        //                    continue;
        //                }
        //                else
        //                {

        //                    var checkdata = list.Any(x => x.UserID == fatchdata[k].UserId && x.AttDate == fatchdata[k].AttDate);
        //                    if (!checkdata)
        //                    {
        //                        var latetime = 0;
        //                        if (logINTime > Convert.ToDateTime("10:00:00 AM").TimeOfDay)
        //                        {
        //                            latetime = (logINTime - Convert.ToDateTime("10:00:00 AM").TimeOfDay).Minutes;
        //                        }
        //                        else
        //                        {
        //                            latetime = 0;
        //                        }
        //                        var workTime = Convert.ToDecimal((UserDeptTime.TotalMinutes - logINTime.TotalMinutes));
        //                        var hour = Math.Round(workTime / 60);

        //                        var workHour = Math.Round((workTime / 60), 2);
        //                        var overtime = Convert.ToDecimal(workTime - 540);
        //                        var earlyout = (Convert.ToDateTime("07:00:00 PM").TimeOfDay - UserDeptTime).Minutes;

        //                        var dataintbl = (from x in db.AttAttendanceRegisters where x.UserId == fatchdata[k].UserId && x.AttDate == fatchdata[k].AttDate select x).ToList();
        //                        foreach (var item in dataintbl)
        //                        {

        //                            ReportViewModel reportView = new ReportViewModel();
        //                            reportView.UserID = fatchdata[k].UserId;
        //                            reportView.Username = fatchdata[k].Username;
        //                            reportView.AttDate = fatchdata[k].AttDate;
        //                            reportView.LoginTime = logINTime;
        //                            reportView.LogOutTIme = UserDeptTime;
        //                            reportView.LunchTIme = lunchTime;
        //                            reportView.LunchEndTime = UserLunchEndTime;
        //                            reportView.LateInMinute = item.LateInMinute;
        //                            reportView.Workhour = item.Workhour;
        //                            if (overtime < 0) { reportView.OverTimeMinute = 0; }
        //                            else { reportView.OverTimeMinute = item.OverTimeMinute; }

        //                            if (earlyout < 0) { reportView.EartlyOutMinute = 0; }
        //                            else { reportView.EartlyOutMinute = item.EartlyOutMinute; }

        //                            list.Add(reportView);

        //                        }
        //                        logINTime = defTime;
        //                        lunchTime = defTime;
        //                        UserLunchEndTime = defTime;
        //                        UserDeptTime = defTime;
        //                    }
        //                }
        //            }
        //        }
        //        return list;
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //}

    }
}
