namespace ProductService.Model;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ExpirationDate { get; set; }
    public bool ProductAvailability { get; set; }
}
