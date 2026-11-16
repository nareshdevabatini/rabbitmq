using Microsoft.EntityFrameworkCore;
using ProductService.Model;
using System.Reflection;

namespace ProductService.Infrastructure
{
    public class ProductDbContext(DbContextOptions<ProductDbContext> options): DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                        Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
