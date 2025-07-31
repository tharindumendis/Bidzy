using Bidzy.Application.Repository.Interfaces;
using Bidzy.Data;
using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext dbContext;

        public TagRepository (ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Tag>> ResolveTagsAsync(List<string> tagNames)
        {
            var tagList = new List<Tag>();

            foreach (var tagName in tagNames)
            {
                var normalizedName = tagName.Trim();
                var existingName = dbContext.Tags
                    .FirstOrDefault(t => t.tagName == normalizedName);
                if (existingName != null)
                {
                   
                    tagList.Add(existingName);
                }
                else
                {
                    var newTag = new Tag { tagId = Guid.NewGuid(), tagName = normalizedName };
                    tagList.Add(newTag);
                    dbContext.Tags.Add(newTag);
                }
            }
            return tagList;
        }
    }
}
