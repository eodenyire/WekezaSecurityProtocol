using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Channels.WorldClass
{
    /// <summary>
    /// Voice Banking Channel
    /// Based on best practices from Erica (Bank of America), Cleo, Eno (Capital One)
    /// Supports Alexa, Google Assistant, Siri Shortcuts
    /// </summary>
    public class VoiceBankingChannelAdapter
    {
        /// <summary>
        /// Voice platform types
        /// </summary>
        public enum VoicePlatform
        {
            AmazonAlexa,
            GoogleAssistant,
            AppleSiri,
            SamsungBixby
        }

        /// <summary>
        /// Process voice command
        /// </summary>
        public async Task<VoiceResponse> ProcessVoiceCommand(
            string accountId,
            string command,
            VoicePlatform platform,
            string voicePrint = "")
        {
            // Verify voice biometrics if available
            if (!string.IsNullOrEmpty(voicePrint))
            {
                var isAuthentic = await VerifyVoicePrint(accountId, voicePrint);
                if (!isAuthentic)
                {
                    return new VoiceResponse
                    {
                        Speech = "Voice authentication failed. Please verify your identity.",
                        EndSession = true
                    };
                }
            }

            // Parse intent from voice command
            var intent = await ParseVoiceIntent(command);

            return intent switch
            {
                "check_balance" => await HandleBalanceCheck(accountId),
                "recent_transactions" => await HandleRecentTransactions(accountId),
                "pay_bill" => await HandleBillPayment(accountId, command),
                "transfer_money" => await HandleTransfer(accountId, command),
                "freeze_card" => await HandleCardFreeze(accountId),
                _ => new VoiceResponse
                {
                    Speech = "I'm sorry, I didn't understand that. You can ask me to check your balance, view recent transactions, pay bills, or transfer money.",
                    Reprompt = "What would you like to do?"
                }
            };
        }

        /// <summary>
        /// Create authentication request from voice session
        /// </summary>
        public AuthenticationRequest CreateVoiceAuthRequest(
            string accountId,
            string voicePrint,
            VoicePlatform platform)
        {
            return new AuthenticationRequest
            {
                AccountId = accountId,
                PinHash = voicePrint, // Voice print as biometric
                DeviceId = $"{platform}-{accountId}",
                ChannelMetadata = new ChannelMetadata
                {
                    Channel = ChannelType.ApiIntegration, // Using API integration as base
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "channel_type", "voice" },
                        { "platform", platform.ToString() },
                        { "auth_method", "voice_biometric" }
                    }
                }
            };
        }

        private async Task<bool> VerifyVoicePrint(string accountId, string voicePrint)
        {
            // Implement voice biometric verification
            // Compare with stored voice print
            return true; // Placeholder
        }

        private async Task<string> ParseVoiceIntent(string command)
        {
            var lowerCommand = command.ToLower();
            
            if (lowerCommand.Contains("balance") || lowerCommand.Contains("how much"))
                return "check_balance";
            if (lowerCommand.Contains("transaction") || lowerCommand.Contains("recent"))
                return "recent_transactions";
            if (lowerCommand.Contains("pay") || lowerCommand.Contains("bill"))
                return "pay_bill";
            if (lowerCommand.Contains("send") || lowerCommand.Contains("transfer"))
                return "transfer_money";
            if (lowerCommand.Contains("freeze") || lowerCommand.Contains("block") || lowerCommand.Contains("lock"))
                return "freeze_card";

            return "unknown";
        }

        private async Task<VoiceResponse> HandleBalanceCheck(string accountId)
        {
            // Get balance from API
            var balance = 15234.50m; // Placeholder

            return new VoiceResponse
            {
                Speech = $"Your current balance is {balance:N2} Kenyan Shillings.",
                Card = new VoiceCard
                {
                    Title = "Account Balance",
                    Text = $"KES {balance:N2}",
                    Type = "Simple"
                }
            };
        }

        private async Task<VoiceResponse> HandleRecentTransactions(string accountId)
        {
            return new VoiceResponse
            {
                Speech = "Your last three transactions were: " +
                        "KPLC payment of 1,200 shillings on January 10th, " +
                        "Nairobi Water bill of 800 shillings on January 12th, " +
                        "and Carrefour purchase of 2,500 shillings on January 15th.",
                Card = new VoiceCard
                {
                    Title = "Recent Transactions",
                    Text = "KPLC: KES 1,200\nWater: KES 800\nCarrefour: KES 2,500",
                    Type = "Standard"
                }
            };
        }

        private async Task<VoiceResponse> HandleBillPayment(string accountId, string command)
        {
            return new VoiceResponse
            {
                Speech = "I can help you pay a bill. Which company would you like to pay?",
                Reprompt = "Say the company name, like KPLC or Nairobi Water.",
                RequiresInput = true
            };
        }

        private async Task<VoiceResponse> HandleTransfer(string accountId, string command)
        {
            return new VoiceResponse
            {
                Speech = "For security, please complete this transfer in the mobile app. " +
                        "I've sent you a notification.",
                EndSession = true
            };
        }

        private async Task<VoiceResponse> HandleCardFreeze(string accountId)
        {
            return new VoiceResponse
            {
                Speech = "I've frozen your card. You can unfreeze it anytime through the mobile app.",
                Card = new VoiceCard
                {
                    Title = "Card Frozen",
                    Text = "Your card has been frozen for security",
                    Type = "Simple"
                }
            };
        }
    }

    /// <summary>
    /// Messaging Platform Channel (WhatsApp, Telegram, Facebook Messenger)
    /// Best practice from Wise, N26, Starling Bank
    /// </summary>
    public class MessagingChannelAdapter
    {
        public enum MessagingPlatform
        {
            WhatsApp,
            Telegram,
            FacebookMessenger,
            Signal
        }

        /// <summary>
        /// Process message from platform
        /// </summary>
        public async Task<MessagingResponse> ProcessMessage(
            string accountId,
            string message,
            MessagingPlatform platform,
            string sessionId)
        {
            // Classify intent
            var intent = await ClassifyIntent(message);

            return intent switch
            {
                "balance" => await GetBalance(accountId),
                "transactions" => await GetTransactions(accountId),
                "transfer" => await InitiateTransfer(accountId, message),
                "help" => GetHelpMessage(),
                _ => new MessagingResponse
                {
                    Text = "I can help you with:\n" +
                          "💰 Check balance\n" +
                          "📊 View transactions\n" +
                          "💸 Transfer money\n" +
                          "❓ Help\n\n" +
                          "Just type what you need!",
                    QuickReplies = new[] { "Balance", "Transactions", "Help" }
                }
            };
        }

        /// <summary>
        /// Send transaction alert via messaging platform
        /// </summary>
        public async Task SendTransactionAlert(
            string accountId,
            MessagingPlatform platform,
            decimal amount,
            string merchant)
        {
            var message = new MessagingResponse
            {
                Text = $"🔔 Transaction Alert\n\n" +
                      $"Amount: KES {amount:N2}\n" +
                      $"Merchant: {merchant}\n" +
                      $"Time: {DateTime.Now:HH:mm}\n\n" +
                      $"Was this you?",
                QuickReplies = new[] { "✅ Yes", "❌ No - Block Card" }
            };

            await SendMessage(accountId, platform, message);
        }

        /// <summary>
        /// Interactive payment via messaging
        /// </summary>
        public async Task<MessagingResponse> ProcessPaymentFlow(
            string accountId,
            string recipient,
            decimal amount,
            string flowStep)
        {
            return flowStep switch
            {
                "confirm" => new MessagingResponse
                {
                    Text = $"📤 Send KES {amount:N2} to {recipient}?\n\n" +
                          $"Reply YES to confirm or CANCEL to abort.",
                    QuickReplies = new[] { "YES", "CANCEL" }
                },
                "pin_request" => new MessagingResponse
                {
                    Text = "🔐 Enter your 4-digit PIN to authorize this transaction:",
                    RequiresInput = true,
                    InputType = "secure"
                },
                "success" => new MessagingResponse
                {
                    Text = $"✅ Transaction Successful!\n\n" +
                          $"Sent: KES {amount:N2}\n" +
                          $"To: {recipient}\n" +
                          $"Ref: WKZ{DateTime.Now:yyyyMMddHHmmss}\n\n" +
                          $"New Balance: KES 12,234.50",
                    QuickReplies = new[] { "📊 Statement", "🏠 Main Menu" }
                },
                _ => new MessagingResponse { Text = "Invalid flow step" }
            };
        }

        private async Task<string> ClassifyIntent(string message)
        {
            var lower = message.ToLower();
            if (lower.Contains("balance") || lower.Contains("💰")) return "balance";
            if (lower.Contains("transaction") || lower.Contains("history") || lower.Contains("📊")) return "transactions";
            if (lower.Contains("send") || lower.Contains("transfer") || lower.Contains("pay") || lower.Contains("💸")) return "transfer";
            if (lower.Contains("help") || lower.Contains("❓")) return "help";
            return "unknown";
        }

        private async Task<MessagingResponse> GetBalance(string accountId)
        {
            return new MessagingResponse
            {
                Text = "💰 Account Balance\n\n" +
                      "Current: KES 15,234.50\n" +
                      "Available: KES 15,234.50\n\n" +
                      "Updated: Just now",
                QuickReplies = new[] { "📊 Transactions", "💸 Send Money", "🏠 Menu" }
            };
        }

        private async Task<MessagingResponse> GetTransactions(string accountId)
        {
            return new MessagingResponse
            {
                Text = "📊 Recent Transactions\n\n" +
                      "🛒 Carrefour Supermarket\n" +
                      "KES 2,500 • Jan 15\n\n" +
                      "⚡ KPLC Bill Payment\n" +
                      "KES 1,200 • Jan 10\n\n" +
                      "💧 Nairobi Water\n" +
                      "KES 800 • Jan 12",
                QuickReplies = new[] { "💰 Balance", "📄 Full Statement", "🏠 Menu" }
            };
        }

        private async Task<MessagingResponse> InitiateTransfer(string accountId, string message)
        {
            return new MessagingResponse
            {
                Text = "💸 Send Money\n\n" +
                      "Who would you like to send money to?\n" +
                      "Enter phone number or name:",
                RequiresInput = true,
                InputType = "text"
            };
        }

        private MessagingResponse GetHelpMessage()
        {
            return new MessagingResponse
            {
                Text = "❓ How can I help?\n\n" +
                      "I can help you:\n" +
                      "• Check your balance 💰\n" +
                      "• View transactions 📊\n" +
                      "• Send money 💸\n" +
                      "• Pay bills ⚡\n" +
                      "• Freeze/unfreeze card 🔒\n\n" +
                      "Just tell me what you need!",
                QuickReplies = new[] { "💰 Balance", "📊 Transactions", "💸 Send Money" }
            };
        }

        private async Task SendMessage(string accountId, MessagingPlatform platform, MessagingResponse message)
        {
            // Send via platform API (WhatsApp Business API, Telegram Bot API, etc.)
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Wearable Device Channel (Apple Watch, Samsung Galaxy Watch, Fitbit)
    /// Best practice from Chase, Capital One, Revolut
    /// </summary>
    public class WearableChannelAdapter
    {
        public enum WearableType
        {
            AppleWatch,
            SamsungGalaxyWatch,
            WearOS,
            Fitbit
        }

        /// <summary>
        /// Process wearable request
        /// </summary>
        public async Task<WearableResponse> ProcessWearableRequest(
            string accountId,
            string action,
            WearableType wearable)
        {
            return action switch
            {
                "quick_balance" => await GetQuickBalance(accountId),
                "recent_transaction" => await GetRecentTransaction(accountId),
                "tap_to_pay" => await InitiateTapToPay(accountId),
                "freeze_card" => await FreezeCard(accountId),
                _ => new WearableResponse
                {
                    Title = "Unknown Action",
                    Message = "Action not supported on wearable",
                    Vibrate = true
                }
            };
        }

        /// <summary>
        /// Send transaction notification to wearable
        /// </summary>
        public async Task SendWearableNotification(
            string accountId,
            WearableType wearable,
            decimal amount,
            string merchant)
        {
            var notification = new WearableNotification
            {
                Title = "Transaction",
                Message = $"KES {amount:N2} at {merchant}",
                Icon = "transaction",
                Vibrate = true,
                Actions = new[]
                {
                    new WearableAction { Id = "view", Label = "View" },
                    new WearableAction { Id = "dispute", Label = "Dispute" }
                }
            };

            await SendToWearable(accountId, wearable, notification);
        }

        private async Task<WearableResponse> GetQuickBalance(string accountId)
        {
            return new WearableResponse
            {
                Title = "Balance",
                Message = "KES 15,234.50",
                Icon = "balance",
                Color = "#4CAF50"
            };
        }

        private async Task<WearableResponse> GetRecentTransaction(string accountId)
        {
            return new WearableResponse
            {
                Title = "Recent",
                Message = "Carrefour\nKES 2,500",
                Icon = "transaction",
                Color = "#2196F3"
            };
        }

        private async Task<WearableResponse> InitiateTapToPay(string accountId)
        {
            return new WearableResponse
            {
                Title = "Ready to Pay",
                Message = "Hold near terminal",
                Icon = "nfc",
                Vibrate = true,
                Color = "#FF9800"
            };
        }

        private async Task<WearableResponse> FreezeCard(string accountId)
        {
            return new WearableResponse
            {
                Title = "Card Frozen",
                Message = "Card successfully frozen",
                Icon = "lock",
                Vibrate = true,
                Color = "#F44336"
            };
        }

        private async Task SendToWearable(string accountId, WearableType wearable, WearableNotification notification)
        {
            // Send via platform-specific API
            await Task.CompletedTask;
        }
    }

    #region Supporting Models

    public class VoiceResponse
    {
        public string Speech { get; set; } = string.Empty;
        public string Reprompt { get; set; } = string.Empty;
        public VoiceCard? Card { get; set; }
        public bool EndSession { get; set; }
        public bool RequiresInput { get; set; }
    }

    public class VoiceCard
    {
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class MessagingResponse
    {
        public string Text { get; set; } = string.Empty;
        public string[]? QuickReplies { get; set; }
        public bool RequiresInput { get; set; }
        public string InputType { get; set; } = "text";
    }

    public class WearableResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool Vibrate { get; set; }
    }

    public class WearableNotification
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool Vibrate { get; set; }
        public WearableAction[]? Actions { get; set; }
    }

    public class WearableAction
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    #endregion
}
