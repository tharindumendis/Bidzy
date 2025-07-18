using System.ComponentModel.DataAnnotations;
using Bidzy.Modles.Enties;

namespace Bidzy.Modles.Dto
{
    public class ProductAddDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public Guid SellerId { get; set; }
    }
}
