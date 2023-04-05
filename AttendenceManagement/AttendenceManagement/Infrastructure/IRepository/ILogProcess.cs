using AttendenceManagement.Models;
using AttendenceManagement.ViewModels;

namespace AttendenceManagement.Infrastructure.IRepository
{
    public interface ILogProcess
    {
        //public Task<List<AttAttendanceRegister>> Log(string month, string year);
        public Task<int> LogProcess(string month, string year);
        public Task<List<ReportViewModel>> ReportsAsync();
    }
}
