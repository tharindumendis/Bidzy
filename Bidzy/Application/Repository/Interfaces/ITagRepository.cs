using Bidzy.Domain.Enties;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface ITagRepository
    {
        Task<List<Tag>> ResolveTagsAsync(List<string> tagNames);
    }
}
