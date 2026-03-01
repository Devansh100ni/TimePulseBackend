using System.Data;

namespace AttendenceManagement.Infrastructure.IRepository
{
    public interface IDatasheet
    {
        string Documentupload(IFormFile formFile);
        DataTable EmployeeData(string path);
        void ImportEmployee(DataTable employee);
    }
}
