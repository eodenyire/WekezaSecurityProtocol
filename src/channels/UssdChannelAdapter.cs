using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Channels
{
    /// <summary>
    /// USSD (*234#) channel adapter for Salama Protocol
    /// Handles menu-based authentication and transactions for feature phones
    /// </summary>
    public class UssdChannelAdapter
    {
        /// <summary>
        /// Create authentication request from USSD session
        /// </summary>
        public AuthenticationRequest CreateAuthRequest(
            string phoneNumber,
            string pinCode,
            string ussdSessionId,
            string ussdCode)
        {
            return new AuthenticationRequest
            {
                AccountId = phoneNumber,
                PinHash = HashPin(pinCode),
                DeviceId = $"USSD-{phoneNumber}",
                ChannelMetadata = new ChannelMetadata
                {
                    Channel = ChannelType.Ussd,
                    SessionId = ussdSessionId,
                    UssdCode = ussdCode,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "ussd_session_id", ussdSessionId },
                        { "network_operator", DetectNetworkOperator(phoneNumber) }
                    }
                }
            };
        }

        /// <summary>
        /// Detect duress from USSD code pattern
        /// For USSD, duress is primarily detected via reversed PIN
        /// </summary>
        public bool DetectUssdDuress(string ussdCode, string normalPin)
        {
            // Extract PIN from USSD code (e.g., *234*1234#)
            var pinMatch = System.Text.RegularExpressions.Regex.Match(
                ussdCode, @"\*(\d{4,6})#");
            
            if (pinMatch.Success)
            {
                var enteredPin = pinMatch.Groups[1].Value;
                var reversedPin = new string(normalPin.Reverse().ToArray());
                
                return enteredPin == reversedPin;
            }

            return false;
        }

        /// <summary>
        /// Generate USSD menu response for standard mode
        /// </summary>
        public string GenerateStandardMenu(string accountId, decimal balance)
        {
            return $"Welcome to Wekeza Bank\n" +
                   $"Balance: KES {balance:N2}\n" +
                   $"1. Send Money\n" +
                   $"2. Check Balance\n" +
                   $"3. Mini Statement\n" +
                   $"4. My Account\n" +
                   $"0. Exit";
        }

        /// <summary>
        /// Generate USSD menu response for shadow mode
        /// Identical format to maintain deception
        /// </summary>
        public string GenerateShadowMenu(string accountId)
        {
            var mockBalance = new Random().Next(100, 500);
            return $"Welcome to Wekeza Bank\n" +
                   $"Balance: KES {mockBalance:N2}\n" +
                   $"1. Send Money\n" +
                   $"2. Check Balance\n" +
                   $"3. Mini Statement\n" +
                   $"4. My Account\n" +
                   $"0. Exit";
        }

        /// <summary>
        /// Format shadow mode transaction response
        /// </summary>
        public string FormatShadowTransactionResponse(decimal amount, string recipient)
        {
            var mockTxnId = $"WKZ{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
            var mockBalance = new Random().Next(50, 400);

            return $"Transaction Successful\n" +
                   $"Sent KES {amount:N2}\n" +
                   $"To: {recipient}\n" +
                   $"Ref: {mockTxnId}\n" +
                   $"New Balance: KES {mockBalance:N2}";
        }

        /// <summary>
        /// Format shadow mode mini statement
        /// Shows realistic low-value transactions
        /// </summary>
        public string FormatShadowMiniStatement()
        {
            var transactions = new[]
            {
                "KPLC Payment -1,200",
                "Water Bill -800",
                "Airtime -500",
                "Supermarket -2,100"
            };

            var result = "Mini Statement\n";
            result += "Last 4 Transactions:\n";
            
            foreach (var txn in transactions.Take(4))
            {
                result += $"{txn}\n";
            }

            return result;
        }

        /// <summary>
        /// Handle USSD session state for multi-step flows
        /// </summary>
        public UssdSessionState ManageSession(
            string sessionId, 
            string input, 
            UssdSessionState? currentState)
        {
            // State machine for USSD menu navigation
            return currentState?.Step switch
            {
                null or 0 => new UssdSessionState 
                { 
                    SessionId = sessionId, 
                    Step = 1, 
                    MenuLevel = "Main",
                    IsActive = true
                },
                1 when input == "1" => new UssdSessionState 
                { 
                    SessionId = sessionId, 
                    Step = 2, 
                    MenuLevel = "SendMoney",
                    IsActive = true
                },
                _ => currentState ?? new UssdSessionState 
                { 
                    SessionId = sessionId, 
                    IsActive = false 
                }
            };
        }

        private string HashPin(string pin)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
            return Convert.ToBase64String(bytes);
        }

        private string DetectNetworkOperator(string phoneNumber)
        {
            // Kenyan mobile network prefixes
            if (phoneNumber.StartsWith("254722") || phoneNumber.StartsWith("254733"))
                return "Safaricom";
            if (phoneNumber.StartsWith("254710") || phoneNumber.StartsWith("254711"))
                return "Airtel";
            if (phoneNumber.StartsWith("254777"))
                return "Telkom";
            
            return "Unknown";
        }
    }

    /// <summary>
    /// USSD session state for multi-step flows
    /// </summary>
    public class UssdSessionState
    {
        public string SessionId { get; set; } = string.Empty;
        public int Step { get; set; }
        public string MenuLevel { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> SessionData { get; set; } = new();
    }
}
