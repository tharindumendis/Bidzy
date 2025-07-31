using System.Text.Json.Serialization;

namespace Bidzy.Domain.Enties
{
    public class Tag
    {
        public Guid tagId { get; set; }
        public string tagName { get; set; }

        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
