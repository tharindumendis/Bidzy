namespace Bidzy.API.DTOs
{
    public class AppReviewCreateDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
