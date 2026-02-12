using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Channels
{
    /// <summary>
    /// STK Push (M-Pesa) channel adapter for Salama Protocol
    /// Handles authentication and transaction flows via mobile money
    /// </summary>
    public class StkPushChannelAdapter
    {
        /// <summary>
        /// Create authentication request from STK Push callback
        /// </summary>
        /// <param name="phoneNumber">M-Pesa registered phone number</param>
        /// <param name="pinCode">PIN entered via USSD prompt</param>
        /// <param name="transactionRef">M-Pesa transaction reference</param>
        /// <returns>Authentication request</returns>
        public AuthenticationRequest CreateAuthRequest(
            string phoneNumber, 
            string pinCode, 
            string transactionRef)
        {
            return new AuthenticationRequest
            {
                AccountId = phoneNumber,
                PinHash = HashPin(pinCode),
                DeviceId = $"STK-{phoneNumber}",
                ChannelMetadata = new ChannelMetadata
                {
                    Channel = ChannelType.StkPush,
                    SessionId = transactionRef,
                    StkTransactionRef = transactionRef,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "mpesa_transaction_ref", transactionRef },
                        { "timestamp", DateTime.UtcNow.ToString("O") }
                    }
                }
            };
        }

        /// <summary>
        /// Process STK Push callback with potential duress detection
        /// </summary>
        /// <param name="callback">M-Pesa callback data</param>
        /// <returns>Whether this appears to be a duress transaction</returns>
        public bool DetectDuressFromStkCallback(StkCallbackData callback)
        {
            // Check for reversed PIN via USSD sequence
            // User might enter *234*4321# when normal PIN is 1234
            if (!string.IsNullOrEmpty(callback.UssdSequence))
            {
                // Extract PIN from USSD sequence
                var pinMatch = System.Text.RegularExpressions.Regex.Match(
                    callback.UssdSequence, @"\*(\d{4,6})#");
                
                if (pinMatch.Success)
                {
                    var enteredPin = pinMatch.Groups[1].Value;
                    // Duress detection happens server-side via reversed PIN check
                    return false; // Let server handle it
                }
            }

            // Check for suspicious timing patterns
            // Multiple failed STK attempts in short period
            if (callback.AttemptCount > 3 && 
                callback.TimeSinceFirstAttempt < TimeSpan.FromMinutes(5))
            {
                return true; // Potential duress indicator
            }

            return false;
        }

        /// <summary>
        /// Format shadow mode response for STK Push
        /// Returns mock success message that appears normal
        /// </summary>
        public string FormatShadowModeResponse(decimal amount, string recipient)
        {
            return $"Payment of KES {amount:N2} to {recipient} successful. " +
                   $"Transaction ID: WKZ{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}. " +
                   $"New balance: KES {new Random().Next(100, 500):N2}";
        }

        private string HashPin(string pin)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
            return Convert.ToBase64String(bytes);
        }
    }

    /// <summary>
    /// STK Push callback data from M-Pesa
    /// </summary>
    public class StkCallbackData
    {
        public string TransactionRef { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UssdSequence { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public TimeSpan TimeSinceFirstAttempt { get; set; }
        public decimal Amount { get; set; }
        public string ResultCode { get; set; } = string.Empty;
    }
}
