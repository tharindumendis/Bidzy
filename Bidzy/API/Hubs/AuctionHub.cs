using System.Security.Claims;
using Bidzy.API.DTOs.favoriteAuctionsDtos;
using Bidzy.Application.DTOs;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Bidzy.API.Hubs
{
    [Authorize]
    public class AuctionHub(IUserAuctionFavoriteRepository favoriteRepository) : Hub
    {
        private readonly IUserAuctionFavoriteRepository _favoriteRepository = favoriteRepository;

        public async Task JoinAuctionGroup(HubSubscribeData payload)
        {
            

            try {
                if(payload.GroupIds == null) {  return; }

                foreach (string gId in payload.GroupIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, gId);
                    if (payload.UserId != null){
                        await AddFavorite(gId, payload.UserId);
                    }
                }
            }
            catch
            {
                return;
            }
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User connected: {userId}");
            await base.OnConnectedAsync();
        }
        public async Task LeaveAuctionGroup(HubSubscribeData payload)
        {
            Console.WriteLine("leaveAuction"+payload.UserId);
            if (payload.GroupIds == null) { return; }
  
            foreach (string gId in payload.GroupIds)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId,gId);
                if (payload.UserId != null)
                {
                    await RemoveFavorite(gId, payload.UserId);
                }
            }
        }
        public async Task SendBidUpdate(string auctionId, string message)
        {
            await Clients.Group(auctionId).SendAsync("ReceiveBidUpdate", message);
        }
        public async Task SendActionUpdate(string auctionId, string message)
        {
            await Clients.Group(auctionId).SendAsync("ReceiveAuctionUpdate", message);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Conn {Context.ConnectionId} disconnected. Reason: {exception?.Message}");

            await base.OnDisconnectedAsync(exception);
        }

        private async Task AddFavorite (string auctionId, string userId)
        {
            if (Guid.TryParse(auctionId, out Guid aId))
            {
                if (Guid.TryParse(userId, out Guid uId))
                {
                    try
                    {
                        var favEntity = new UserAuctionFavorite
                        {
                            auctionId = aId,
                            userId = uId
                        };
                        await _favoriteRepository.AddAsync(favEntity);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            return;
        }
        private async Task RemoveFavorite(string auctionId, string userId)
        {
            if (Guid.TryParse(auctionId, out Guid aId))
            {
                if (Guid.TryParse(userId, out Guid uId))
                {
                    try
                    {
                        await _favoriteRepository.RemoveAsync(uId, aId);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            return;
        }
    }
}
