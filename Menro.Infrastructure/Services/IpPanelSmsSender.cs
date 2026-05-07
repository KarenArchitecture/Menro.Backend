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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    public IpPanelSmsSender(
        HttpClient http,
        IOptions<SmsSettings> settings,
        ILogger<IpPanelSmsSender> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<SmsSendResult> SendOtpAsync(
        string phoneNumber,
        string message,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return new SmsSendResult(false, "Phone number is empty.", null);

        if (string.IsNullOrWhiteSpace(message))
            return new SmsSendResult(false, "SMS message is empty.", null);

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            return new SmsSendResult(false, "IPPanel ApiKey is not configured.", null);

        if (string.IsNullOrWhiteSpace(_settings.FromNumber))
            return new SmsSendResult(false, "IPPanel FromNumber is not configured.", null);

        var recipient = NormalizeIranMobileToE164(phoneNumber);

        var body = new SendWebserviceRequest(
            sending_type: "webservice",
            from_number: _settings.FromNumber,
            message: message,
            @params: new SendWebserviceParams(new[] { recipient })
        );

        var finalUri = new Uri(_http.BaseAddress!, "api/send");
        var json = JsonSerializer.Serialize(body, JsonOptions);

        _logger.LogInformation("IPPanel final URI: {Uri}", finalUri);
        _logger.LogInformation("IPPanel FromNumber: {FromNumber}", _settings.FromNumber);
        _logger.LogInformation("IPPanel ApiKey exists: {Exists}, length: {Length}",
            !string.IsNullOrWhiteSpace(_settings.ApiKey),
            _settings.ApiKey?.Length ?? 0);
        _logger.LogInformation("IPPanel request recipient: {Recipient}", recipient);

        using var req = new HttpRequestMessage(HttpMethod.Post, finalUri);
        req.Headers.TryAddWithoutValidation("Authorization", _settings.ApiKey);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var res = await _http.SendAsync(req, ct);
            var raw = await res.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("IPPanel HTTP Status: {Status}", (int)res.StatusCode);
            _logger.LogInformation("IPPanel RAW Response: {Raw}", raw);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("IPPanel HTTP failed. Status: {Status}. Body: {Body}",
                    (int)res.StatusCode,
                    raw);

                return new SmsSendResult(false, $"HTTP {(int)res.StatusCode}: {raw}", null);
            }

            var parsed = JsonSerializer.Deserialize<IpPanelSendResponse>(raw, JsonOptions);

            if (parsed?.Meta?.Status != true)
            {
                _logger.LogWarning("IPPanel rejected request. Message: {Message}, Code: {Code}",
                    parsed?.Meta?.Message,
                    parsed?.Meta?.MessageCode);

                return new SmsSendResult(false, parsed?.Meta?.Message ?? "Send failed", null);
            }

            var outboxId = parsed.Data?.MessageOutboxIds?.FirstOrDefault();

            _logger.LogInformation(
                "IPPanel accepted OTP SMS. Phone: {Phone}, Message: {Message}, OutboxId: {OutboxId}",
                recipient,
                parsed.Meta?.Message,
                outboxId);

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
        var p = input.Trim()
            .Replace(" ", "")
            .Replace("-", "");

        if (p.StartsWith("+98"))
            return p;

        if (p.StartsWith("0098"))
            return "+98" + p[4..];

        if (p.StartsWith("98") && p.Length == 12)
            return "+" + p;

        if (p.StartsWith("09") && p.Length == 11)
            return "+98" + p[1..];

        if (p.StartsWith("9") && p.Length == 10)
            return "+98" + p;

        return p;
    }

    private sealed record SendWebserviceRequest(
        string sending_type,
        string from_number,
        string message,
        SendWebserviceParams @params
    );

    private sealed record SendWebserviceParams(
        string[] recipients
    );

    private sealed class IpPanelSendResponse
    {
        [JsonPropertyName("data")]
        public IpPanelData? Data { get; set; }

        [JsonPropertyName("meta")]
        public IpPanelMeta? Meta { get; set; }
    }

    private sealed class IpPanelData
    {
        [JsonPropertyName("message_outbox_ids")]
        public List<long>? MessageOutboxIds { get; set; }
    }

    private sealed class IpPanelMeta
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("message_code")]
        public string? MessageCode { get; set; }
    }
}