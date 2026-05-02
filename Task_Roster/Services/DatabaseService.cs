using SQLite;
using Task_Roster.Models;

namespace Task_Roster.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService()
    {
        string dbPath = Path.Combine(
            FileSystem.AppDataDirectory,
            "schedule.db");

        _database = new SQLiteAsyncConnection(dbPath);

        _database.CreateTableAsync<ShiftModel>().Wait();
        _database.CreateTableAsync<EmployeeModel>().Wait();
        _database.CreateTableAsync<UserModel>().Wait();
        _database.CreateTableAsync<TaskModel>().Wait();
        _database.CreateTableAsync<NotificationReadModel>().Wait();

        AddMissingUserColumns();
    }

    private void AddMissingUserColumns()
    {
        try
        {
            _database.ExecuteAsync(
                "ALTER TABLE UserModel ADD COLUMN ProfileImageBytes BLOB"
            ).Wait();
        }
        catch { }
    }

    // ---------------- USERS ----------------

    public async Task<int> AddUserAsync(UserModel user)
        => await _database.InsertAsync(user);

    public async Task<UserModel?> GetUserByEmailAsync(string email)
        => await _database.Table<UserModel>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();

    public async Task<List<UserModel>> GetUsersAsync()
        => await _database.Table<UserModel>().ToListAsync();

    public async Task<int> UpdateUserAsync(UserModel user)
        => await _database.UpdateAsync(user);

    public async Task<UserModel?> LoginUserAsync(string email, string password)
        => await _database.Table<UserModel>()
            .Where(u => u.Email == email && u.Password == password)
            .FirstOrDefaultAsync();

    public async Task<int> UpdatePasswordAsync(string email, string newPassword)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return 0;

        user.Password = newPassword;
        return await _database.UpdateAsync(user);
    }

    // ---------------- SHIFTS ----------------

    public async Task<int> AddShiftAsync(ShiftModel shift)
    {
        await _database.InsertAsync(shift);
        return shift.Id; // IMPORTANT FIX
    }

    public async Task<List<ShiftModel>> GetShiftsAsync()
        => await _database.Table<ShiftModel>().ToListAsync();

    public async Task<int> UpdateShiftAsync(ShiftModel shift)
        => await _database.UpdateAsync(shift);

    public async Task<int> DeleteShiftAsync(ShiftModel shift)
        => await _database.DeleteAsync(shift);

    // Delete shift WITH tasks
    public async Task DeleteShiftWithTasksAsync(ShiftModel shift)
    {
        var tasks = await GetTasksByShiftIdAsync(shift.Id);

        foreach (var task in tasks)
            await _database.DeleteAsync(task);

        await _database.DeleteAsync(shift);
    }

    // ---------------- EMPLOYEES ----------------

    public async Task AddEmployeeAsync(EmployeeModel employee)
        => await _database.InsertAsync(employee);

    public async Task<List<EmployeeModel>> GetEmployeesAsync()
        => await _database.Table<EmployeeModel>().ToListAsync();

    public async Task UpdateEmployeeAsync(EmployeeModel employee)
        => await _database.UpdateAsync(employee);

    public async Task DeleteEmployeeAsync(EmployeeModel employee)
        => await _database.DeleteAsync(employee);

    // ---------------- TASKS ----------------

    public async Task<int> AddTaskAsync(TaskModel task)
        => await _database.InsertAsync(task);

    public async Task<List<TaskModel>> GetTasksByShiftIdAsync(int shiftId)
        => await _database.Table<TaskModel>()
            .Where(t => t.ShiftId == shiftId)
            .ToListAsync();

    public async Task<int> UpdateTaskAsync(TaskModel task)
        => await _database.UpdateAsync(task);

    public async Task<int> DeleteTaskAsync(TaskModel task)
        => await _database.DeleteAsync(task);

    // ---------------- SHIFT DETAILS ----------------

    public async Task<ShiftDetailsDto> GetShiftDetailsAsync(int shiftId)
    {
        var shift = await _database.Table<ShiftModel>()
            .Where(s => s.Id == shiftId)
            .FirstOrDefaultAsync();

        var tasks = await GetTasksByShiftIdAsync(shiftId);

        return new ShiftDetailsDto
        {
            Shift = shift,
            Tasks = tasks
        };
    }

    public async Task<List<NotificationReadModel>> GetNotificationReadsByUserAsync(string userEmail)
    {
        return await _database.Table<NotificationReadModel>()
            .Where(n => n.UserEmail == userEmail)
            .ToListAsync();
    }

    public async Task<NotificationReadModel?> GetNotificationReadAsync(string userEmail, string notificationKey)
    {
        return await _database.Table<NotificationReadModel>()
            .Where(n => n.UserEmail == userEmail && n.NotificationKey == notificationKey)
            .FirstOrDefaultAsync();
    }

    public async Task<int> MarkNotificationReadAsync(string userEmail, string notificationKey)
    {
        var existing = await GetNotificationReadAsync(userEmail, notificationKey);

        if (existing != null)
            return 0;

        return await _database.InsertAsync(new NotificationReadModel
        {
            UserEmail = userEmail,
            NotificationKey = notificationKey,
            ReadAt = DateTime.Now
        });
    }
}