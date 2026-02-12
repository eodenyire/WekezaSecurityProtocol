namespace WekezaSecurityProtocol.Models
{
    /// <summary>
    /// Defines the channel through which the authentication request originated
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// Mobile application (iOS/Android)
        /// </summary>
        MobileApp,

        /// <summary>
        /// Web banking portal (browser-based)
        /// </summary>
        WebPortal,

        /// <summary>
        /// STK Push (M-Pesa/mobile money integration)
        /// </summary>
        StkPush,

        /// <summary>
        /// USSD channel (*234# menu-based)
        /// </summary>
        Ussd,

        /// <summary>
        /// API integration (third-party systems)
        /// </summary>
        ApiIntegration
    }

    /// <summary>
    /// Channel-specific metadata for authentication requests
    /// </summary>
    public class ChannelMetadata
    {
        /// <summary>
        /// The channel type for this authentication request
        /// </summary>
        public ChannelType Channel { get; set; }

        /// <summary>
        /// Session ID for stateful channels (USSD, STK)
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// User agent string (for web and mobile)
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// IP address of the request origin
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// For USSD: The USSD code entered (e.g., *234*1234#)
        /// </summary>
        public string? UssdCode { get; set; }

        /// <summary>
        /// For STK: Transaction reference from M-Pesa
        /// </summary>
        public string? StkTransactionRef { get; set; }

        /// <summary>
        /// For Web: Browser fingerprint for device tracking
        /// </summary>
        public string? BrowserFingerprint { get; set; }

        /// <summary>
        /// Additional channel-specific data (JSON)
        /// </summary>
        public Dictionary<string, string>? AdditionalData { get; set; }
    }
}
