using System.ComponentModel.DataAnnotations;
using Bidzy.Application.DTOs;
using Bidzy.Domain.Enties;
using Microsoft.AspNetCore.Mvc;

namespace Bidzy.Application.Services.Email
{
    public interface IEmailJobService
    {
        Task SendEmailAsync([FromBody] EmailDto dto);
        Task SendAuctionStartedEmailsAsync(Auction auction, List<string> emailAddresses);
        Task SendAuctionEndedEmails(Auction auction, Bid winBid);
        Task SendAuctionCancelledEmail(Auction auction);
        Task SendAuctionStartedEmailForSeller(string auctionId, [EmailAddress] string receiverEmail);
        Task SendOTP(string OTP, [EmailAddress] string receiverEmail);
    }
}
