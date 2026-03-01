namespace AttendenceManagement.ViewModels
{
    public class ReportViewModel
    {
        public string? UserID { get; set; }
        public string Username { get; set; } = null!;

        public DateTime AttDate { get; set; }

        public TimeSpan LoginTime { get; set; }
        public TimeSpan LogOutTIme { get; set; }
        public decimal LateInMinute { get; set; }
        public decimal EartlyOutMinute { get; set; }
        public decimal Workhour { get; set; }
        public decimal? OverTimeMinute { get; set; }
    }
}
