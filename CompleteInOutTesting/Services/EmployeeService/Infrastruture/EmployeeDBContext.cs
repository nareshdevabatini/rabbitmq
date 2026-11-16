using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EmployeeService.Infrastruture;

public class EmployeeDBContext(DbContextOptions<EmployeeDBContext> options) : DbContext(options)
{
    public DbSet<Models.Employee> Employees { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
                    Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
