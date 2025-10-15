namespace Bidzy.API.DTOs.appReviewDtos
{
    public class AppReviewCreateDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
