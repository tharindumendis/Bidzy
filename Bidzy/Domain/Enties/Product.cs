using System.ComponentModel.DataAnnotations;

namespace Bidzy.Domain.Enties
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public Guid SellerId { get; set; }
        public User Seller { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    }
}
