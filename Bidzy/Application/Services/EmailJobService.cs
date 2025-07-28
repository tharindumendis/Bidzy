using Bidzy.Application.DTOs;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace Bidzy.Application.Services
{
    public class EmailJobService : IEmailJobService
    {
        private readonly IConfiguration _config;

        public EmailJobService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync([FromBody] EmailDto dto)
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
        public Task SendAuctionStartedEmail(string auctionId, string receiverEmail)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = receiverEmail,
                Subject = $"Auction #{auctionId} has started!",
                Body = $"Hello,\n\nAuction #{auctionId} is now live. Place your bids before it ends!"
            };
            return SendEmailAsync(dto);
        }

        public Task SendAuctionEndedEmail(string auctionId, string receiverEmail, string winnerName)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = receiverEmail,
                Subject = $"Auction #{auctionId} has ended",
                Body = $"Hello,\n\nAuction #{auctionId} has ended. Winner: {winnerName}. Thank you for participating!"
            };
            return SendEmailAsync(dto);
        }

    }
}
