using System;
using System.Collections.Generic;
using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Authentication
{
    /// <summary>
    /// Segment-aware Salama Security Protocol implementation
    /// Provides appropriate shadow mode data based on customer segment
    /// </summary>
    public class SegmentAwareSalamaAuthentication
    {
        private readonly CustomerSegment _segment;

        public SegmentAwareSalamaAuthentication(CustomerSegment segment)
        {
            _segment = segment;
        }

        /// <summary>
        /// Generate segment-appropriate shadow mode data
        /// </summary>
        public ShadowModeData GenerateShadowData(string accountId)
        {
            return _segment switch
            {
                CustomerSegment.Personal => GeneratePersonalShadowData(accountId),
                CustomerSegment.SME => GenerateSMEShadowData(accountId),
                CustomerSegment.Corporate => GenerateCorporateShadowData(accountId),
                CustomerSegment.PublicSector => GeneratePublicSectorShadowData(accountId),
                _ => throw new ArgumentException($"Unknown segment: {_segment}")
            };
        }

        private ShadowModeData GeneratePersonalShadowData(string accountId)
        {
            return new ShadowModeData
            {
                Segment = CustomerSegment.Personal,
                AccountId = accountId,
                Balance = new Random().Next(100, 500),  // Low balance: 100-500 KES
                Accounts = new List<ShadowAccount>
                {
                    new ShadowAccount
                    {
                        AccountNumber = accountId,
                        AccountType = "Savings",
                        Balance = new Random().Next(100, 500),
                        Currency = "KES"
                    }
                },
                RecentTransactions = new List<ShadowTransaction>
                {
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-1),
                        Description = "ATM Withdrawal",
                        Amount = -500,
                        Balance = 450
                    },
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-3),
                        Description = "Salary",
                        Amount = 5000,
                        Balance = 950
                    },
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-5),
                        Description = "Bill Payment - KPLC",
                        Amount = -1500,
                        Balance = -4050
                    }
                },
                Cards = new List<ShadowCard>
                {
                    new ShadowCard
                    {
                        CardNumber = "****1234",
                        CardType = "Debit Card",
                        Status = "Active"
                    }
                },
                Loans = new List<ShadowLoan>(), // No active loans
                Message = "Operating in secure mode"
            };
        }

        private ShadowModeData GenerateSMEShadowData(string accountId)
        {
            return new ShadowModeData
            {
                Segment = CustomerSegment.SME,
                AccountId = accountId,
                Balance = new Random().Next(50000, 200000),  // Moderate: 50k-200k KES
                Accounts = new List<ShadowAccount>
                {
                    new ShadowAccount
                    {
                        AccountNumber = accountId,
                        AccountType = "Business Current",
                        Balance = new Random().Next(50000, 200000),
                        Currency = "KES"
                    }
                },
                RecentTransactions = new List<ShadowTransaction>
                {
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-1),
                        Description = "Payroll Payment",
                        Amount = -45000,
                        Balance = 120000
                    },
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-5),
                        Description = "Customer Payment",
                        Amount = 80000,
                        Balance = 165000
                    },
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-7),
                        Description = "Supplier Payment",
                        Amount = -25000,
                        Balance = 85000
                    }
                },
                PendingApprovals = new List<ShadowApproval>(), // No pending approvals
                Users = new List<ShadowUser>
                {
                    new ShadowUser
                    {
                        Name = "Account Owner",
                        Role = "Admin",
                        Status = "Active"
                    }
                },
                Message = "Business account - secure mode"
            };
        }

        private ShadowModeData GenerateCorporateShadowData(string accountId)
        {
            return new ShadowModeData
            {
                Segment = CustomerSegment.Corporate,
                AccountId = accountId,
                Balance = new Random().Next(500000, 2000000),  // Higher: 500k-2M KES
                Accounts = new List<ShadowAccount>
                {
                    new ShadowAccount
                    {
                        AccountNumber = accountId,
                        AccountType = "Corporate Current",
                        Balance = new Random().Next(500000, 2000000),
                        Currency = "KES"
                    },
                    new ShadowAccount
                    {
                        AccountNumber = accountId + "-USD",
                        AccountType = "USD Account",
                        Balance = new Random().Next(10000, 50000),
                        Currency = "USD"
                    }
                },
                RecentTransactions = new List<ShadowTransaction>
                {
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-1),
                        Description = "FX Trade - USD/KES",
                        Amount = -1500000,
                        Balance = 1200000
                    },
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-3),
                        Description = "Bulk Payment - Suppliers",
                        Amount = -850000,
                        Balance = 2700000
                    },
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-5),
                        Description = "Trade Finance Settlement",
                        Amount = 2000000,
                        Balance = 3550000
                    }
                },
                PendingApprovals = new List<ShadowApproval>(), // All pre-approved in shadow
                TreasuryPosition = new ShadowTreasuryPosition
                {
                    KESPosition = 1500000,
                    USDPosition = 30000,
                    EURPosition = 0,
                    TotalInKES = 1500000
                },
                Message = "Corporate treasury - secure mode"
            };
        }

        private ShadowModeData GeneratePublicSectorShadowData(string accountId)
        {
            return new ShadowModeData
            {
                Segment = CustomerSegment.PublicSector,
                AccountId = accountId,
                Balance = new Random().Next(5000000, 20000000),  // Budget allocation
                Accounts = new List<ShadowAccount>
                {
                    new ShadowAccount
                    {
                        AccountNumber = accountId,
                        AccountType = "Government Account",
                        Balance = new Random().Next(5000000, 20000000),
                        Currency = "KES"
                    }
                },
                RecentTransactions = new List<ShadowTransaction>
                {
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-1),
                        Description = "Salary Payment - Civil Servants",
                        Amount = -3500000,
                        Balance = 12000000
                    },
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-10),
                        Description = "Pension Disbursement",
                        Amount = -2000000,
                        Balance = 15500000
                    },
                    new ShadowTransaction
                    {
                        Date = DateTime.UtcNow.AddDays(-15),
                        Description = "Budget Allocation",
                        Amount = 10000000,
                        Balance = 17500000
                    }
                },
                BudgetStatus = new ShadowBudgetStatus
                {
                    TotalBudget = 50000000,
                    SpentToDate = 30000000,
                    RemainingBalance = 20000000,
                    UtilizationPercent = 60
                },
                PendingApprovals = new List<ShadowApproval>(), // No pending in shadow
                Message = "Government account - secure mode"
            };
        }

        /// <summary>
        /// Create SOC alert with segment context
        /// </summary>
        public SOCAlert CreateSegmentAwareAlert(string accountId, DuressContext context)
        {
            return new SOCAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Priority = "HIGH",
                Segment = _segment,
                AccountId = accountId,
                DuressType = context.DuressType,
                Location = context.Location,
                DeviceInfo = context.DeviceInfo,
                Message = $"DURESS DETECTED - {_segment} Banking",
                RecommendedAction = GetSegmentRecommendedAction(),
                NotificationChannels = new List<string> { "SMS", "Email", "Dashboard", "Mobile Push" }
            };
        }

        private string GetSegmentRecommendedAction()
        {
            return _segment switch
            {
                CustomerSegment.Personal => "Monitor account for 6 hours. Contact customer via secure channel.",
                CustomerSegment.SME => "Monitor business account. Contact all authorized users. Review pending transactions.",
                CustomerSegment.Corporate => "Immediate treasury review. Contact CFO/CEO. Freeze high-value transactions.",
                CustomerSegment.PublicSector => "Alert compliance team. Notify government security. Enhanced audit mode.",
                _ => "Standard duress protocol"
            };
        }

        /// <summary>
        /// Quarantine transaction with segment awareness
        /// </summary>
        public QuarantinedTransaction QuarantineTransaction(Transaction transaction)
        {
            return new QuarantinedTransaction
            {
                TransactionId = transaction.TransactionId,
                Segment = _segment,
                Amount = transaction.Amount,
                Recipient = transaction.Recipient,
                QuarantinedAt = DateTime.UtcNow,
                Reason = "Duress Mode Active",
                RequiresReview = true,
                ReviewPriority = GetSegmentPriority(),
                CanBeReleased = false, // Never released from quarantine
                NotificationSent = true
            };
        }

        private string GetSegmentPriority()
        {
            return _segment switch
            {
                CustomerSegment.Personal => "Medium",
                CustomerSegment.SME => "High",
                CustomerSegment.Corporate => "Critical",
                CustomerSegment.PublicSector => "Critical",
                _ => "Medium"
            };
        }
    }

    public class ShadowModeData
    {
        public CustomerSegment Segment { get; set; }
        public string AccountId { get; set; }
        public decimal Balance { get; set; }
        public List<ShadowAccount> Accounts { get; set; }
        public List<ShadowTransaction> RecentTransactions { get; set; }
        public List<ShadowCard> Cards { get; set; }
        public List<ShadowLoan> Loans { get; set; }
        public List<ShadowApproval> PendingApprovals { get; set; }
        public List<ShadowUser> Users { get; set; }
        public ShadowTreasuryPosition TreasuryPosition { get; set; }
        public ShadowBudgetStatus BudgetStatus { get; set; }
        public string Message { get; set; }
    }

    public class ShadowAccount
    {
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
    }

    public class ShadowTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }

    public class ShadowCard
    {
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public string Status { get; set; }
    }

    public class ShadowLoan
    {
        public string LoanType { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal MonthlyPayment { get; set; }
    }

    public class ShadowApproval
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }

    public class ShadowUser
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }

    public class ShadowTreasuryPosition
    {
        public decimal KESPosition { get; set; }
        public decimal USDPosition { get; set; }
        public decimal EURPosition { get; set; }
        public decimal TotalInKES { get; set; }
    }

    public class ShadowBudgetStatus
    {
        public decimal TotalBudget { get; set; }
        public decimal SpentToDate { get; set; }
        public decimal RemainingBalance { get; set; }
        public int UtilizationPercent { get; set; }
    }

    public class SOCAlert
    {
        public string AlertId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Priority { get; set; }
        public CustomerSegment Segment { get; set; }
        public string AccountId { get; set; }
        public string DuressType { get; set; }
        public GeoLocation Location { get; set; }
        public string DeviceInfo { get; set; }
        public string Message { get; set; }
        public string RecommendedAction { get; set; }
        public List<string> NotificationChannels { get; set; }
    }

    public class DuressContext
    {
        public string DuressType { get; set; }
        public GeoLocation Location { get; set; }
        public string DeviceInfo { get; set; }
    }

    public class GeoLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }

    public class Transaction
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Recipient { get; set; }
    }

    public class QuarantinedTransaction
    {
        public string TransactionId { get; set; }
        public CustomerSegment Segment { get; set; }
        public decimal Amount { get; set; }
        public string Recipient { get; set; }
        public DateTime QuarantinedAt { get; set; }
        public string Reason { get; set; }
        public bool RequiresReview { get; set; }
        public string ReviewPriority { get; set; }
        public bool CanBeReleased { get; set; }
        public bool NotificationSent { get; set; }
    }
}
