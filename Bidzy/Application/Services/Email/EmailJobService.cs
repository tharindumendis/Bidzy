using System.ComponentModel.DataAnnotations;
using Bidzy.Application.DTOs;
using Bidzy.Domain.Enties;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using Org.BouncyCastle.Tls;

namespace Bidzy.Application.Services.Email
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

                email.Body = new TextPart(TextFormat.Html)
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
                Body = $@"
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2 style=""color: #007BFF;"">Hello,</h2>
                        <p>Great news! Your auction <strong>#{auctionId}</strong> has just gone live on <strong>BIDZY</strong>.</p>

                        <h3 style=""margin-top: 20px;"">📌 Auction Details:</h3>
                        <ul>
                          <li><strong>Auction ID:</strong> {auctionId}</li>
                          <li><strong>Start Time:</strong> {DateTime.UtcNow:dddd, MMMM d, yyyy h:mm tt} UTC</li>
                          <li><strong>Status:</strong> Live and accepting bids</li>
                        </ul>

                        <p>You can monitor bids, update your listing, or respond to buyer inquiries directly from your seller dashboard.</p>

                        <p>
                          🔗 <a href=""https://bidzy.com/seller/auction/{auctionId}"" style=""color: #007BFF;"">View Your Auction</a>
                        </p>

                        <p>Thank you for choosing <strong>BIDZY</strong>. We wish you a successful auction!</p>

                        <br/>
                        <p>Warm regards,<br/>The BIDZY Team</p>
                      </body>
                    </html>"
            };
            return SendEmailAsync(dto);
        }
        public Task SendOTP(string OTP, [EmailAddress] string receiverEmail)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = receiverEmail,
                Subject = "Your Bidzy OTP Code",
                Body = $@"
                        <html>
                            <body style=""font-family: Arial, sans-serif; color: #333;"">
                                <h2>Welcome to Bidzy!</h2>
                                <p>To complete your registration, please use the following One-Time Password (OTP):</p>
                                <div style=""font-size: 24px; font-weight: bold; margin: 20px 0; color: #007BFF;"">{OTP}</div>
                                <p>This code is valid for the next 10 minutes. Please do not share it with anyone.</p>
                                <br/>
                                <p>If you didn’t request this, you can safely ignore this email.</p>
                                <p>Thanks,<br/>The Bidzy Team</p>
                            </body>
                        </html>"
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
                            Body = $@"
                                <html>
                                  <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                                    <h2 style=""color: #007BFF;"">Hello,</h2>
                                    <p>Auction <strong>#{auction.Id}</strong> is now live. Place your bids before it ends!</p>

                                    <p>
                                      👉 <a href=""https://bidzy.com/auction/{auction.Id}"" style=""color: #007BFF; font-weight: bold;"">View Auction</a>
                                    </p>

                                    <p>Happy bidding,<br/>The BIDZY Team</p>
                                  </body>
                                </html>"
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
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2 style=""color: #28a745;"">🎉 Congratulations, {winBid.Bidder.FullName}!</h2>
                        <p>You’ve won <strong>Auction #{auction.Id}</strong> on <strong>BIDZY</strong>.</p>

                        <h3 style=""margin-top: 20px;"">🛍️ Auction Summary:</h3>
                        <ul>
                          <li><strong>Item:</strong> {auction.Product.Title}</li>
                          <li><strong>Final Bid:</strong> {winBid.Amount:c}</li>
                          <li><strong>Seller:</strong> {auction.Product.Seller.FullName}</li>
                        </ul>

                        <p>We’re thrilled to have you as the winning bidder. Please proceed to finalize your purchase and coordinate with the seller for delivery or pickup.</p>

                        <p>
                          🔗 <a href=""https://bidzy.com/auction/{auction.Id}"" style=""color: #007BFF; font-weight: bold;"">View Auction Details</a>
                        </p>

                        <p>If you have any questions or need help, our support team is here for you.</p>

                        <br/>
                        <p>Happy bidding,<br/>The BIDZY Team</p>
                      </body>
                    </html>"


            };
            SendEmailAsync(dto).Wait();
            dto.ReceiverEmail = auction.Product.Seller.Email;
            dto.Body = $@"
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2>Hello {auction.Product.Seller.FullName},</h2>
                        <p>We’re writing to inform you that your auction 
                          <strong>#{auction.Id}</strong> – <em>{auction.Product.Title}</em> 
                          has successfully concluded.</p>

                        <h3 style=""margin-top: 20px;"">🎉 Winning Bidder:</h3>
                        <p><strong>{winBid.Bidder.FullName}</strong></p>

                        <p>Thank you for listing your item on <strong>BIDZY</strong> and being part of our auction community. 
                        We appreciate your participation and hope the process was smooth and rewarding.</p>

                        <p>If you have any questions or need assistance with next steps, feel free to reach out to our support team.</p>

                        <br/>
                        <p>Best regards,<br/>The BIDZY Team</p>
                      </body>
                    </html>";

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
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2>Hello {auction.Product.Seller.FullName},</h2>
                        <p>We regret to inform you that <strong>Auction #{auction.Id}</strong> has been closed without a winning bidder.</p>

                        <h3 style=""margin-top: 20px;"">📦 Auction Summary:</h3>
                        <ul>
                          <li><strong>Item:</strong> {auction.Product.Title}</li>
                          <li><strong>Reason:</strong> No qualifying bids were placed during the auction period.</li>
                        </ul>

                        <p>We understand this may be disappointing. You’re welcome to relist the item or adjust the auction settings to improve visibility and engagement.</p>

                        <p>
                          👉 <a href=""https://bidzy.com/auction/relist/{auction.Id}"" style=""color: #007BFF; font-weight: bold;"">Relist Your Item</a>
                        </p>

                        <p>If you need help optimizing your listing or have any questions, our support team is here to assist you.</p>

                        <br/>
                        <p>Thank you for using <strong>BIDZY</strong>,<br/>The BIDZY Team</p>
                      </body>
                    </html>"

            };
            return SendEmailAsync(dto);
        }

        public Task SendPaymentReceiptEmail(Payment payment, User buyer, Auction auction)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = buyer.Email,
                Subject = $"Payment Receipt for Auction #{auction.Id}",
                Body = $@"
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2 style=""color: #28a745;"">Payment Successful!</h2>
                        <p>Dear {buyer.FullName},</p>
                        <p>Your payment for <strong>Auction #{auction.Id} - {auction.Product.Title}</strong> has been successfully processed.</p>

                        <h3 style=""margin-top: 20px;"">Payment Details:</h3>
                        <ul>
                          <li><strong>Payment ID:</strong> {payment.Id}</li>
                          <li><strong>Auction Item:</strong> {auction.Product.Title}</li>
                          <li><strong>Amount Paid:</strong> {payment.TotalAmount:C} {(payment.Currency?.ToUpper() ?? "")}</li>
                          <li><strong>Date:</strong> {payment.PaidAt?.ToUniversalTime() ?? DateTime.UtcNow:dddd, MMMM d, yyyy h:mm tt} UTC</li>
                        </ul>

                        <p>You can view your payment details and auction status on your Bidzy account.</p>
                        <p>
                          🔗 <a href=""https://bidzy.com/my-payments/{payment.Id}"" style=""color: #007BFF; font-weight: bold;"">View Payment Details</a>
                        </p>

                        <p>Thank you for using BIDZY!</p>
                        <br/>
                        <p>Best regards,<br/>The BIDZY Team</p>
                      </body>
                    </html>"
            };
            return SendEmailAsync(dto);
        }

        public Task SendPaymentReceiptSellerEmail(Payment payment, User seller, User buyer, Auction auction)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = seller.Email,
                Subject = $"Payment Received for Auction #{auction.Id}",
                Body = $@"
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2 style=""color: #28a745;"">Payment Received!</h2>
                        <p>Dear {seller.FullName},</p>
                        <p>Great news! Payment for <strong>Auction #{auction.Id} - {auction.Product.Title}</strong> has been successfully received from {buyer.FullName}.</p>

                        <h3 style=""margin-top: 20px;"">Payment Details:</h3>
                        <ul>
                          <li><strong>Payment ID:</strong> {payment.Id}</li>
                          <li><strong>Auction Item:</strong> {auction.Product.Title}</li>
                          <li><strong>Amount Received:</strong> {payment.TotalAmount:C} {(payment.Currency?.ToUpper() ?? "")}</li>
                          <li><strong>Buyer:</strong> {buyer.FullName} ({buyer.Email})</li>
                          <li><strong>Date:</strong> {payment.PaidAt?.ToUniversalTime() ?? DateTime.UtcNow:dddd, MMMM d, yyyy h:mm tt} UTC</li>
                        </ul>

                        <p>You can view the payment details and proceed with delivery arrangements in your seller dashboard.</p>
                        <p>
                          🔗 <a href=""https://bidzy.com/seller/payments/{payment.Id}"" style=""color: #007BFF; font-weight: bold;"">View Payment Details</a>
                        </p>

                        <p>Thank you for using BIDZY!</p>
                        <br/>
                        <p>Best regards,<br/>The BIDZY Team</p>
                      </body>
                    </html>"
            };
            return SendEmailAsync(dto);
        }

        public Task SendPaymentFailedEmail(Payment payment, User buyer, Auction auction, string reason)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = buyer.Email,
                Subject = $"Payment Failed for Auction #{auction.Id}",
                Body = $@"
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2 style=""color: #dc3545;"">Payment Failed!</h2>
                        <p>Dear {buyer.FullName},</p>
                        <p>We regret to inform you that your payment for <strong>Auction #{auction.Id} - {auction.Product.Title}</strong> has failed.</p>

                        <h3 style=""margin-top: 20px;"">Payment Details:</h3>
                        <ul>
                          <li><strong>Payment ID:</strong> {payment.Id}</li>
                          <li><strong>Auction Item:</strong> {auction.Product.Title}</li>
                          <li><strong>Attempted Amount:</strong> {payment.TotalAmount:C} {(payment.Currency?.ToUpper() ?? "")}</li>
                          <li><strong>Reason:</strong> {reason}</li>
                          <li><strong>Date:</strong> {DateTime.UtcNow:dddd, MMMM d, yyyy h:mm tt} UTC</li>
                        </ul>

                        <p>Please try again or update your payment method. If you continue to experience issues, please contact our support team.</p>
                        <p>
                          🔗 <a href=""https://bidzy.com/my-payments/{payment.Id}"" style=""color: #007BFF; font-weight: bold;"">Retry Payment</a>
                        </p>

                        <p>We apologize for any inconvenience.</p>
                        <br/>
                        <p>Best regards,<br/>The BIDZY Team</p>
                      </body>
                    </html>"
            };
            return SendEmailAsync(dto);
        }

        public Task SendRefundReceiptEmail(Payment payment, User buyer, Auction auction)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = buyer.Email,
                Subject = $"Refund Processed for Auction #{auction.Id}",
                Body = $@"
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2 style=""color: #28a745;"">Refund Successful</h2>
                        <p>Dear {buyer.FullName},</p>
                        <p>Your refund for <strong>Auction #{auction.Id} - {auction.Product.Title}</strong> has been processed.</p>

                        <h3 style=""margin-top: 20px;"">Refund Details:</h3>
                        <ul>
                          <li><strong>Payment ID:</strong> {payment.Id}</li>
                          <li><strong>Refund ID:</strong> {payment.RefundId}</li>
                          <li><strong>Refund Amount:</strong> {payment.RefundAmount:C} {(payment.Currency?.ToUpper() ?? "")}</li>
                          <li><strong>Date:</strong> {DateTime.UtcNow:dddd, MMMM d, yyyy h:mm tt} UTC</li>
                        </ul>

                        <p>You can view details in your Bidzy account.</p>
                        <p>
                          🔗 <a href=""https://bidzy.com/my-payments/{payment.Id}"" style=""color: #007BFF; font-weight: bold;"">View Payment</a>
                        </p>

                        <br/>
                        <p>Best regards,<br/>The BIDZY Team</p>
                      </body>
                    </html>"
            };
            return SendEmailAsync(dto);
        }

        public Task SendRefundNotificationEmail(Payment payment, User seller, Auction auction)
        {
            var dto = new EmailDto
            {
                ReceiverEmail = seller.Email,
                Subject = $"Refund Issued for Auction #{auction.Id}",
                Body = $@"
                    <html>
                      <body style=""font-family: Arial, sans-serif; color: #333; line-height: 1.6;"">
                        <h2>Refund Notification</h2>
                        <p>Hello {seller.FullName},</p>
                        <p>A refund has been issued for <strong>Auction #{auction.Id} - {auction.Product.Title}</strong>.</p>

                        <h3 style=""margin-top: 20px;"">Details:</h3>
                        <ul>
                          <li><strong>Payment ID:</strong> {payment.Id}</li>
                          <li><strong>Refund ID:</strong> {payment.RefundId}</li>
                          <li><strong>Refund Amount:</strong> {payment.RefundAmount:C} {(payment.Currency?.ToUpper() ?? "")}</li>
                          <li><strong>Date:</strong> {DateTime.UtcNow:dddd, MMMM d, yyyy h:mm tt} UTC</li>
                        </ul>

                        <p>You can review this transaction in your seller dashboard.</p>

                        <br/>
                        <p>Regards,<br/>The BIDZY Team</p>
                      </body>
                    </html>"
            };
            return SendEmailAsync(dto);
        }
    }
}
