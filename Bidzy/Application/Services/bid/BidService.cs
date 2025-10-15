using Bidzy.API.DTOs.bidDtos;
using Bidzy.API.DTOs.Common;
using Bidzy.Application.Mappers;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Auction;
using Bidzy.Application.Repository.Bid;
using Bidzy.Application.Services.SignalR;
using Bidzy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Services.Bid
{
    public class BidService(IBidRepository bidRepository, IAuctionRepository auctionRepository, ISignalRNotifier signalRNotifier) : IBidService
    {
        private readonly IBidRepository _bidRepository = bidRepository;
        private readonly IAuctionRepository _auctionRepository = auctionRepository;
        private readonly ISignalRNotifier _signalRNotifier = signalRNotifier;

        public async Task<List<Domain.Entities.Bid>> GetAllBidsByUser(Guid userId)
        {
            var bids = await _bidRepository.GetBidsByUserIdAsync(userId);
            return bids;
        }

        public async Task<Domain.Entities.Bid?> PlaceBid(Domain.Entities.Bid bid)
        {
            Auction auction = await _auctionRepository.GetAuctionByIdLowAsync(bid.AuctionId);
            DateTime now = DateTime.UtcNow;
            if (auction == null) return null;
            bool isAmountValid = auction.MinimumBid < bid.Amount;
            bool isStarted = auction.StartTime < now;
            bool isCompleted = auction.EndTime < now;
            if (isStarted && !isCompleted && isAmountValid)
            {
                Domain.Entities.Bid newBid = new()
                {
                    Amount = bid.Amount,
                    AuctionId = auction.Id,
                    Timestamp = now,
                    BidderId = bid.BidderId,
                };
               var savedBid = await _bidRepository.AddBidAsync(newBid);
               await _signalRNotifier.BroadcastNewBid(savedBid);
               return savedBid;
            }
            return null;
        }
        public async Task<PagedResult<Domain.Entities.Bid>> GetPagedBidsByUserAsync(Guid userId, int page, int pageSize)
        {
            PagedResult<Domain.Entities.Bid> result = await _bidRepository.GetPagedBidsByUserAsync(userId, page, pageSize);

            return result;
        }

        public async Task<BidderActivityDto> GetBidderActivityAsync(Guid userId)
        {
            var userBids = await _bidRepository.GetBidsByUserIdAsync(userId);
            var participatedAuctionIds = userBids.Select(b => b.AuctionId).Distinct();
            var participatedAuctions = await _auctionRepository.GetFullAuctionsByIdsAsync(participatedAuctionIds);
            var wonAuctions = await _auctionRepository.GetFullWonAuctionsByUserIdAsync(userId);

            var activityDto = new BidderActivityDto
            {
                ParticipatedAuctions = participatedAuctions.Select(auction => auction.ToBidderAuctionDto()),
                WonAuctions = wonAuctions.Select(auction => auction.ToBidderAuctionDto())
            };

            return activityDto;
        }

    }
}
