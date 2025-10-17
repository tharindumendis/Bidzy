namespace Bidzy.API.DTOs.appReview
{
    public class AppReviewCreateDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
