using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Settings;
using Menro.Application.Extensions;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Extensions;
using Menro.Infrastructure.Repositories;
using Menro.Infrastructure.Services;
using Menro.Web.Middleware;
using Menro.Web.Services;
using Menro.Web.Services.Implementations;
using Menro.Infrastructure.Data.Seed.Core.Seeders;
using Menro.Infrastructure.Data.Seed.Contracts;
using Menro.Infrastructure.Data.Seed.Demo.Seeders;
using Menro.Infrastructure.Seed.Demo.Seeders;
using Menro.Web.Hubs;
using Menro.Web.Hubs.SignalR;
using Microsoft.AspNetCore.SignalR;
using Menro.Application.Common.Implementations;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment =
    builder.Environment.IsDevelopment();

#region Required Configuration Validation

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing. Set Environment Variable: ConnectionStrings__DefaultConnection");
}

var jwtSecret =
    builder.Configuration["JwtSettings:Secret"];

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "JWT secret is missing. Set Environment Variable: JwtSettings__Secret");
}

var jwtIssuer =
    builder.Configuration["JwtSettings:Issuer"];

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT issuer is missing. Configure JwtSettings:Issuer in appsettings.Production.json");
}

var jwtAudience =
    builder.Configuration["JwtSettings:Audience"];

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT audience is missing. Configure JwtSettings:Audience in appsettings.Production.json");
}

#endregion

#region DbContext & Identity

builder.Services.AddDbContext<MenroDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });

    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<MenroDbContext>()
.AddDefaultTokenProviders();

#endregion

#region Authentication & JWT

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear();
});

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<JwtSettings>>().Value);

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)
                ),

            ClockSkew = TimeSpan.Zero,

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken =
                context.Request.Query["access_token"];

            var path =
                context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/music"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

#endregion

#region Services

builder.Services.AddInfrastructureServices(
    builder.Configuration);

var applicationAssembly =
    Assembly.Load("Menro.Application");

builder.Services.AddAutoRegisteredServices(
    applicationAssembly);

var infrastructureAssembly =
    Assembly.Load("Menro.Infrastructure");

builder.Services.AddAutoRegisteredRepositories(
    infrastructureAssembly);

// multi-layered services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileUrlService, FileUrlService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
builder.Services.AddScoped<IMusicNotificationService, MusicNotificationService>(); 

builder.Services.AddSingleton<IGlobalDateTimeService, GlobalDateTimeService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddMemoryCache();

builder.Services.AddSignalR();

#endregion

#region Data Seeders

/* ============================================================
   CORE SEEDERS
============================================================ */

builder.Services.AddScoped<IDataSeeder, RoleSeeder>();
builder.Services.AddScoped<IDataSeeder, AdminSeeder>();

builder.Services.AddScoped<IDataSeeder, IconSeeder>();
builder.Services.AddScoped<IDataSeeder, GlobalFoodCategorySeeder>();

/* ============================================================
   DEMO SEEDERS
============================================================ */

builder.Services.AddScoped<IDataSeeder, DemoRestaurantSeeder>();

builder.Services.AddScoped<IDataSeeder, DemoRestaurantAdSeeder>();

builder.Services.AddScoped<IDataSeeder, DemoVariantSeeder>();

builder.Services.AddScoped<IDataSeeder, DemoDiscountSeeder>();

builder.Services.AddScoped<IDataSeeder, DemoRatingSeeder>();

builder.Services.AddScoped<IDataSeeder, DemoCustomerSeeder>();

builder.Services.AddScoped<IDataSeeder, DemoFavoriteFoodSeeder>();

builder.Services.AddScoped<IDataSeeder, DemoOrderSeeder>();

builder.Services.AddScoped<IDataSeeder, DemoBlogSeeder>();

#endregion

#region API

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.CamelCase;

    options.JsonSerializerOptions.DictionaryKeyPolicy =
        JsonNamingPolicy.CamelCase;

    options.JsonSerializerOptions.PropertyNameCaseInsensitive =
        true;

    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Menro API",
        Version = "v1"
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;

    options.DefaultApiVersion =
        new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);

    options.ReportApiVersions = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactClient", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://89.33.129.71",
                "https://89.33.129.71",
                "http://menro.ir",
                "https://menro.ir",
                "http://www.menro.ir",
                "https://www.menro.ir"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
#endregion

var app = builder.Build();

#region Middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Menro API v1");

        c.RoutePrefix = "swagger";
    });

    // app.UseHsts();
}

// app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowReactClient");

app.UseStaticFiles();

app.UseErrorHandlingMiddleware();
app.UseForwardedHeaders();

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Routing

app.MapControllers();
app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "Healthy",
        app = "Menro API",
        environment = app.Environment.EnvironmentName,
        time = DateTime.UtcNow
    }))
.AllowAnonymous();

//hubs
app.MapHub<MusicHub>("/hubs/music")
    .RequireCors("AllowReactClient");


#endregion

#region DB Initialization

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();

    await dbInitializer.InitializeAsync();
}

#endregion

app.Logger.LogInformation("Menro.Api is running");

app.Run();