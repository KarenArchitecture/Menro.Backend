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

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();


#region Required Configuration Validation

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing. Set Environment Variable: ConnectionStrings__DefaultConnection");
}

var jwtSecret = builder.Configuration["JwtSettings:Secret"];

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "JWT secret is missing. Set Environment Variable: JwtSettings__Secret");
}

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT issuer is missing. Configure JwtSettings:Issuer in appsettings.Production.json");
}

var jwtAudience = builder.Configuration["JwtSettings:Audience"];

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

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtSettings>>().Value);

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

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret)
        ),

        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

#endregion

#region DI Services

builder.Services.AddInfrastructureServices(builder.Configuration);

var applicationAssembly = Assembly.Load("Menro.Application");
builder.Services.AddAutoRegisteredServices(applicationAssembly);

var infrastructureAssembly = Assembly.Load("Menro.Infrastructure");
builder.Services.AddAutoRegisteredRepositories(infrastructureAssembly);

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileUrlService, FileUrlService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

builder.Services.AddSingleton<IGlobalDateTimeService, GlobalDateTimeService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddMemoryCache();

#endregion

#region API

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Menro API", Version = "v1" });
});

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactClient", policy =>
    {
        if (isDevelopment)
        {
            policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            );
        }
        else // isProdustion
        {
            policy.WithOrigins(
                "http://89.33.129.71"
            );
        }

        policy.AllowAnyHeader()
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
    app.UseHsts();
}

// فعلاً چون SSL/domain نداری و BaseUrl روی http است، این را فعال نکن.
// وقتی HTTPS واقعی راه افتاد، این خط را برگردان.
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseErrorHandlingMiddleware();

app.UseRouting();

app.UseCors("AllowReactClient");

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Routing

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    app = "Menro API",
    environment = app.Environment.EnvironmentName,
    time = DateTime.UtcNow
}))
.AllowAnonymous();

#endregion

#region DB Initialization

if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await dbInitializer.InitializeAsync();
}

#endregion

app.Run();