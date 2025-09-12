using System;

namespace Bidzy.API.DTOs
{
    public class AppReviewReadDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
