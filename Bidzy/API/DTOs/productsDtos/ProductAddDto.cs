using System.ComponentModel.DataAnnotations;
using Bidzy.Domain.Enties;

namespace Bidzy.API.DTOs.productsDtos
{
    public class ProductAddDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public Guid SellerId { get; set; }

        public List<string>? Tags { get; set; }
    }
}
