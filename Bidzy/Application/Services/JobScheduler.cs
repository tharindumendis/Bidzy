using System.Linq.Expressions;
using Bidzy.Application.Repository.Interfaces;
using Hangfire;

namespace Bidzy.Application.Services
{
    public class JobScheduler : IJobScheduler
    {
        public void Enqueue<T>(Expression<Action<T>> methodCall)
        {
            BackgroundJob.Enqueue(methodCall);
        }

        public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            BackgroundJob.Schedule(methodCall, delay);
        }

        public void Recurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression)
        {
            RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);
        }

        
    }
}
