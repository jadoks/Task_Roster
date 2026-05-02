namespace Task_Roster.Models;

public class ShiftDetailsDto
{
    public ShiftModel Shift { get; set; }
    public List<TaskModel> Tasks { get; set; }
}