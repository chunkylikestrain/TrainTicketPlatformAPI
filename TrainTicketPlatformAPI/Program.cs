using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Diagnostics;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using TrainTicketPlatformAPI.Services;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Middleware;
using TrainTicketPlatformAPI.Security;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

const string FrontendCorsPolicy = "FrontendClient";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(RateLimitPolicyNames.Auth, httpContext =>
        FixedWindow(httpContext, permitLimit: 10, window: TimeSpan.FromMinutes(1)));
    options.AddPolicy(RateLimitPolicyNames.PublicRead, httpContext =>
        FixedWindow(httpContext, permitLimit: 120, window: TimeSpan.FromMinutes(1)));
    options.AddPolicy(RateLimitPolicyNames.PublicSearch, httpContext =>
        FixedWindow(httpContext, permitLimit: 30, window: TimeSpan.FromMinutes(1)));
    options.AddPolicy(RateLimitPolicyNames.BookingWrite, httpContext =>
        FixedWindow(httpContext, permitLimit: 20, window: TimeSpan.FromMinutes(1)));
    options.AddPolicy(RateLimitPolicyNames.TicketAccess, httpContext =>
        FixedWindow(httpContext, permitLimit: 30, window: TimeSpan.FromMinutes(1)));
    options.AddPolicy(RateLimitPolicyNames.Payment, httpContext =>
        FixedWindow(httpContext, permitLimit: 10, window: TimeSpan.FromMinutes(1)));
    options.AddPolicy(RateLimitPolicyNames.AdminImport, httpContext =>
        FixedWindow(httpContext, permitLimit: 12, window: TimeSpan.FromMinutes(1)));
});

builder.Services.AddHealthChecks()
    .AddCheck("api", () => HealthCheckResult.Healthy("API is running"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Train Ticket Platform API",
        Version = "v1",
        Description = "Backend API for train search, booking, payment, and admin workflows."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT bearer token returned by /api/auth/login."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            []
        }
    });
});
builder.Services.AddDbContext<TrainTicketDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<OpenRailwayOptions>(options =>
{
    builder.Configuration.GetSection(OpenRailwayOptions.SectionName).Bind(options);

    var legacySection = builder.Configuration.GetSection("PlkOpenRailway");
    if (legacySection.Exists())
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            options.ApiKey = legacySection["ApiKey"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(options.BaseUrl) && !string.IsNullOrWhiteSpace(legacySection["BaseUrl"]))
            options.BaseUrl = legacySection["BaseUrl"]!;
    }
});
builder.Services.AddHttpClient<IOpenRailwayClient, OpenRailwayClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenRailwayOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBookingHoldExpiryService, BookingHoldExpiryService>();
builder.Services.AddHostedService<BookingHoldExpiryHostedService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITrainService, TrainService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<ITicketArtifactService, TicketArtifactService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IAdminAuditService, AdminAuditService>();
builder.Services.AddScoped<IOpenRailwayImportService, OpenRailwayImportService>();


// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is not configured");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes long.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});


var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TrainTicketDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    await LogTimedAsync(startupLogger, "EF migrations", () => db.Database.MigrateAsync());
    await DevelopmentSeedData.SeedAsync(
        db,
        app.Configuration,
        logger: startupLogger,
        contentRootPath: app.Environment.ContentRootPath);
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AdminAuditMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

static async Task LogTimedAsync(ILogger logger, string operation, Func<Task> action)
{
    var stopwatch = Stopwatch.StartNew();
    logger.LogInformation("Starting {Operation}", operation);
    await action();
    stopwatch.Stop();
    logger.LogInformation("Finished {Operation} in {ElapsedMilliseconds} ms", operation, stopwatch.ElapsedMilliseconds);
}

static RateLimitPartition<string> FixedWindow(
    HttpContext httpContext,
    int permitLimit,
    TimeSpan window)
{
    var partitionKey = httpContext.User.Identity?.IsAuthenticated == true
        ? $"user:{httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "unknown"}"
        : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"}";

    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey,
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0
        });
}

public partial class Program { }
