using Bidzy.Domain.Entities;

namespace Bidzy.Application.Repository.Tag
{
    public interface ITagRepository
    {
        Task<List<Domain.Entities.Tag>> ResolveTagsAsync(List<string> tagNames);
    }
}
