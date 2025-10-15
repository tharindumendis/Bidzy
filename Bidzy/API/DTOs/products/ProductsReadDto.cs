using Bidzy.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.productsDtos
{
    public class ProductsReadDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public Guid SellerId { get; set; }
        public string SellerName { get; set; }
        public List<string> Tags { get; set; }
    }
}
