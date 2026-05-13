using SQLite;

namespace Task_Roster.Models;

public class UserModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public byte[]? ProfileImageBytes { get; set; }

    // This property allows your ScheduleView and DatabaseService to use "Name"
    // without you having to change every single line of code.
    [Ignore]
    public string Name => $"{FirstName} {LastName}".Trim();
}