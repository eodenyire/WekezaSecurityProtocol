using System;
using System.Collections.Generic;

namespace WekezaSecurityProtocol.Models
{
    /// <summary>
    /// Customer segment types for multi-segment banking support
    /// </summary>
    public enum CustomerSegment
    {
        Personal,       // Individual retail customers
        SME,           // Small & Medium Enterprises (1-250 employees)
        Corporate,     // Large corporations (250+ employees)
        PublicSector   // Government entities and agencies
    }

    /// <summary>
    /// Account type associated with each customer segment
    /// </summary>
    public enum AccountType
    {
        // Personal Banking
        SavingsAccount,
        CurrentAccount,
        FixedDeposit,
        CreditCard,
        PersonalLoan,
        MortgageLoan,

        // SME Banking
        BusinessAccount,
        MerchantAccount,
        BusinessLoan,
        Overdraft,
        TradeFinance,

        // Corporate Banking
        CorporateAccount,
        TreasuryAccount,
        MultiCurrencyAccount,
        VirtualAccount,
        CorporateLoan,
        SyndicatedLoan,

        // Public Sector Banking
        GovernmentAccount,
        AgencyAccount,
        RevenueAccount,
        BudgetAccount,
        PensionAccount
    }

    /// <summary>
    /// Transaction limits and permissions per customer segment
    /// </summary>
    public class SegmentLimits
    {
        public CustomerSegment Segment { get; set; }
        public decimal SingleTransactionLimit { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal MonthlyLimit { get; set; }
        public int MaxUsers { get; set; }
        public int ApprovalLevels { get; set; }
        public bool RequiresMultipleApprovals { get; set; }
        public bool SupportsMultiCurrency { get; set; }
        public bool SupportsBulkPayments { get; set; }
        public bool RequiresEnhancedAudit { get; set; }

        public static SegmentLimits GetLimits(CustomerSegment segment)
        {
            return segment switch
            {
                CustomerSegment.Personal => new SegmentLimits
                {
                    Segment = CustomerSegment.Personal,
                    SingleTransactionLimit = 250000m,      // 250k KES
                    DailyLimit = 500000m,                  // 500k KES
                    MonthlyLimit = 10000000m,              // 10M KES
                    MaxUsers = 1,
                    ApprovalLevels = 0,
                    RequiresMultipleApprovals = false,
                    SupportsMultiCurrency = false,
                    SupportsBulkPayments = false,
                    RequiresEnhancedAudit = false
                },
                CustomerSegment.SME => new SegmentLimits
                {
                    Segment = CustomerSegment.SME,
                    SingleTransactionLimit = 1000000m,     // 1M KES
                    DailyLimit = 5000000m,                 // 5M KES
                    MonthlyLimit = 100000000m,             // 100M KES
                    MaxUsers = 5,
                    ApprovalLevels = 2,
                    RequiresMultipleApprovals = true,
                    SupportsMultiCurrency = true,
                    SupportsBulkPayments = true,
                    RequiresEnhancedAudit = true
                },
                CustomerSegment.Corporate => new SegmentLimits
                {
                    Segment = CustomerSegment.Corporate,
                    SingleTransactionLimit = 10000000m,    // 10M KES
                    DailyLimit = 100000000m,               // 100M KES
                    MonthlyLimit = 1000000000m,            // 1B KES
                    MaxUsers = int.MaxValue,               // Unlimited
                    ApprovalLevels = 4,
                    RequiresMultipleApprovals = true,
                    SupportsMultiCurrency = true,
                    SupportsBulkPayments = true,
                    RequiresEnhancedAudit = true
                },
                CustomerSegment.PublicSector => new SegmentLimits
                {
                    Segment = CustomerSegment.PublicSector,
                    SingleTransactionLimit = decimal.MaxValue,  // Unlimited
                    DailyLimit = decimal.MaxValue,             // Unlimited
                    MonthlyLimit = decimal.MaxValue,           // Unlimited
                    MaxUsers = 20,
                    ApprovalLevels = 4,
                    RequiresMultipleApprovals = true,
                    SupportsMultiCurrency = true,
                    SupportsBulkPayments = true,
                    RequiresEnhancedAudit = true
                },
                _ => throw new ArgumentException($"Unknown segment: {segment}")
            };
        }
    }

    /// <summary>
    /// Feature permissions per customer segment
    /// </summary>
    public class SegmentFeatures
    {
        public CustomerSegment Segment { get; set; }
        public List<string> AvailableFeatures { get; set; }
        public List<string> AccountTypes { get; set; }
        public List<string> PaymentMethods { get; set; }
        public List<string> Integrations { get; set; }

        public static SegmentFeatures GetFeatures(CustomerSegment segment)
        {
            return segment switch
            {
                CustomerSegment.Personal => new SegmentFeatures
                {
                    Segment = CustomerSegment.Personal,
                    AvailableFeatures = new List<string>
                    {
                        "Savings Accounts", "Current Accounts", "Fixed Deposits",
                        "Personal Loans", "Mortgage Loans", "Credit Cards",
                        "Mobile Money Integration", "Bill Payments",
                        "Fund Transfers (Local)", "Standing Orders",
                        "Investment Products", "Budget Tracking",
                        "Savings Goals", "Loan Calculator",
                        "ATM/Branch Locator", "Card Controls",
                        "Statement Download", "Tax Certificates"
                    },
                    AccountTypes = new List<string>
                    {
                        "Savings", "Current", "Fixed Deposit"
                    },
                    PaymentMethods = new List<string>
                    {
                        "M-Pesa", "Airtel Money", "Bank Transfer", "Card Payment"
                    },
                    Integrations = new List<string>
                    {
                        "M-Pesa", "Airtel Money", "KPLC", "DSTV", "Water Utility"
                    }
                },
                CustomerSegment.SME => new SegmentFeatures
                {
                    Segment = CustomerSegment.SME,
                    AvailableFeatures = new List<string>
                    {
                        "Business Accounts", "Merchant Payments",
                        "Bulk Payroll (1000 employees)", "Trade Finance",
                        "Business Loans", "Overdraft Facilities",
                        "Cash Management", "Multi-User Access (5 users)",
                        "Role-Based Permissions", "Accounting Integration",
                        "Invoice Management", "Expense Tracking",
                        "Petty Cash Management", "Supplier Payments",
                        "POS Integration", "E-Commerce Gateway",
                        "Business Credit Cards", "Cash Advance",
                        "Working Capital Loans", "Asset Financing",
                        "Business Reports", "Tax Filing Integration"
                    },
                    AccountTypes = new List<string>
                    {
                        "Business Current", "Merchant Account", "USD Account"
                    },
                    PaymentMethods = new List<string>
                    {
                        "M-Pesa", "Bank Transfer", "Card Payment", "POS", "Bulk STK"
                    },
                    Integrations = new List<string>
                    {
                        "QuickBooks", "Xero", "Sage", "KRA iTax",
                        "M-Pesa Till", "PesaLink", "EFT"
                    }
                },
                CustomerSegment.Corporate => new SegmentFeatures
                {
                    Segment = CustomerSegment.Corporate,
                    AvailableFeatures = new List<string>
                    {
                        "Treasury Management", "FX Trading (30+ currencies)",
                        "Bulk Payments (Unlimited)", "Multi-Level Approvals (4 levels)",
                        "Cash Pooling", "Virtual Accounts",
                        "Letter of Credit", "Bank Guarantees",
                        "SWIFT Transfers", "Investment Banking",
                        "Corporate Loans", "Syndicated Financing",
                        "Project Financing", "Multi-Currency Accounts",
                        "Hedging Instruments", "Interest Rate Swaps",
                        "Liquidity Management", "Working Capital Facilities",
                        "Supply Chain Finance", "Advanced Trade Finance",
                        "Custodial Services", "ERP Integration",
                        "API Banking", "Multi-Entity Consolidation",
                        "Group Reporting", "Compliance Dashboards"
                    },
                    AccountTypes = new List<string>
                    {
                        "Corporate Current", "Treasury Account", "Multi-Currency",
                        "Virtual Account", "Escrow Account"
                    },
                    PaymentMethods = new List<string>
                    {
                        "SWIFT", "RTGS", "EFT", "Bulk Payments", "Wire Transfer"
                    },
                    Integrations = new List<string>
                    {
                        "SAP", "Oracle Financials", "Microsoft Dynamics",
                        "SWIFT Network", "Bloomberg Terminal", "FX Platforms"
                    }
                },
                CustomerSegment.PublicSector => new SegmentFeatures
                {
                    Segment = CustomerSegment.PublicSector,
                    AvailableFeatures = new List<string>
                    {
                        "Government Payments", "Tax Collection (KRA)",
                        "Pension Disbursements", "Salary Payments (Civil Servants)",
                        "Procurement Payments (IFMIS)", "Multi-Agency Access",
                        "Enhanced Audit Trail", "Budget Management",
                        "Budget Tracking", "Compliance Reporting (CBK)",
                        "Revenue Collection", "License Fee Payments",
                        "Fine Collections", "Subsidy Disbursements",
                        "Grant Management", "Donor Fund Tracking",
                        "County Government Integration", "National Government Integration",
                        "Parastatals Support", "Multi-Level Approvals (4 levels)",
                        "Regulatory Reporting", "Transparency Portal",
                        "Public Procurement Audit", "E-Government Integration"
                    },
                    AccountTypes = new List<string>
                    {
                        "Government Account", "Agency Account", "Revenue Collection",
                        "Budget Account", "Pension Account"
                    },
                    PaymentMethods = new List<string>
                    {
                        "Government Paybill", "RTGS", "EFT", "Bulk Payments"
                    },
                    Integrations = new List<string>
                    {
                        "IFMIS", "KRA iTax", "NHIF", "NSSF",
                        "e-Citizen", "Government Portal", "County Systems"
                    }
                },
                _ => throw new ArgumentException($"Unknown segment: {segment}")
            };
        }
    }

    /// <summary>
    /// Approval workflow configuration per segment
    /// </summary>
    public class ApprovalWorkflow
    {
        public CustomerSegment Segment { get; set; }
        public List<ApprovalLevel> Levels { get; set; }

        public static ApprovalWorkflow GetWorkflow(CustomerSegment segment, decimal amount)
        {
            return segment switch
            {
                CustomerSegment.Personal => new ApprovalWorkflow
                {
                    Segment = CustomerSegment.Personal,
                    Levels = new List<ApprovalLevel>() // No approvals needed
                },
                CustomerSegment.SME => GetSMEWorkflow(amount),
                CustomerSegment.Corporate => GetCorporateWorkflow(amount),
                CustomerSegment.PublicSector => GetPublicSectorWorkflow(),
                _ => throw new ArgumentException($"Unknown segment: {segment}")
            };
        }

        private static ApprovalWorkflow GetSMEWorkflow(decimal amount)
        {
            var levels = new List<ApprovalLevel>();
            
            if (amount >= 500000m && amount < 1000000m)
            {
                levels.Add(new ApprovalLevel { Level = 1, Role = "Manager", Required = true });
            }
            else if (amount >= 1000000m)
            {
                levels.Add(new ApprovalLevel { Level = 1, Role = "Manager", Required = true });
                levels.Add(new ApprovalLevel { Level = 2, Role = "Director", Required = true });
            }

            return new ApprovalWorkflow { Segment = CustomerSegment.SME, Levels = levels };
        }

        private static ApprovalWorkflow GetCorporateWorkflow(decimal amount)
        {
            var levels = new List<ApprovalLevel>();
            
            if (amount >= 1000000m && amount < 5000000m)
            {
                levels.Add(new ApprovalLevel { Level = 1, Role = "Manager", Required = true });
            }
            else if (amount >= 5000000m && amount < 10000000m)
            {
                levels.Add(new ApprovalLevel { Level = 1, Role = "Manager", Required = true });
                levels.Add(new ApprovalLevel { Level = 2, Role = "Director", Required = true });
            }
            else if (amount >= 10000000m && amount < 50000000m)
            {
                levels.Add(new ApprovalLevel { Level = 1, Role = "Manager", Required = true });
                levels.Add(new ApprovalLevel { Level = 2, Role = "Director", Required = true });
                levels.Add(new ApprovalLevel { Level = 3, Role = "CFO", Required = true });
            }
            else if (amount >= 50000000m)
            {
                levels.Add(new ApprovalLevel { Level = 1, Role = "Manager", Required = true });
                levels.Add(new ApprovalLevel { Level = 2, Role = "Director", Required = true });
                levels.Add(new ApprovalLevel { Level = 3, Role = "CFO", Required = true });
                levels.Add(new ApprovalLevel { Level = 4, Role = "CEO", Required = true });
            }

            return new ApprovalWorkflow { Segment = CustomerSegment.Corporate, Levels = levels };
        }

        private static ApprovalWorkflow GetPublicSectorWorkflow()
        {
            // Public sector always requires 4-level approval
            return new ApprovalWorkflow
            {
                Segment = CustomerSegment.PublicSector,
                Levels = new List<ApprovalLevel>
                {
                    new ApprovalLevel { Level = 1, Role = "Initiator", Required = true },
                    new ApprovalLevel { Level = 2, Role = "Reviewer", Required = true },
                    new ApprovalLevel { Level = 3, Role = "Approver", Required = true },
                    new ApprovalLevel { Level = 4, Role = "Authorizer", Required = true }
                }
            };
        }
    }

    /// <summary>
    /// Approval level configuration
    /// </summary>
    public class ApprovalLevel
    {
        public int Level { get; set; }
        public string Role { get; set; }
        public bool Required { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
