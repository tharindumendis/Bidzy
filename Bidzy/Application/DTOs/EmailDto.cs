using System.ComponentModel.DataAnnotations;

namespace Bidzy.Application.DTOs
{
    public class EmailDto
    {
        [EmailAddress]
        public required string ReceiverEmail { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}
