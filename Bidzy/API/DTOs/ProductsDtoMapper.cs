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
                SellerName = product.Seller?.FullName ?? string.Empty
            };
        }

        public static Product ToEntity (this ProductAddDto productAddDto)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Title = productAddDto.Title,
                Description = productAddDto.Description,
                ImageUrl = productAddDto.ImageUrl,
                SellerId = productAddDto.SellerId
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
