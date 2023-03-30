using AttendenceManagement.Infrastructure.IRepository;
using AttendenceManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AttendenceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeAPIController : ControllerBase
    {
        private IWebHostEnvironment _environment;
        private readonly IDatasheet _iEmp;
        private readonly ILogProcess logProcess;

        public HomeAPIController(IWebHostEnvironment environment, IDatasheet iEmp, ILogProcess logProcess)
        {
            _environment = environment;
            _iEmp = iEmp;
            this.logProcess = logProcess;
           
        }


        [HttpPost]
        [Route("PostExcelFile")]
        public async Task<IActionResult> Index()
        {
            try
            {

                var form = await Request.ReadFormAsync();
                var file = form.Files.GetFile("file");
                if (file == null)
                {
                    return Problem("Please Upload file");
                }

                string path = _iEmp.Documentupload(file);
                DataTable dt = _iEmp.EmployeeData(path);
                _iEmp.ImportEmployee(dt);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        [HttpPost]
        [Route("LogProcess")]
        public async Task<IActionResult> LogProcess(ViewModels.MonthYearModel monthYear)
        {
            try
            {

                var month = monthYear.month;
                var year = monthYear.year;
                var list = await logProcess.Log(month, year);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        [HttpGet]
        [Route("GetList")]
        public async Task<IActionResult> WorkHourReport()
        {
            var list = await logProcess.WorkHourReport();
            return Ok(list);
        }
    }
}