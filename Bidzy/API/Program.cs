using System.Text;
using Bidzy.API.Hubs;
using Bidzy.Application;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Bidzy.Application.Services.AuctionEngine;
using Bidzy.Application.Services.Auth;
using Bidzy.Application.Services.NotificationSchedulerService;
using Bidzy.Application.Services.SignalR;
using Bidzy.Data;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bidzy API", Version = "v1" });

    // 🔐 Add JWT Bearer Authorization
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer eyJhbGciOiJIUzI"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//// Temp
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = "Dummy";
//    options.DefaultChallengeScheme = "Dummy";
//}).AddScheme<AuthenticationSchemeOptions, DummyAuthHandler>("Dummy", options => { });
//// End Temp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "yourIssuer",
        ValidAudience = "yourAudience",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bidzyUltraSecureKey_2025!@#LongEnoughToPass"))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for SignalR hub
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/auctionHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();


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
builder.Services.AddSingleton<ILiveAuctionCountService, LiveAuctionCountService>();
builder.Services.AddTransient<INotificationSchedulerService, NotificationSchedulerService>();
builder.Services.AddScoped<ISignalRNotifier, SignalRNotifier>();
builder.Services.AddScoped<IAuctionEngine, AuctionEngine>();
builder.Services.AddScoped<IAuthService, AuthService>();
// Configure Entity Repository
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddTransient<IAuctionRepository, AuctionRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IBidRepository, BidRepository>();
builder.Services.AddTransient<ITagRepository, TagRepository>();
builder.Services.AddTransient<IUserAuctionFavoriteRepository, UserAuctionFavoriteRepository>();

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
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireServer();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/auctionHub");
app.MapHub<UserHub>("/userHub");




app.Run();
