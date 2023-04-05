namespace AttendenceManagement.Models;

public partial class AttAttendanceLog
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public DateTime AttDate { get; set; }

    public DateTime LogTime { get; set; }

    public string Createdby { get; set; } = null!;

    public DateTime? Createddate { get; set; }
}
