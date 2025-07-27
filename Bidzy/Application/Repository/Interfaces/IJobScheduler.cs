using System.Linq.Expressions;

namespace Bidzy.Application.Repository.Interfaces
{
    public interface IJobScheduler
    {
        void Enqueue<T>(Expression<Action<T>> methodCall);
        void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
        void Recurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);
        
    }

}
