using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Channels
{
    /// <summary>
    /// Web Portal channel adapter for Salama Protocol
    /// Handles browser-based authentication with web-specific behavioral detection
    /// </summary>
    public class WebPortalChannelAdapter
    {
        /// <summary>
        /// Create authentication request from web login form
        /// </summary>
        public AuthenticationRequest CreateAuthRequest(
            string accountId,
            string pinCode,
            WebBehavioralData webBehavior,
            string ipAddress,
            string userAgent)
        {
            return new AuthenticationRequest
            {
                AccountId = accountId,
                PinHash = HashPin(pinCode),
                DeviceId = webBehavior.BrowserFingerprint,
                BehavioralData = ConvertWebBehavior(webBehavior),
                ChannelMetadata = new ChannelMetadata
                {
                    Channel = ChannelType.WebPortal,
                    UserAgent = userAgent,
                    IpAddress = ipAddress,
                    BrowserFingerprint = webBehavior.BrowserFingerprint,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "screen_resolution", webBehavior.ScreenResolution },
                        { "timezone", webBehavior.Timezone },
                        { "is_incognito", webBehavior.IsIncognito.ToString() }
                    }
                }
            };
        }

        /// <summary>
        /// Detect duress signals from web-based behavioral patterns
        /// </summary>
        public bool DetectWebDuress(WebBehavioralData webBehavior)
        {
            // Mouse tremor detection - erratic movements during PIN entry
            if (webBehavior.MouseMovementVariance > 0.6)
            {
                return true; // High mouse tremor suggests duress
            }

            // Unusual typing patterns - long pauses or many corrections
            if (webBehavior.TypingPauseAverage > 2000 || // 2+ seconds between keys
                webBehavior.BackspaceCount > 3)
            {
                return true; // Hesitation suggests duress
            }

            // Browser context changes during PIN entry
            if (webBehavior.TabSwitchCount > 0 || webBehavior.WindowBlurCount > 0)
            {
                return true; // Distraction/monitoring by attacker
            }

            // Unusual time of day for this user
            if (webBehavior.IsUnusualTimeOfDay)
            {
                // Additional risk factor, not definitive
                return webBehavior.MouseMovementVariance > 0.4; // Lower threshold
            }

            return false;
        }

        /// <summary>
        /// Convert web behavioral data to generic behavioral data
        /// </summary>
        private BehavioralData ConvertWebBehavior(WebBehavioralData webBehavior)
        {
            return new BehavioralData
            {
                // Map mouse tremor to accelerometer variance equivalent
                AccelerometerVariance = webBehavior.MouseMovementVariance,
                EntryDurationMs = webBehavior.TotalEntryDuration,
                CorrectionCount = webBehavior.BackspaceCount
            };
        }

        /// <summary>
        /// Generate shadow mode web response
        /// Maintains consistent UI/UX to avoid alerting attacker
        /// </summary>
        public WebAuthResponse CreateShadowResponse(string accountId)
        {
            return new WebAuthResponse
            {
                Success = true,
                SessionToken = GenerateMockToken(),
                RedirectUrl = "/dashboard", // Same as normal mode
                UserDisplayName = GetUserDisplayName(accountId),
                // No indication of shadow mode in response
            };
        }

        private string HashPin(string pin)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateMockToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private string GetUserDisplayName(string accountId)
        {
            // Placeholder - would retrieve from user service
            return "User";
        }
    }

    /// <summary>
    /// Web-specific behavioral data collected via JavaScript
    /// </summary>
    public class WebBehavioralData
    {
        public string BrowserFingerprint { get; set; } = string.Empty;
        public double MouseMovementVariance { get; set; }
        public int TotalEntryDuration { get; set; }
        public int BackspaceCount { get; set; }
        public int TypingPauseAverage { get; set; }
        public int TabSwitchCount { get; set; }
        public int WindowBlurCount { get; set; }
        public bool IsUnusualTimeOfDay { get; set; }
        public string ScreenResolution { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public bool IsIncognito { get; set; }
    }

    /// <summary>
    /// Web authentication response
    /// </summary>
    public class WebAuthResponse
    {
        public bool Success { get; set; }
        public string SessionToken { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
    }
}
