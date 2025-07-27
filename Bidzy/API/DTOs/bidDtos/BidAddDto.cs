using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.bidDtos
{
    public class BidAddDto
    {
        [Required]
        public Guid AuctionId { get; set; }
        [Required]
        public Guid BidderId { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
