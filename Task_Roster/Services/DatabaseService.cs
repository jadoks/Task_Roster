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
        catch
        {
            // Column already exists.
        }
    }

    public async Task<int> AddUserAsync(UserModel user)
    {
        return await _database.InsertAsync(user);
    }

    public async Task<UserModel?> GetUserByEmailAsync(string email)
    {
        return await _database.Table<UserModel>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<List<UserModel>> GetUsersAsync()
    {
        return await _database.Table<UserModel>()
            .ToListAsync();
    }

    public async Task<int> UpdateUserAsync(UserModel user)
    {
        return await _database.UpdateAsync(user);
    }

    public async Task<UserModel?> LoginUserAsync(
        string email,
        string password)
    {
        return await _database.Table<UserModel>()
            .Where(u =>
                u.Email == email &&
                u.Password == password)
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdatePasswordAsync(
        string email,
        string newPassword)
    {
        var user = await GetUserByEmailAsync(email);

        if (user == null)
            return 0;

        user.Password = newPassword;

        return await _database.UpdateAsync(user);
    }

    public async Task AddShiftAsync(ShiftModel shift)
    {
        await _database.InsertAsync(shift);
    }

    public async Task<List<ShiftModel>> GetShiftsAsync()
    {
        return await _database.Table<ShiftModel>()
            .ToListAsync();
    }

    public async Task UpdateShiftAsync(ShiftModel shift)
    {
        await _database.UpdateAsync(shift);
    }

    public async Task DeleteShiftAsync(ShiftModel shift)
    {
        await _database.DeleteAsync(shift);
    }

    public async Task AddEmployeeAsync(EmployeeModel employee)
    {
        await _database.InsertAsync(employee);
    }

    public async Task<List<EmployeeModel>> GetEmployeesAsync()
    {
        return await _database.Table<EmployeeModel>()
            .ToListAsync();
    }

    public async Task UpdateEmployeeAsync(EmployeeModel employee)
    {
        await _database.UpdateAsync(employee);
    }

    public async Task DeleteEmployeeAsync(EmployeeModel employee)
    {
        await _database.DeleteAsync(employee);
    }

    public async Task<int> AddTaskAsync(TaskModel task)
    {
        return await _database.InsertAsync(task);
    }

    public async Task<List<TaskModel>> GetTasksByShiftIdAsync(int shiftId)
    {
        return await _database.Table<TaskModel>()
            .Where(t => t.ShiftId == shiftId)
            .ToListAsync();
    }

    public async Task<int> UpdateTaskAsync(TaskModel task)
    {
        return await _database.UpdateAsync(task);
    }

    public async Task<int> DeleteTaskAsync(TaskModel task)
    {
        return await _database.DeleteAsync(task);
    }
}