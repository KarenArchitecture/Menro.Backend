using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Services;

// ارسال OTP از طریق قالب (Template) در SMS.ir
// مستندات: https://app.sms.ir/developer/help/verify  |  endpoint: POST /v1/send/verify
//
// نکته مهم: این متد به‌جای متن آزاد، از یک «قالب» از پیش تعریف‌شده در پنل
// sms.ir استفاده می‌کند (بخش «لیست قالب‌ها»). باید توی پنل یک قالب مثل:
//   کد تایید شما: #Code#
// بسازی، TemplateId اون رو بگیری و توی تنظیمات پروژه بذاری.
// نام پارامتر (اینجا "Code") هم باید دقیقاً با اسمی که توی قالب تعریف کردی یکی باشه.
public class SmsIrSmsSender : ISmsSender
{
    private readonly HttpClient _http;
    private readonly SmsSettings _settings;
    private readonly ILogger<SmsIrSmsSender> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SmsIrSmsSender(
        HttpClient http,
        IOptions<SmsSettings> settings,
        ILogger<SmsIrSmsSender> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    // ⚠️ توجه: امضای این متد نسبت به قبل عوض شده.
    // قبلاً پارامتر دوم "message" کامل بود (مثلاً "کد تایید شما: 12345").
    // الان چون قالب توی پنل sms.ir متن رو می‌سازه، پارامتر دوم فقط
    // خودِ کد (مثلاً "12345") است، نه متن کامل. باید ISmsSender و
    // فراخوانی‌اش توی AuthService هم مطابق همین آپدیت بشه (پایین توضیح دادم).
    public async Task<SmsSendResult> SendOtpAsync(
        string phoneNumber,
        string code,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return new SmsSendResult(false, "Phone number is empty.", null);

        if (string.IsNullOrWhiteSpace(code))
            return new SmsSendResult(false, "OTP code is empty.", null);

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            return new SmsSendResult(false, "SMS.ir ApiKey is not configured.", null);

        if (_settings.OtpTemplateId <= 0)
            return new SmsSendResult(false, "SMS.ir OtpTemplateId is not configured.", null);

        var recipient = NormalizeIranMobileLocal(phoneNumber);

        var body = new VerifySendModel(
            Mobile: recipient,
            TemplateId: _settings.OtpTemplateId,
            Parameters: new[] { new VerifySendParameterModel("Code", code) }
        );

        var finalUri = new Uri(_http.BaseAddress!, "v1/send/verify");
        var json = JsonSerializer.Serialize(body, JsonOptions);

        _logger.LogInformation("SMS.ir final URI: {Uri}", finalUri);
        _logger.LogInformation("SMS.ir TemplateId: {TemplateId}", _settings.OtpTemplateId);
        _logger.LogInformation("SMS.ir ApiKey exists: {Exists}, length: {Length}",
            !string.IsNullOrWhiteSpace(_settings.ApiKey),
            _settings.ApiKey?.Length ?? 0);
        _logger.LogInformation("SMS.ir request recipient: {Recipient}", recipient);

        using var req = new HttpRequestMessage(HttpMethod.Post, finalUri);
        req.Headers.TryAddWithoutValidation("x-api-key", _settings.ApiKey);
        req.Headers.TryAddWithoutValidation("Accept", "text/plain");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var res = await _http.SendAsync(req, ct);
            var raw = await res.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("SMS.ir HTTP Status: {Status}", (int)res.StatusCode);
            _logger.LogInformation("SMS.ir RAW Response: {Raw}", raw);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("SMS.ir HTTP failed. Status: {Status}. Body: {Body}",
                    (int)res.StatusCode,
                    raw);

                return new SmsSendResult(false, $"HTTP {(int)res.StatusCode}: {raw}", null);
            }

            var parsed = JsonSerializer.Deserialize<SmsIrSendResponse>(raw, JsonOptions);

            // در SMS.ir مقدار status == 1 یعنی موفق
            if (parsed is null || parsed.Status != 1)
            {
                _logger.LogWarning("SMS.ir rejected request. Message: {Message}, Status: {Status}",
                    parsed?.Message,
                    parsed?.Status);

                return new SmsSendResult(false, parsed?.Message ?? "Send failed", null);
            }

            var messageId = parsed.Data?.MessageId;

            _logger.LogInformation(
                "SMS.ir accepted OTP verify SMS. Phone: {Phone}, Message: {Message}, MessageId: {MessageId}",
                recipient,
                parsed.Message,
                messageId);

            return new SmsSendResult(true, parsed.Message, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS.ir request failed.");
            return new SmsSendResult(false, ex.Message, null);
        }
    }

    // طبق نمونه‌ی sms.ir، شماره باید بدون صفر ابتدایی و بدون +98 باشه: 9xxxxxxxxx
    private static string NormalizeIranMobileLocal(string input)
    {
        var p = input.Trim()
            .Replace(" ", "")
            .Replace("-", "");

        if (p.StartsWith("+98"))
            return p[3..];

        if (p.StartsWith("0098"))
            return p[4..];

        if (p.StartsWith("98") && p.Length == 12)
            return p[2..];

        if (p.StartsWith("09") && p.Length == 11)
            return p[1..];

        if (p.StartsWith("9") && p.Length == 10)
            return p;

        return p;
    }

    private sealed record VerifySendParameterModel(string Name, string Value);

    private sealed record VerifySendModel(
        string Mobile,
        int TemplateId,
        VerifySendParameterModel[] Parameters
    );

    private sealed class SmsIrSendResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public SmsIrData? Data { get; set; }
    }

    private sealed class SmsIrData
    {
        [JsonPropertyName("messageId")]
        public long MessageId { get; set; }

        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }
    }
}
