namespace Bidzy.API.DTOs.productsDtos
{
    public class ProductsUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? SellerId { get; set; }
    }
}
