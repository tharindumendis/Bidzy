namespace Bidzy.Domain.Enties
{
    public class SearchHistory
    {
        public Guid Id { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public string Query { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}
