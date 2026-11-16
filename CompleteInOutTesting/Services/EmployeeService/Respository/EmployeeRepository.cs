using EmployeeService.Infrastruture;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Respository
{
    public class EmployeeRepository: IEmployeeRepository
    {
        private readonly EmployeeDBContext _context;

        public EmployeeRepository(EmployeeDBContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Employees.ToListAsync(cancellationToken);
        }
        public async Task<Models.Employee> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid employee ID.", nameof(id));
            }
            return await _context.Employees.FindAsync(id);
        }
        public async Task AddEmployeeAsync(Models.Employee employee, CancellationToken cancellationToken = default)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task AddEmployeesAsync(List<Models.Employee> employees, CancellationToken cancellationToken = default)
        {
            if (employees == null)
            {
                throw new ArgumentNullException(nameof(employees));
            }
            _context.Employees.AddRange((IEnumerable<Models.Employee>)employees);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateEmployeeAsync(Models.Employee employee, CancellationToken cancellationToken = default)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
