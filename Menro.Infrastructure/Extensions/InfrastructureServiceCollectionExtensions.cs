using Microsoft.Extensions.DependencyInjection;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Sms;
using Menro.Application.Services.Interfaces;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Repositories;

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

            return services;
        }
    }
}
