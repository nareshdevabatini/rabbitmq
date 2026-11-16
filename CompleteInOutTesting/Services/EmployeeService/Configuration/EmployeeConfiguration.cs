using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Configuration;

public class EmployeeConfiguration:IEntityTypeConfiguration<Models.Employee>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Models.Employee> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(50);
        builder.HasData(
            new () { Id = 1, FirstName = "John", LastName = "Doe" },
            new () { Id = 2, FirstName = "Jane", LastName = "Smith" },
            new() { Id = 3, FirstName = "Naresh", LastName = "Dev" }
        );
    }
}
