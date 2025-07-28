using Bidzy.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.Application.Services
{
    public interface IEmailJobService
    {
        Task SendEmailAsync([FromBody] EmailDto dto);
        Task SendAuctionStartedEmail(string auctionId, string receiverEmail);
        Task SendAuctionEndedEmail(string auctionId, string receiverEmail, string winnerName);

    }
}
