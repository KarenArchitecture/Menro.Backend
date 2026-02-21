using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Services;

public class IpPanelSmsSender : ISmsSender
{
    private readonly HttpClient _http;
    private readonly SmsSettings _settings;
    private readonly ILogger<IpPanelSmsSender> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = null };

    public IpPanelSmsSender(HttpClient http, IOptions<SmsSettings> settings, ILogger<IpPanelSmsSender> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<SmsSendResult> SendOtpAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        var recipient = NormalizeIranMobileToE164(phoneNumber);

        var body = new SendWebserviceRequest(
            sending_type: "webservice",
            from_number: _settings.FromNumber,
            message: message,
            @params: new SendWebserviceParams(new[] { recipient })
        );

        var relativePath = "api/send";
        var finalUri = new Uri(_http.BaseAddress!, relativePath); // BaseAddress الان حتماً / داره

        using var req = new HttpRequestMessage(HttpMethod.Post, finalUri);
        req.Headers.TryAddWithoutValidation("Authorization", _settings.ApiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");


        try
        {
            var res = await _http.SendAsync(req, ct);
            var raw = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("IPPanel HTTP {Status}. Body: {Body}", (int)res.StatusCode, raw);
                return new SmsSendResult(false, $"HTTP {(int)res.StatusCode}", null);
            }

            var parsed = JsonSerializer.Deserialize<IpPanelSendResponse>(raw, JsonOptions);

            if (parsed?.Meta?.Status != true)
                return new SmsSendResult(false, parsed?.Meta?.Message ?? "Send failed", null);

            var outboxId = parsed.Data?.MessageOutboxIds?.FirstOrDefault();
            return new SmsSendResult(true, parsed.Meta?.Message, outboxId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IPPanel request failed.");
            return new SmsSendResult(false, ex.Message, null);
        }
    }
    private static string NormalizeIranMobileToE164(string input)
    {
        var p = input.Trim().Replace(" ", "").Replace("-", "");
        if (p.StartsWith("+98")) return p;
        if (p.StartsWith("0098")) return "+98" + p[4..];
        if (p.StartsWith("09") && p.Length == 11) return "+98" + p[1..];
        if (p.StartsWith("9") && p.Length == 10) return "+98" + p;
        return p;
    }

    private sealed record SendWebserviceRequest(
        string sending_type,
        string from_number,
        string message,
        SendWebserviceParams @params
    );

    private sealed record SendWebserviceParams(string[] recipients);

    private sealed class IpPanelSendResponse
    {
        [JsonPropertyName("data")] public IpPanelData? Data { get; set; }
        [JsonPropertyName("meta")] public IpPanelMeta? Meta { get; set; }
    }

    private sealed class IpPanelData
    {
        [JsonPropertyName("message_outbox_ids")]
        public List<long>? MessageOutboxIds { get; set; }
    }

    private sealed class IpPanelMeta
    {
        [JsonPropertyName("status")] public bool Status { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("message_code")] public string? MessageCode { get; set; }
    }
}