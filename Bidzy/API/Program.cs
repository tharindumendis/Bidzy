using Bidzy.API.Hubs;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Bidzy.Application.Services.SignalR;
using Bidzy.Data;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure Entity Framework Core with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Configure Email Job Service
builder.Services.AddScoped<IEmailJobService, EmailJobService>();
// Configure SignalR for real-time notifications
builder.Services.AddSignalR();
// Configure Hangfire for scheduling
builder.Services.AddHangfire((sp, config) =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("HangfireConnection");
    config.UseSqlServerStorage(connectionString);
});
builder.Services.AddScoped<IJobScheduler, JobScheduler>();
// Configure CORS Policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true); // Adjust for production
    });
});
// Configure Job Services
builder.Services.AddTransient<IEmailJobService, EmailJobService>();
builder.Services.AddSingleton<IJobScheduler, JobScheduler>();
builder.Services.AddTransient<INotificationSchedulerService, NotificationSchedulerService>();
builder.Services.AddScoped<ISignalRNotifier, SignalRNotifier>();
// Configure Entity Repository
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddTransient<IAuctionRepository, AuctionRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IBidRepository, BidRepository>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/test/job",
        new DashboardOptions
        {
            DashboardTitle = "Hangfire Job Demo Application",
            DisplayStorageConnectionString = false,
            Authorization = new[]
        {
            new HangfireCustomBasicAuthenticationFilter
            {
                User = "admin",
                Pass = "admin"
            }
        }
        }
        );
}

app.UseRouting();

app.UseCors();

app.UseHangfireServer();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/auctionHub");


app.Run();
