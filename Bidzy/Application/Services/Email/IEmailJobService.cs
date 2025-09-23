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
        Task SendPaymentReceiptEmail(Payment payment, User buyer, Auction auction);
        Task SendPaymentFailedEmail(Payment payment, User buyer, Auction auction, string reason);
        Task SendRefundReceiptEmail(Payment payment, User buyer, Auction auction);
        Task SendRefundNotificationEmail(Payment payment, User seller, Auction auction);
        Task SendPaymentReceiptSellerEmail(Payment payment, User seller, User buyer, Auction auction);
    }
}
