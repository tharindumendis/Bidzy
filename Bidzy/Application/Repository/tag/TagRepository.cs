using Bidzy.Domain.Entities;
using Bidzy.Infrastructure.Data;

namespace Bidzy.Application.Repository.Tag
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext dbContext;

        public TagRepository (ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<List<Domain.Entities.Tag>> ResolveTagsAsync(List<string> tagNames)
        {
            var tagList = new List<Domain.Entities.Tag>();

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
                    var newTag = new Domain.Entities.Tag { tagId = Guid.NewGuid(), tagName = normalizedName };
                    tagList.Add(newTag);
                    dbContext.Tags.Add(newTag);
                }
            }
            return Task.FromResult(tagList);
        }
    }
}
