using EmployeeService.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EmployeeService.Infrastruture;

public class EmployeeDBContext(DbContextOptions<EmployeeDBContext> options) : DbContext(options)
{
    public DbSet<Models.Employee> Employees => Set<Models.Employee>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<EmployeeOutboxMessage> EmployeeOutboxMessages => Set<EmployeeOutboxMessage>();
    public DbSet<EmployeeProductNotification> ProductNotifications => Set<EmployeeProductNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(x => x.MessageId);
        });

        modelBuilder.Entity<EmployeeOutboxMessage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MessageType).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.HasIndex(x => new { x.ProcessedAtUtc, x.CreatedAtUtc });
        });

        modelBuilder.Entity<EmployeeProductNotification>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceMessageId).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
