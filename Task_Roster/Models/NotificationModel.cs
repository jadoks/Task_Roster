namespace Task_Roster.Models;

public class NotificationModel
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Time { get; set; } = "";
    public string Type { get; set; } = ""; // Shift / Task
    public DateTime Date { get; set; }
}