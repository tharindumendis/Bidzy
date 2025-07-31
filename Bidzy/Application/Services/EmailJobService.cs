using System.ComponentModel.DataAnnotations;
using Bidzy.Application.DTOs;
using Bidzy.Domain.Enties;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Org.BouncyCastle.Tls;

namespace Bidzy.Application.Services
{
    public class EmailJobService : IEmailJobService
    {
        private readonly IConfiguration _config;

        public EmailJobService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(EmailDto dto)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["EmailSettings:Sender"]));
                email.To.Add(MailboxAddress.Parse(dto.ReceiverEmail));
                email.Subject = dto.Subject;

                email.Body = new TextPart("plain")
                {
                    Text = dto.Body
                };

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["EmailSettings:Sender"], _config["EmailSettings:AppPassword"]); // Use App Password
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                Console.WriteLine($"✅ Email sent to {dto.ReceiverEmail} with subject: {dto.Subject}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending email: {ex.Message}");

            }
        }

        public Task SendAuctionStartedEmailForSeller(string auctionId, [EmailAddress] string receiverEmail)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = receiverEmail,
                Subject = $"Auction #{auctionId} has started!",
                Body = $@"Hello,

                        Great news! Your auction #{auctionId} has just gone live on BIDZY.

                        📌 Auction Details:
                        - Auction ID: {auctionId}
                        - Start Time: {DateTime.UtcNow:dddd, MMMM d, yyyy h:mm tt} UTC
                        - Status: Live and accepting bids

                        You can monitor bids, update your listing, or respond to buyer inquiries directly from your seller dashboard.

                        🔗 [View Your Auction](https://bidzy.com/seller/auction/{auctionId})

                        Thank you for choosing BIDZY. We wish you a successful auction!

                        Warm regards,  
                        The BIDZY Team"
            };
            return SendEmailAsync(dto);
        }

        public Task SendAuctionStartedEmailsAsync(Auction auction, List<string> receiverEmails)
        {
            
            if (receiverEmails != null)
            {
                var emailTasks = receiverEmails
                    .Where(email => !string.IsNullOrWhiteSpace(email)) // Optional: filter invalid entries
                    .Select(email =>
                    {
                        var dto = new EmailDto
                        {
                            ReceiverEmail = email,
                            Subject = $"Auction #{auction.Id} has started!",
                            Body = $@"Hello,

                                Auction #{auction.Id} is now live. Place your bids before it ends!

                                👉 [View Auction](https://bidzy.com/auction/{auction.Id})

                                Happy bidding,
                                The BIDZY Team"
                        };
                        return SendEmailAsync(dto);
                    });
                return Task.WhenAll(emailTasks);
            }

            return SendAuctionStartedEmailForSeller(auction.Id.ToString(), auction.Product.Seller.Email); // Run all email sends concurrently
        }

        public Task SendAuctionEndedEmails(Auction auction,Bid winBid)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = winBid.Bidder.Email,
                Subject = $"Auction #{auction.Id} has ended",
                // TODO URL Link 
                Body = $@"
                        Hello {winBid.Bidder.FullName},

                        🎉 Congratulations! You’ve won Auction #{auction.Id} on BIDZY.

                        **Item**: {auction.Product.Title}  
                        **Final Bid**: {winBid.Amount:c}  
                        **Seller**: {auction.Product.Seller.FullName}

                        We’re thrilled to have you as the winning bidder. Please proceed to finalize your purchase and coordinate with the seller for delivery or pickup.

                        You can view the auction details and next steps here: [View Auction](https://bidzy.com/auction/{auction.Id})

                        If you have any questions or need help, our support team is here for you.

                        Happy bidding,  
                        The BIDZY Team
                        "

            };
            SendEmailAsync(dto).Wait();
            dto.ReceiverEmail = auction.Product.Seller.Email;
            dto.Body = $@"
                        Hello {auction.Product.Seller.FullName},
                        We’re writing to inform you that your auction (Auction #{auction.Id}, - {auction.Product.Title}) has successfully concluded.

                        🎉 **Winning Bidder**: {winBid.Bidder.FullName}

                        Thank you for listing your item on BIDZY and being part of our auction community. We appreciate your participation and hope the process was smooth and rewarding.

                        If you have any questions or need assistance with next steps, feel free to reach out to our support team.

                        Best regards,  
                        The BIDZY Team
                        ";
            return SendEmailAsync(dto);
        }

        public Task SendAuctionCancelledEmail(Auction auction)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = auction.Product.Seller.Email,
                Subject = $"Auction #{auction.Id} has started!",
                // TODO create url
                Body = $@"
                        Hello {auction.Product.Seller.FullName},

                        We regret to inform you that Auction #{auction.Id} has been closed without a winning bidder.

                        **Item**: {auction.Product.Title}  
                        **Reason**: No qualifying bids were placed during the auction period.

                        We understand this may be disappointing. You’re welcome to relist the item or adjust the auction settings to improve visibility and engagement.

                        👉 [Relist Your Item](https://bidzy.com/auction/relist/{auction.Id})

                        If you need help optimizing your listing or have any questions, our support team is here to assist you.

                        Thank you for using BIDZY,  
                        The BIDZY Team
                        "

            };
            return SendEmailAsync(dto);
        }
    }
}
