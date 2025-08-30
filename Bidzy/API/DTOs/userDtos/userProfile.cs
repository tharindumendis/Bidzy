namespace Bidzy.API.DTOs.userDtos
{
    public class userProfile
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public List<string> FavoriteAuctions { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }
}
