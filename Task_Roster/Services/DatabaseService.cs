using SQLite;
using Task_Roster.Models;
using Task_Roster.Models;

namespace Task_Roster.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService()
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "schedule.db");

        _database = new SQLiteAsyncConnection(dbPath);

        _database.CreateTableAsync<ShiftModel>().Wait();

        _database.CreateTableAsync<EmployeeModel>().Wait();
    }

    // CREATE
    public async Task AddShiftAsync(ShiftModel shift)
    {
        await _database.InsertAsync(shift);
    }

    // READ
    public async Task<List<ShiftModel>> GetShiftsAsync()
    {
        return await _database.Table<ShiftModel>().ToListAsync();
    }

    // UPDATE
    public async Task UpdateShiftAsync(ShiftModel shift)
    {
        await _database.UpdateAsync(shift);
    }

    // DELETE
    public async Task DeleteShiftAsync(ShiftModel shift)
    {
        await _database.DeleteAsync(shift);
    }


    // EMPLOYEE CRUD OPERATIONS
    // CREATE EMPLOYEE
    public async Task AddEmployeeAsync(EmployeeModel employee)
    {
        await _database.InsertAsync(employee);
    }

    // READ EMPLOYEES
    public async Task<List<EmployeeModel>> GetEmployeesAsync()
    {
        return await _database.Table<EmployeeModel>().ToListAsync();
    }

    // UPDATE EMPLOYEE
    public async Task UpdateEmployeeAsync(EmployeeModel employee)
    {
        await _database.UpdateAsync(employee);
    }

    // DELETE EMPLOYEE
    public async Task DeleteEmployeeAsync(EmployeeModel employee)
    {
        await _database.DeleteAsync(employee);
    }
}