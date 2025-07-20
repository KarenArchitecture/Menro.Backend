using Menro.Application.Services.Interfaces;
using Menro.Application.DTO;
using Menro.Infrastructure.Repositories;
using Menro.Application.Services.Implementations;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Menro.Infrastructure.Data;
using Menro.Web.Middleware;
using Menro.Domain.Entities;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Application.Restaurants.Services.Implementations;
using Menro.Application.Common;
using Menro.Application.Common.Settings;
using Menro.Application.Foods.Services.Implementations;
using Menro.Application.Foods.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<MenroDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.User.RequireUniqueEmail = false; // Set this to false
})
    .AddEntityFrameworkStores<MenroDbContext>()
    .AddDefaultTokenProviders();

// -------------------- Configure JWT --------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// -------------------- Application Services --------------------
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<IFoodCategoryService, FoodCategoryService>();
builder.Services.AddScoped<IFoodCardService, FoodCardService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IFeaturedRestaurantService, FeaturedRestaurantService>();
builder.Services.AddScoped<IRestaurantCategoryService, RestaurantCategoryService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<IRestaurantAdBannerService, RestaurantAdBannerService>();
builder.Services.AddScoped<IRandomRestaurantCardService, RandomRestaurantCardService>();
builder.Services.AddScoped<IUserRecentOrderCardService, UserRecentOrderCardService>();
builder.Services.AddScoped<IRestaurantShopBannerService, RestaurantShopBannerService>();

// -------------------- Unit of Work --------------------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// -------------------- Repository Layer --------------------
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IFoodCategoryRepository, FoodCategoryRepository>();
builder.Services.AddScoped<IRestaurantCategoryRepository, RestaurantCategoryRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// -------------------- Swagger --------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Menro API", Version = "v1" });
});

// -------------------- Controllers --------------------
builder.Services.AddControllers();

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDevClient", builder =>
    {
        builder.WithOrigins("http://localhost:5173") // <-- React dev server
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// -------------------- API Versioning --------------------
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

// -------------------- Build App --------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------- Middleware --------------------
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowReactDevClient");

app.UseErrorHandlingMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -------------------- Run DbInitializer --------------------
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await dbInitializer.InitializeAsync(); // if it's async, or .Initialize() if sync
}

app.Run();
