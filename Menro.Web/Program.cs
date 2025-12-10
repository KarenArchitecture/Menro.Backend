using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Menro.Application.Common.Settings;
using Menro.Domain.Entities;
using Menro.Infrastructure.Data;
using Menro.Web.Middleware;
using Menro.Infrastructure.Extensions;
using Menro.Application.Extensions;
using Menro.Web.Services;
using Menro.Application.Common.Interfaces;
using Menro.Web.Services.Implementations;
using Menro.Infrastructure.Services;
using System.Security.Claims;
using Menro.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#region DbContext & Identity

builder.Services.AddDbContext<MenroDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");

    if (builder.Environment.IsDevelopment())
        options.UseSqlServer(conn);
    else
        options.UseNpgsql(conn);
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

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])
        ),
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});

#endregion

#region DI Services

builder.Services.AddInfrastructureServices();

var appAssembly = Assembly.Load("Menro.Application");
builder.Services.AddAutoRegisteredServices(appAssembly);

var infraAssembly = Assembly.Load("Menro.Infrastructure");
builder.Services.AddAutoRegisteredRepositories(infraAssembly);

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileUrlService, FileUrlService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
builder.Services.AddMemoryCache();

#endregion

#region API & MVC

builder.Services.AddControllersWithViews();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

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
    options.AddPolicy("AllowReactDevClient", p =>
    {
        p.WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "http://89.33.129.91"
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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactDevClient");
app.UseStaticFiles();
app.UseErrorHandlingMiddleware();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Routing

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

#endregion

#region DB Initialization (Production Only)

if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var init = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await init.InitializeAsync();  // Includes: Migrate + Seed
}

#endregion

app.Run();
