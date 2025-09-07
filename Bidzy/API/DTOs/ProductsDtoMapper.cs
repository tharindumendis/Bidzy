using Bidzy.API.DTOs.productsDtos;
using Bidzy.Domain.Enties;

namespace Bidzy.API.DTOs
{
    public static class ProductsDtoMapper
    {
        public static ProductsReadDto ToReadDto(this Product product )
        {
            return new ProductsReadDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                SellerId = product.SellerId,
                SellerName = product.Seller?.FullName ?? string.Empty,
                Tags = product.Tags.Select(t => t.tagName).ToList()
            };
        }

        public static Product ToEntity (this ProductAddDto productAddDto, List<Tag> tags,Guid sellerId)
        {
            Guid productId = Guid.NewGuid();
            return new Product
            {
                Id = productId,
                Title = productAddDto.Title,
                Description = productAddDto.Description,
                ImageUrl = "/Image/product/"+ productId.ToString(),
                SellerId = sellerId,
                Tags = tags
            };
        }

        public static void UpdateEntity(this Product product, ProductsUpdateDto productsUpdateDto)
        {
            if (!string.IsNullOrWhiteSpace(productsUpdateDto.Title))
            {
                product.Title = productsUpdateDto.Title;
            }

            if (!string.IsNullOrWhiteSpace(productsUpdateDto.Description))
            {
                product.Description = productsUpdateDto.Description;
            }
            if (!string.IsNullOrWhiteSpace(productsUpdateDto.ImageUrl))
            {
                product.ImageUrl = productsUpdateDto.ImageUrl;
            }
            if (productsUpdateDto.SellerId.HasValue)
            {
                product.SellerId = productsUpdateDto.SellerId.Value;
            }
        }

    }
}
