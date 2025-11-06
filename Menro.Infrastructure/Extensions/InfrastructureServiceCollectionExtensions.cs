using Microsoft.Extensions.DependencyInjection;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Services;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Repositories;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Menro.Application.Common.Interfaces;

namespace Menro.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Infrastructure-specific services
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ISmsSender, FakeSmsSender>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IDateTimeService, DateTimeService>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
