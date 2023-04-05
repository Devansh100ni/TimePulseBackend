using System.Data;

namespace AttendenceManagement.Infrastructure.IRepository
{
    public interface IDatasheet
    {
        public Task<string> Documentupload(IFormFile formFile);
        public Task<DataTable> EmployeeData(string path);
        public Task<int> ImportEmployee(DataTable employee);
    }
}
