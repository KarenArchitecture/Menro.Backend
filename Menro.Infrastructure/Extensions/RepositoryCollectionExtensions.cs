using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Menro.Infrastructure.Extensions
{
    public static class RepositoryCollectionExtensions
    {
        // Automatic DI for Repositories
        
        public static IServiceCollection AddAutoRegisteredRepositories(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            if (assembliesToScan == null || assembliesToScan.Length == 0)
                throw new ArgumentException("حداقل یک اسمبلی باید برای اسکن مشخص شود.");

            foreach (var assembly in assembliesToScan)
            {
                // همه کلاس‌های public غیر abstract که پسوند Repository دارن
                var repoTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"));

                foreach (var implType in repoTypes)
                {
                    // اینترفیس‌هایی که این کلاس implement کرده، معمولاً یکی با I + نام کلاس
                    var interfaceType = implType.GetInterfaces()
                        .FirstOrDefault(i => i.Name == "I" + implType.Name);

                    if (interfaceType != null)
                    {
                        services.AddScoped(interfaceType, implType);
                    }
                }
            }

            return services;
        }
    }
}
/*
چجوری کار می‌کنه؟
همونطور که ServiceCollectionExtensions کار میکنه، همونجا توضیحات کامل داده شده
*/