using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;
using ProductService.Model;

namespace ProductService.Repository;

public class ProductRepository(ProductDbContext _dbContext) : IProductRepository
{
    public async Task<int> AddProduct(Product product, CancellationToken cancellationToken)
    {
         _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product.Id;
    }

    public async Task<bool> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken)
    {
        return await _dbContext.Products.ToListAsync(cancellationToken);
    }

    public async Task<Product> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.FindAsync(id, cancellationToken);
        if (product == null)
        {
            throw new ArgumentNullException(nameof(id));
        }
        return product;
    }

    public async Task<bool> UpdateProduct(Product product, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
