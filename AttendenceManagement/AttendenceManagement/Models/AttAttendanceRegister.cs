namespace AttendenceManagement.Models;

public partial class AttAttendanceRegister
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime AttDate { get; set; }

    public decimal LateInMinute { get; set; }

    public decimal EartlyOutMinute { get; set; }

    public decimal Workhour { get; set; }

    public decimal? OverTimeMinute { get; set; }

    public bool? IsLeave { get; set; }

    public bool? IsHoliday { get; set; }

    public bool? IsWeekOff { get; set; }

    public DateTime? Processdate { get; set; }

    public string? Processby { get; set; }
}
