using Bidzy.API.DTOs.user;
using Bidzy.Application.Repository.Auction;
using Bidzy.Application.Repository.User;
using Bidzy.Application.Services.Auth;
using Bidzy.Application.Services.LiveService;
using Bidzy.Domain.Entities;
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
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var liveCountService = scope.ServiceProvider.GetRequiredService<ILiveAuctionCountService>();
            try
            {


                int activeCount = await auctionRepository.ActiveAuctionCountAsync();
                int scheduledCount = await auctionRepository.ScheduledAuctionCountAsync();
                
                User Admin = await userRepository.GetUserByEmailAsync("admin@bidzy.com");

                if (Admin != null )
                {
                    await userRepository.DeleteUserAsync(Admin.Id);
                    Console.Write("Admin user Deleted");
                }
                if (Admin == null)
                {

                    string pass = PasswordHasher.Hash("adminpass");
                    User newAdmin = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Admin",
                        Email = "admin@bidzy.com",
                        imageUrl = "/Image/profile/admin",
                        Phone = "0778279843",
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        Role = Domain.Enum.UserRole.Admin,
                        PasswordHash = pass,

                    };
                    await userRepository.AddUserAsync(newAdmin);
                    Console.Write("new admin added");
                }

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