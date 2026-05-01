using SQLite;

namespace Task_Roster.Models;

public class ShiftModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Role { get; set; } = "";

    public string Location { get; set; } = "";

    public string Employee { get; set; } = "";

    public string Status { get; set; } = "Pending";
}