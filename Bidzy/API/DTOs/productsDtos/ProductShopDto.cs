namespace Bidzy.API.DTOs.productsDtos
{
    public class ProductShopDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Tags { get; set; }
    }
}
