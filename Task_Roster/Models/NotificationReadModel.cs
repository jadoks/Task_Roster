using SQLite;

namespace Task_Roster.Models;

public class NotificationReadModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string UserEmail { get; set; } = "";

    public string NotificationKey { get; set; } = "";

    public DateTime ReadAt { get; set; } = DateTime.Now;
}