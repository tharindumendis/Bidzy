using System.ComponentModel.DataAnnotations;
using Bidzy.Domain.Entities;
using Microsoft.Extensions.FileProviders;

namespace Bidzy.API.DTOs.products
{
    public class ProductAddDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile file { get; set; }

        public List<string>? Tags { get; set; }
    }
}
