namespace SearchEngine.Application.Features.Products.DTOs;

public class ReadProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
}
