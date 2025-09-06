using System.ComponentModel.DataAnnotations;

namespace Bidzy.Domain.Enties
{
    public class Otp
    {
        public Guid Id { get; set; }
        public required string OtpCode { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public required DateTime ExpireAt { get; set; }
    }
}
