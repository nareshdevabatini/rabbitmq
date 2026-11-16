using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductService.Model;

namespace ProductService.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasData(
            new Product
            {
                Id = 1,
                Name = "Product 1",
                Description = "Product 1 description",
                ExpirationDate = "2024-12-31",
                ProductAvailability = true,
                Price = 10.0m
            },
            new Product
            {
                Id = 2,
                Name = "Product 2",
                Description = "Product 2 description",
                ExpirationDate = "2024-12-31",
                ProductAvailability = true,
                Price = 20.0m
            },
            new Product
            {
                Id = 3,
                Name = "Product 3",
                Description = "Product 3 description",
                ExpirationDate = "2024-12-31",
                ProductAvailability = false,
                Price = 30.0m
            }
        );
    }
}
