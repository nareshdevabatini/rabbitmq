using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace EmployeeService.Respository;

public class CachedEmployeeRepository(IEmployeeRepository repository, IDistributedCache cache) : IEmployeeRepository
{
    public Task AddEmployeeAsync(Models.Employee employee, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddEmployeesAsync(List<Models.Employee> employees, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Models.Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Models.Employee> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"employee:{id}";
        var cachedEmployee = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedEmployee))
        {
            return JsonSerializer.Deserialize<Models.Employee>(cachedEmployee);
        }

        // 2. Fetch from database if cache miss
        var employee = await repository.GetEmployeeByIdAsync(id,cancellationToken);

        if (employee is null)
        {
            return null;
        }

        // 3. Store employee in Redis
        await cache.SetStringAsync(cacheKey,JsonSerializer.Serialize(employee),cancellationToken);

        return employee;
    }

    public Task UpdateEmployeeAsync(Models.Employee employee, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
