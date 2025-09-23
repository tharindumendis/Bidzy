namespace Bidzy.Application.Settings
{
    public class StripeSettings
    {
        public string PublishableKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string Currency { get; set; } = "usd";
        public decimal CommissionRate { get; set; } = 0.10m; // 10%
        public bool SkipSignatureVerification { get; set; } = false; // Dev-only helper
        public int WebhookToleranceSeconds { get; set; } = 300; // Signature timestamp tolerance
    }
}

