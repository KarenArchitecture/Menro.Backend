using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Menro.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            if (assembliesToScan == null || assembliesToScan.Length == 0)
                throw new ArgumentException("حداقل یک اسمبلی باید برای اسکن مشخص شود.");

            foreach (var assembly in assembliesToScan)
            {
                // همه کلاس‌های public غیر abstract که پسوند Service دارن
                var serviceTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"));

                foreach (var implType in serviceTypes)
                {
                    // اینترفیس‌هایی که این کلاس implement کرده، معمولاً فقط یک اینترفیس اصلی هست
                    var interfaceType = implType.GetInterfaces()
                        .FirstOrDefault(i => i.Name == "I" + implType.Name);

                    if (interfaceType != null)
                    {
                        // رجیستر کردن با lifetime Scoped
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
AddAutoRegisteredServices یک یا چند اسمبلی می‌گیره برای اسکن.
برای هر اسمبلی، می‌ره همه کلاس‌ها رو پیدا می‌کنه که نامشون با Service تموم می‌شه.
برای هر کلاس، می‌گرده ببینه این کلاس کدوم اینترفیس با نام مشابه IClassName رو پیاده کرده.
اگه پیدا کرد، services.AddScoped<IClassName, ClassName>() می‌زنه.
در نتیجه تو فقط کافیه اینترفیس و کلاس رو درست نامگذاری کنی (مثلاً IAuthService و AuthService)، نیازی نیست دستی خط تزریق بنویسی.
 */