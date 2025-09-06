using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;

namespace Bidzy.Application
{
    public class StartupTask : IHostedService
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly ILiveAuctionCountService _liveCountService;

        public StartupTask(IAuctionRepository auctionRepository, ILiveAuctionCountService liveCountService)
        {
            _auctionRepository = auctionRepository;
            _liveCountService = liveCountService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Running startup task...");

            await StartMethodAsync();
        }

        private async Task StartMethodAsync()
        {
            int activeCount = await _auctionRepository.ActiveAuctionCountAsync();
            int scheduledCount = await _auctionRepository.ScheduledAuctionCountAsync();

            await _liveCountService.UpdateScheduledCount(scheduledCount);
            await _liveCountService.UpdateOngoingCount(activeCount);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}