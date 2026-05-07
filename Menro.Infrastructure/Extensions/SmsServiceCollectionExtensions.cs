using System.Net.Http.Headers;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Settings;
using Menro.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Extensions
{
    public static class SmsServiceCollectionExtensions
    {
        public static IServiceCollection AddSms(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SmsSettings>(config.GetSection("SmsSettings"));

            services.AddHttpClient<IpPanelSmsSender>((sp, http) =>
            {
                var s = sp.GetRequiredService<IOptions<SmsSettings>>().Value;

                http.BaseAddress = new Uri(s.BaseUrl.TrimEnd('/'));
                http.Timeout = TimeSpan.FromSeconds(15);
                http.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddScoped<ISmsSender>(sp => sp.GetRequiredService<IpPanelSmsSender>());

            return services;
        }
    }
}