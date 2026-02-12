using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Services
{
    /// <summary>
    /// Shadow service provides mock banking data for duress mode
    /// All data is generated, never written to production ledger
    /// </summary>
    public class ShadowBankingService
    {
        private readonly Random _random = new();

        /// <summary>
        /// Get mock account balance (always low to appear realistic)
        /// </summary>
        public Task<BalanceResponse> GetBalanceAsync(string accountId)
        {
            // Generate low balance between 100 and 500 with random cents
            var balance = _random.Next(100, 500) + Math.Round(_random.NextDouble(), 2);

            return Task.FromResult(new BalanceResponse
            {
                AccountId = accountId,
                Balance = balance,
                Currency = "KES",
                AvailableBalance = balance,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Generate mock transaction history
        /// Filters out high-value transactions, adds realistic utility payments
        /// </summary>
        public Task<TransactionHistoryResponse> GetTransactionHistoryAsync(
            string accountId, 
            DateTime fromDate, 
            DateTime toDate,
            int pageSize = 20)
        {
            var transactions = new List<Transaction>();

            // Generate mock transactions (utilities, small purchases)
            var mockTransactions = new[]
            {
                ("KPLC Bill Payment", 1200, "Electricity"),
                ("Nairobi Water", 800, "Utility"),
                ("Safaricom Airtime", 500, "Airtime"),
                ("Equity Bank ATM", 1000, "Withdrawal"),
                ("Carrefour Supermarket", 2500, "Purchase"),
                ("Shell Petrol Station", 1500, "Fuel"),
                ("Naivas Supermarket", 1800, "Shopping"),
                ("KFC Westlands", 650, "Dining")
            };

            // Generate 10-15 transactions
            var count = _random.Next(10, 16);
            for (int i = 0; i < count; i++)
            {
                var (description, baseAmount, category) = mockTransactions[_random.Next(mockTransactions.Length)];
                var amount = baseAmount + _random.Next(-200, 200);
                var daysAgo = _random.Next(1, 30);

                transactions.Add(new Transaction
                {
                    TransactionId = GenerateMockTransactionId(),
                    AccountId = accountId,
                    Amount = amount,
                    TransactionType = "Debit",
                    Description = description,
                    Category = category,
                    Timestamp = DateTime.UtcNow.AddDays(-daysAgo),
                    Status = "Completed"
                });
            }

            return Task.FromResult(new TransactionHistoryResponse
            {
                AccountId = accountId,
                Transactions = transactions.OrderByDescending(t => t.Timestamp).ToList(),
                TotalCount = transactions.Count,
                FromDate = fromDate,
                ToDate = toDate
            });
        }

        /// <summary>
        /// Process transfer in shadow mode (quarantine, not execute)
        /// Returns success response but doesn't move funds
        /// </summary>
        public async Task<TransferResponse> ProcessTransferAsync(TransferRequest request)
        {
            // Generate mock transaction ID
            var mockTxnId = GenerateMockTransactionId();

            // Log to quarantine table (not shown here, would be in repository)
            await QuarantineTransactionAsync(request, mockTxnId);

            // Return "success" to maintain deception
            return new TransferResponse
            {
                Success = true,
                TransactionId = mockTxnId,
                Message = "Transfer successful",
                Amount = request.Amount,
                RecipientAccount = request.RecipientAccount,
                Timestamp = DateTime.UtcNow,
                NewBalance = await GetRandomLowBalance()
            };
        }

        /// <summary>
        /// Quarantine transaction - logs to duress_quarantine table, not ledger
        /// </summary>
        private Task QuarantineTransactionAsync(TransferRequest request, string mockTxnId)
        {
            // In real implementation, this would write to database
            // with is_duress = true flag for compliance reporting
            var quarantineEntry = new QuarantineTransaction
            {
                MockTransactionId = mockTxnId,
                FromAccountId = request.FromAccountId,
                ToAccountId = request.RecipientAccount,
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                IsDuress = true,
                Timestamp = DateTime.UtcNow,
                Status = "Quarantined"
            };

            // TODO: Write to database via repository
            return Task.CompletedTask;
        }

        /// <summary>
        /// Generate realistic-looking transaction ID
        /// </summary>
        private string GenerateMockTransactionId()
        {
            var prefix = "WKZ";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = _random.Next(1000, 9999);
            return $"{prefix}{timestamp}{random}";
        }

        /// <summary>
        /// Get random low balance for post-transaction display
        /// </summary>
        private Task<double> GetRandomLowBalance()
        {
            var balance = _random.Next(50, 400) + Math.Round(_random.NextDouble(), 2);
            return Task.FromResult(balance);
        }
    }

    #region Models

    /// <summary>
    /// Balance response model
    /// </summary>
    public class BalanceResponse
    {
        public string AccountId { get; set; } = string.Empty;
        public double Balance { get; set; }
        public double AvailableBalance { get; set; }
        public string Currency { get; set; } = "KES";
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Transaction history response
    /// </summary>
    public class TransactionHistoryResponse
    {
        public string AccountId { get; set; } = string.Empty;
        public List<Transaction> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    /// <summary>
    /// Transaction model
    /// </summary>
    public class Transaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Transfer request model
    /// </summary>
    public class TransferRequest
    {
        public string FromAccountId { get; set; } = string.Empty;
        public string RecipientAccount { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Currency { get; set; } = "KES";
        public string Description { get; set; } = string.Empty;
        public string TransferType { get; set; } = "MobileMoney"; // MobileMoney, Internal, Bank
    }

    /// <summary>
    /// Transfer response model
    /// </summary>
    public class TransferResponse
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string RecipientAccount { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double NewBalance { get; set; }
    }

    /// <summary>
    /// Quarantine transaction model for database storage
    /// </summary>
    public class QuarantineTransaction
    {
        public string MockTransactionId { get; set; } = string.Empty;
        public string FromAccountId { get; set; } = string.Empty;
        public string ToAccountId { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Currency { get; set; } = "KES";
        public string Description { get; set; } = string.Empty;
        public bool IsDuress { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "Quarantined";
    }

    #endregion
}
