namespace Menro.Application.Common.Settings
{
    public class SmsSettings
    {
        public string ApiKey { get; set; } = default!;
        public string BaseUrl { get; set; } = default!;
        public string FromNumber { get; set; } = default!;
        public string? OtpPatternCode { get; set; }
    }
}
