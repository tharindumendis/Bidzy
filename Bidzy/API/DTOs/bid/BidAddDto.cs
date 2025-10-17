using System.ComponentModel.DataAnnotations;

namespace Bidzy.API.DTOs.bid
{
    public class BidAddDto
    {
        [Required]
        public required Guid AuctionId { get; set; }
        public Guid BidderId { get; set; }
        [Required]
        public required decimal Amount { get; set; }
    }
}
