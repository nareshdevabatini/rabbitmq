namespace EmployeeService.Respository
{
    public interface IEmployeeRepository
    {
        Task<List<Models.Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
        Task<Models.Employee> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddEmployeeAsync(Models.Employee employee, CancellationToken cancellationToken = default);
        Task AddEmployeesAsync(List<Models.Employee> employees, CancellationToken cancellationToken = default);
        Task UpdateEmployeeAsync(Models.Employee employee, CancellationToken cancellationToken = default);
        Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
    }
}
