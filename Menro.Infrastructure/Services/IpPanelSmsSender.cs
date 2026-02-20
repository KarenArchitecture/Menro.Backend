using Menro.Application.Common.Interfaces;
using System.Text.Json;
using System.Text;
using Menro.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Services
{
    public class IpPanelSmsSender : ISmsSender
    {
        private readonly HttpClient _httpClient;
        private readonly SmsSettings _settings;

        public IpPanelSmsSender(
            HttpClient httpClient,
            IOptions<SmsSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string message)
        {
            try
            {
                var url = $"{_settings.BaseUrl}/api/send";

                var body = new
                {
                    sending_type = "webservice",
                    from_number = _settings.FromNumber,
                    message = message,
                    @params = new
                    {
                        recipients = new[] { phoneNumber }
                    }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("Authorization", _settings.ApiKey);

                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return false;

                var responseContent = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseContent);

                var status = doc.RootElement
                    .GetProperty("meta")
                    .GetProperty("status")
                    .GetBoolean();

                return status;
            }
            catch
            {
                return false;
            }
        }
    }
}
