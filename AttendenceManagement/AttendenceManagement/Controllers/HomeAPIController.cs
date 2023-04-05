using AttendenceManagement.Infrastructure.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AttendenceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeAPIController : ControllerBase
    {
        private readonly IDatasheet _iEmp;
        private readonly ILogProcess logProcess;

        public HomeAPIController(IDatasheet iEmp, ILogProcess logProcess)
        {
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

                string path = await _iEmp.Documentupload(file);
                DataTable dt = await _iEmp.EmployeeData(path);
                var rowsEffected = await _iEmp.ImportEmployee(dt);
                return Ok(rowsEffected);
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
                var rawsAffected = await logProcess.LogProcess(month, year);
                return Ok(rawsAffected);
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
            var list = await logProcess.ReportsAsync();
            //var jsonList = await Task.Run(() => JsonConvert.SerializeObject(list));
            return Ok(list);
        }
    }
}