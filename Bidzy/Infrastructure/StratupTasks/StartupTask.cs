using Bidzy.Application.Repository.Auction;
using Bidzy.Application.Services.LiveService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bidzy.Infrastructure.StratupTasks
{
    public class StartupTask : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public StartupTask(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Running startup task...");

            using var scope = _scopeFactory.CreateScope();

            var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var liveCountService = scope.ServiceProvider.GetRequiredService<ILiveAuctionCountService>();
            try
            {


                int activeCount = await auctionRepository.ActiveAuctionCountAsync();
                int scheduledCount = await auctionRepository.ScheduledAuctionCountAsync();

                await liveCountService.UpdateScheduledCount(scheduledCount);
                await liveCountService.UpdateOngoingCount(activeCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during startup task: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}