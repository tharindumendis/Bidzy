using System.Text;
using AspNetCoreRateLimit;
using Bidzy.API.Hubs;
using Bidzy.Application;
using Bidzy.Application.Converters;
using Bidzy.Application.Repository;
using Bidzy.Application.Repository.Interfaces;
using Bidzy.Application.Services;
using Bidzy.Application.Services.Admin;
using Bidzy.Application.Services.AuctionEngine;
using Bidzy.Application.Services.Auth;
using Bidzy.Application.Services.Email;
using Bidzy.Application.Services.NotificationEngine;
using Bidzy.Application.Services.NotificationSchedulerService;
using Bidzy.Application.Services.Payments;
using Bidzy.Application.Services.Scheduler;
using Bidzy.Application.Services.SignalR;
using Bidzy.Application.Settings;
using Bidzy.Application.Validators;
using Bidzy.Data;
using Bidzy.Domain.Enties;
using FluentValidation.AspNetCore;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://192.168.8.134:3000", "http://localhost:3000") // your frontend IP
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
    });
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateCheckoutSessionRequestValidator>());
// Configure IP Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bidzy API", Version = "v1" });

    //  Add JWT Bearer Authorization
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
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
// Stripe settings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
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
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials()
//              .SetIsOriginAllowed(_ => true); // Adjust for production
//    });
//});




// Configure Job Services
builder.Services.AddTransient<IEmailJobService, EmailJobService>();
builder.Services.AddSingleton<IJobScheduler, JobScheduler>();
builder.Services.AddSingleton<ILiveAuctionCountService, LiveAuctionCountService>();
builder.Services.AddTransient<INotificationSchedulerService, NotificationSchedulerService>();
builder.Services.AddScoped<ISignalRNotifier, SignalRNotifier>();
builder.Services.AddScoped<IAuctionEngine, AuctionEngine>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// tthis class run when server start
builder.Services.AddHostedService<StartupTask>();



// Configure Entity Repository
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddHangfireServer();
builder.Services.AddTransient<IAuctionRepository, AuctionRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IBidRepository, BidRepository>();
builder.Services.AddTransient<ITagRepository, TagRepository>();
builder.Services.AddTransient<IUserAuctionFavoriteRepository, UserAuctionFavoriteRepository>();
builder.Services.AddTransient<ISearchhistoryRepository, SearchHistoryRepository>();
builder.Services.AddTransient<INotificationRepository, NotificationRepository>();
builder.Services.AddTransient<IPaymentRepository, PaymentRepository>();
builder.Services.AddTransient<IViewHistoryRepository, ViewHistoryRepository>();
builder.Services.AddTransient<IAppReviewRepository, AppReviewRepository>();





var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var hangfireSection = app.Configuration.GetSection("Hangfire");
if (hangfireSection.GetValue<bool>("EnableDashboard"))
{
    var dashboardPath = hangfireSection.GetValue<string>("DashboardPath") ?? "/hangfire";
    var dashboardUser = hangfireSection.GetValue<string>("DashboardUser") ?? "admin";
    var dashboardPass = hangfireSection.GetValue<string>("DashboardPass") ?? "admin";

    app.UseHangfireDashboard(dashboardPath,
        new DashboardOptions
        {
            DashboardTitle = "Bidzy Hangfire Dashboard",
            DisplayStorageConnectionString = false,
            Authorization = new[]
            {
                new HangfireCustomBasicAuthenticationFilter
                {
                    User = dashboardUser,
                    Pass = dashboardPass
                }
            }
        });
}

app.UseRouting();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseIpRateLimiting();

app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/auctionHub");
app.MapHub<UserHub>("/userHub");




app.Run();
