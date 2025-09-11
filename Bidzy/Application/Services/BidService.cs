using Bidzy.API.DTOs.bidDtos;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Services
{
    public class BidService(IBidRepository bidRepository, IAuctionRepository auctionRepository) : IBidService
    {
        private readonly IBidRepository _bidRepository = bidRepository;
        private readonly IAuctionRepository _auctionRepository = auctionRepository;
        

        public async Task<List<Bid>> GetAllBidsByUser(Guid userId)
        {
            var bids = await _bidRepository.GetBidsByUserIdAsync(userId);
            return bids;
        }

        public async Task<Bid?> PlaceBid(Bid bid)
        {
            Auction auction = await _auctionRepository.GetAuctionByIdLowAsync(bid.AuctionId);
            DateTime now = DateTime.UtcNow;
            if (auction == null) return null;
            bool isAmountValid = auction.MinimumBid < bid.Amount;
            bool isStarted = auction.StartTime < now;
            bool isCompleted = auction.EndTime < now;
            if (isStarted && !isCompleted && isAmountValid)
            {
                Bid newBid = new()
                {
                    Amount = bid.Amount,
                    AuctionId = auction.Id,
                    Timestamp = now,
                    BidderId = bid.BidderId,
                };
               var savedBid = await _bidRepository.AddBidAsync(newBid);
               return savedBid;
            }
            return null;
        }
    }
}
