using SQLite;

namespace Task_Roster.Models;

public class EmployeeModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    // ADD THIS LINE
    public string Role { get; set; } = "";

    public string Skills { get; set; } = "";

    public int MaxHours { get; set; }

    public string Availability { get; set; } = "";
}