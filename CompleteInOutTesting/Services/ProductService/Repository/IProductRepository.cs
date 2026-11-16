using ProductService.Model;

namespace ProductService.Repository;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken);
    Task<Product> GetProductByIdAsync(int id, CancellationToken cancellationToken);
    Task<int> AddProduct(Product product, CancellationToken cancellationToken);
    Task<bool> UpdateProduct(Product product, CancellationToken cancellationToken);
    Task<bool> DeleteProduct(int id, CancellationToken cancellationToken);
}
