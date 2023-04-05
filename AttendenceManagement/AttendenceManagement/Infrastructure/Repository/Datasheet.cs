using AttendenceManagement.Infrastructure.IRepository;
using AttendenceManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.OleDb;

namespace AttendenceManagement.Infrastructure.Repository
{
    public class Datasheet : IDatasheet
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly AttendanceManagement001Context db;

        public Datasheet(IConfiguration configuration, IWebHostEnvironment hostEnvironment, AttendanceManagement001Context db)
        {
            this.configuration = configuration;
            this.hostEnvironment = hostEnvironment;
            this.db = db;

        }


        public async Task<DataTable> EmployeeData(string path)
        {
            try
            {
                var connStr = configuration.GetConnectionString("excelconnection");
                DataTable dataTable = new DataTable();
                connStr = string.Format(connStr, path);

                using (OleDbConnection excelconn = new OleDbConnection(connStr))
                {
                    using (OleDbCommand cmd = new OleDbCommand())
                    {
                        using (OleDbDataAdapter adptExcel = new OleDbDataAdapter())
                        {
                            await excelconn.OpenAsync();
                            cmd.Connection = excelconn;
                            DataTable excelSchema;
                            excelSchema = excelconn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            var sheetname = excelSchema.Rows[0]["Table_Name"].ToString();
                            await excelconn.CloseAsync();

                            await excelconn.OpenAsync();
                            cmd.CommandText = "SELECT * From [" + sheetname + "]";
                            adptExcel.SelectCommand = cmd;
                            adptExcel.Fill(dataTable);
                            await excelconn.CloseAsync();
                        }
                    }
                }
                return dataTable;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> Documentupload(IFormFile formFile)
        {
            try
            {
                string extension = Path.GetExtension(Path.GetFileName(formFile.FileName));
                string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };
                if (!allowedExtsnions.Contains(extension))
                {
                    throw new Exception("Sorry! This file is not allowed, make sure that file having extension as either.xls or.xlsx is uploaded.");
                }

                string uploadpath = hostEnvironment.ContentRootPath;
                string dest_path = Path.Combine(uploadpath, "uploaded_doc");

                if (!Directory.Exists(dest_path))
                {
                    Directory.CreateDirectory(dest_path);
                }
                string sourcefile = Path.GetFileName(Guid.NewGuid() + "_/_" + formFile.FileName);
                string path = Path.Combine(dest_path, sourcefile);
                using (FileStream filestream = new FileStream(path, FileMode.Create))
                {
                    await formFile.CopyToAsync(filestream);
                }
                return path;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> ImportEmployee(DataTable employee)
        {
            try
            {
                var rawsEffected = 0;
                for (int i = 0; i < employee.Rows.Count; i++)
                {
                    var userID = employee.Rows[i][0].ToString();
                    var username = employee.Rows[i][1].ToString();
                    var attDate = employee.Rows[i][2].ToString();

                    if (userID != "" && username != "" && attDate != null && attDate != "")
                    {
                        AttAttendanceLog log = new AttAttendanceLog();
                        log.UserId = userID;
                        log.Username = username;
                        log.AttDate = Convert.ToDateTime(employee.Rows[i][2].ToString()).Date;
                        log.LogTime = Convert.ToDateTime(employee.Rows[i][2].ToString());
                        log.Createdby = configuration["CreatedBy"];
                        log.Createddate = DateTime.Now.Date;

                        if (!await db.AttAttendanceLogs.AnyAsync(x => x.UserId == log.UserId && x.LogTime == log.LogTime && x.AttDate == log.AttDate))
                        {
                            await db.AttAttendanceLogs.AddAsync(log);
                            rawsEffected += await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                return rawsEffected;
            }
            catch
            {
                throw;
            }
        }
    }
}
