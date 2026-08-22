using System.Net;
using System.Net.Http.Headers;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Settings;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Persistence;
using Menro.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SmsSettings>(config.GetSection("SmsSettings"));

            services.AddHttpClient<SmsIrSmsSender>((sp, http) =>
            {
                var s = sp.GetRequiredService<IOptions<SmsSettings>>().Value;

                var baseUrl = s.BaseUrl?.Trim();
                if (!baseUrl.EndsWith("/")) baseUrl += "/";

                http.BaseAddress = new Uri(baseUrl);
                http.Timeout = TimeSpan.FromSeconds(20);

                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseProxy = false,
                Proxy = null,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            // ISmsSender -> IpPanelSmsSender (با HttpClient تنظیم‌شده بالا)
            services.AddScoped<ISmsSender>(sp => sp.GetRequiredService<SmsIrSmsSender>());

            services.AddScoped<IDbInitializer, DbInitializer>();

            return services;
        }
    }
}