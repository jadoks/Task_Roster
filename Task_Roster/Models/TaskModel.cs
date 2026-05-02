using SQLite;

namespace Task_Roster.Models;

public class TaskModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ShiftId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Priority { get; set; } = "Medium";

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}