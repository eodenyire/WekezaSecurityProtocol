using System;
using System.Collections.Generic;
using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Channels.SegmentSupport
{
    /// <summary>
    /// Mobile channel support for all customer segments with segment-specific features
    /// Based on best practices from Chase, DBS, Revolut, N26
    /// </summary>
    public class MobileChannelSegmentSupport
    {
        private readonly CustomerSegment _segment;
        private readonly SegmentLimits _limits;
        private readonly SegmentFeatures _features;

        public MobileChannelSegmentSupport(CustomerSegment segment)
        {
            _segment = segment;
            _limits = SegmentLimits.GetLimits(segment);
            _features = SegmentFeatures.GetFeatures(segment);
        }

        /// <summary>
        /// Get segment-specific dashboard configuration
        /// </summary>
        public MobileDashboard GetDashboard()
        {
            return _segment switch
            {
                CustomerSegment.Personal => GetPersonalDashboard(),
                CustomerSegment.SME => GetSMEDashboard(),
                CustomerSegment.Corporate => GetCorporateDashboard(),
                CustomerSegment.PublicSector => GetPublicSectorDashboard(),
                _ => throw new ArgumentException($"Unknown segment: {_segment}")
            };
        }

        private MobileDashboard GetPersonalDashboard()
        {
            return new MobileDashboard
            {
                Segment = CustomerSegment.Personal,
                Title = "Personal Banking",
                Widgets = new List<DashboardWidget>
                {
                    new DashboardWidget
                    {
                        Type = "AccountBalance",
                        Title = "Account Balance",
                        Features = new List<string> { "Savings", "Current", "Fixed Deposit" }
                    },
                    new DashboardWidget
                    {
                        Type = "QuickActions",
                        Title = "Quick Actions",
                        Features = new List<string>
                        {
                            "Transfer Money", "Pay Bills", "Buy Airtime",
                            "M-Pesa", "Card Controls"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "RecentTransactions",
                        Title = "Recent Transactions",
                        Features = new List<string> { "Last 10 transactions" }
                    },
                    new DashboardWidget
                    {
                        Type = "Loans",
                        Title = "My Loans",
                        Features = new List<string> { "Personal Loans", "Mortgage", "View Balance" }
                    },
                    new DashboardWidget
                    {
                        Type = "Savings",
                        Title = "Savings Goals",
                        Features = new List<string> { "Emergency Fund", "Vacation", "New Car" }
                    },
                    new DashboardWidget
                    {
                        Type = "BudgetTracker",
                        Title = "Budget Tracker",
                        Features = new List<string> { "Monthly Budget", "Spending by Category" }
                    }
                },
                QuickActions = new List<string>
                {
                    "Check Balance", "Send Money", "Pay Bill", "Buy Airtime",
                    "Freeze Card", "Find ATM", "Get Statement"
                }
            };
        }

        private MobileDashboard GetSMEDashboard()
        {
            return new MobileDashboard
            {
                Segment = CustomerSegment.SME,
                Title = "SME Banking",
                Widgets = new List<DashboardWidget>
                {
                    new DashboardWidget
                    {
                        Type = "BusinessAccounts",
                        Title = "Business Accounts",
                        Features = new List<string> { "Current Account", "USD Account", "Merchant Account" }
                    },
                    new DashboardWidget
                    {
                        Type = "QuickActions",
                        Title = "Business Actions",
                        Features = new List<string>
                        {
                            "Pay Suppliers", "Process Payroll", "Merchant Payments",
                            "Bulk Payments", "Invoice Management"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "CashFlow",
                        Title = "Cash Flow",
                        Features = new List<string> { "Inflows", "Outflows", "Net Position", "Forecast" }
                    },
                    new DashboardWidget
                    {
                        Type = "Payroll",
                        Title = "Payroll Management",
                        Features = new List<string> { "Upload Payroll", "Schedule Payment", "View History" }
                    },
                    new DashboardWidget
                    {
                        Type = "Approvals",
                        Title = "Pending Approvals",
                        Features = new List<string> { "Awaiting My Approval", "My Pending Requests" }
                    },
                    new DashboardWidget
                    {
                        Type = "Reports",
                        Title = "Business Reports",
                        Features = new List<string> { "Profit & Loss", "Cash Flow Statement", "Tax Reports" }
                    }
                },
                QuickActions = new List<string>
                {
                    "Process Payroll", "Pay Supplier", "Check Cash Flow",
                    "Approve Payments", "Generate Report", "Manage Users"
                }
            };
        }

        private MobileDashboard GetCorporateDashboard()
        {
            return new MobileDashboard
            {
                Segment = CustomerSegment.Corporate,
                Title = "Corporate Banking",
                Widgets = new List<DashboardWidget>
                {
                    new DashboardWidget
                    {
                        Type = "TreasuryOverview",
                        Title = "Treasury Overview",
                        Features = new List<string>
                        {
                            "Multi-Currency Positions", "FX Exposure", "Liquidity Position",
                            "Cash Pooling Status"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "FXRates",
                        Title = "FX Trading",
                        Features = new List<string>
                        {
                            "Live FX Rates", "Execute Trade", "Hedging Positions",
                            "Market Analysis"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "BulkPayments",
                        Title = "Bulk Payments",
                        Features = new List<string>
                        {
                            "Upload Payment File", "Schedule Payments",
                            "Payment Status", "Approval Queue"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "TradeFinance",
                        Title = "Trade Finance",
                        Features = new List<string>
                        {
                            "Letters of Credit", "Bank Guarantees",
                            "Documentary Collections", "Supply Chain Finance"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "ApprovalWorkflow",
                        Title = "Approval Workflow",
                        Features = new List<string>
                        {
                            "Pending at Level 1", "Pending at Level 2",
                            "Pending at Level 3", "Pending at Level 4"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "Reporting",
                        Title = "Corporate Reporting",
                        Features = new List<string>
                        {
                            "Consolidated Statements", "Group Cash Position",
                            "Treasury Reports", "Compliance Dashboards"
                        }
                    }
                },
                QuickActions = new List<string>
                {
                    "Execute FX Trade", "Bulk Payment", "Approve Transaction",
                    "Check Liquidity", "Generate Report", "Manage Entities"
                }
            };
        }

        private MobileDashboard GetPublicSectorDashboard()
        {
            return new MobileDashboard
            {
                Segment = CustomerSegment.PublicSector,
                Title = "Government Banking",
                Widgets = new List<DashboardWidget>
                {
                    new DashboardWidget
                    {
                        Type = "BudgetOverview",
                        Title = "Budget Overview",
                        Features = new List<string>
                        {
                            "Total Budget", "Spent to Date", "Remaining Balance",
                            "Budget by Department"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "Payments",
                        Title = "Government Payments",
                        Features = new List<string>
                        {
                            "Salary Payments", "Pension Disbursements",
                            "Procurement Payments", "Subsidies"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "RevenueCollection",
                        Title = "Revenue Collection",
                        Features = new List<string>
                        {
                            "Tax Collections", "License Fees", "Fines",
                            "Service Charges"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "ApprovalQueue",
                        Title = "Approval Queue",
                        Features = new List<string>
                        {
                            "Initiator", "Reviewer", "Approver", "Authorizer"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "Compliance",
                        Title = "Compliance & Audit",
                        Features = new List<string>
                        {
                            "Audit Trail", "Compliance Reports",
                            "Regulatory Submissions", "Transparency Portal"
                        }
                    },
                    new DashboardWidget
                    {
                        Type = "Integrations",
                        Title = "Government Systems",
                        Features = new List<string>
                        {
                            "IFMIS Integration", "KRA iTax", "e-Citizen",
                            "County Systems"
                        }
                    }
                },
                QuickActions = new List<string>
                {
                    "Process Salaries", "Disburse Pensions", "Review Budget",
                    "Approve Payment", "Generate Audit Report", "Check Compliance"
                }
            };
        }

        /// <summary>
        /// Get segment-specific features for mobile app
        /// </summary>
        public List<MobileFeature> GetFeatures()
        {
            return _segment switch
            {
                CustomerSegment.Personal => GetPersonalFeatures(),
                CustomerSegment.SME => GetSMEFeatures(),
                CustomerSegment.Corporate => GetCorporateFeatures(),
                CustomerSegment.PublicSector => GetPublicSectorFeatures(),
                _ => throw new ArgumentException($"Unknown segment: {_segment}")
            };
        }

        private List<MobileFeature> GetPersonalFeatures()
        {
            return new List<MobileFeature>
            {
                new MobileFeature { Name = "Account Management", Icon = "account", Screen = "AccountsScreen" },
                new MobileFeature { Name = "Transfer Money", Icon = "transfer", Screen = "TransferScreen" },
                new MobileFeature { Name = "Pay Bills", Icon = "bill", Screen = "BillPaymentScreen" },
                new MobileFeature { Name = "Mobile Money", Icon = "mobile", Screen = "MobileMoneyScreen" },
                new MobileFeature { Name = "Loans", Icon = "loan", Screen = "LoansScreen" },
                new MobileFeature { Name = "Cards", Icon = "card", Screen = "CardsScreen" },
                new MobileFeature { Name = "Investments", Icon = "investment", Screen = "InvestmentsScreen" },
                new MobileFeature { Name = "Savings Goals", Icon = "savings", Screen = "SavingsGoalsScreen" },
                new MobileFeature { Name = "Budget Tracker", Icon = "budget", Screen = "BudgetScreen" },
                new MobileFeature { Name = "Statements", Icon = "statement", Screen = "StatementsScreen" },
                new MobileFeature { Name = "ATM Locator", Icon = "location", Screen = "ATMLocatorScreen" },
                new MobileFeature { Name = "Support", Icon = "support", Screen = "SupportScreen" }
            };
        }

        private List<MobileFeature> GetSMEFeatures()
        {
            return new List<MobileFeature>
            {
                new MobileFeature { Name = "Business Accounts", Icon = "business", Screen = "BusinessAccountsScreen" },
                new MobileFeature { Name = "Payroll", Icon = "payroll", Screen = "PayrollScreen" },
                new MobileFeature { Name = "Bulk Payments", Icon = "bulk", Screen = "BulkPaymentsScreen" },
                new MobileFeature { Name = "Merchant Payments", Icon = "merchant", Screen = "MerchantScreen" },
                new MobileFeature { Name = "Invoices", Icon = "invoice", Screen = "InvoicesScreen" },
                new MobileFeature { Name = "Expenses", Icon = "expenses", Screen = "ExpensesScreen" },
                new MobileFeature { Name = "Cash Flow", Icon = "cashflow", Screen = "CashFlowScreen" },
                new MobileFeature { Name = "Business Loans", Icon = "loan", Screen = "BusinessLoansScreen" },
                new MobileFeature { Name = "Trade Finance", Icon = "trade", Screen = "TradeFinanceScreen" },
                new MobileFeature { Name = "Reports", Icon = "report", Screen = "ReportsScreen" },
                new MobileFeature { Name = "User Management", Icon = "users", Screen = "UserManagementScreen" },
                new MobileFeature { Name = "Approvals", Icon = "approval", Screen = "ApprovalsScreen" },
                new MobileFeature { Name = "Integrations", Icon = "integration", Screen = "IntegrationsScreen" }
            };
        }

        private List<MobileFeature> GetCorporateFeatures()
        {
            return new List<MobileFeature>
            {
                new MobileFeature { Name = "Treasury Dashboard", Icon = "treasury", Screen = "TreasuryScreen" },
                new MobileFeature { Name = "FX Trading", Icon = "fx", Screen = "FXTradingScreen" },
                new MobileFeature { Name = "Multi-Currency", Icon = "currency", Screen = "MultiCurrencyScreen" },
                new MobileFeature { Name = "Bulk Payments", Icon = "bulk", Screen = "BulkPaymentsScreen" },
                new MobileFeature { Name = "Cash Pooling", Icon = "pool", Screen = "CashPoolingScreen" },
                new MobileFeature { Name = "Virtual Accounts", Icon = "virtual", Screen = "VirtualAccountsScreen" },
                new MobileFeature { Name = "Trade Finance", Icon = "trade", Screen = "TradeFinanceScreen" },
                new MobileFeature { Name = "Letters of Credit", Icon = "lc", Screen = "LettersOfCreditScreen" },
                new MobileFeature { Name = "SWIFT Transfers", Icon = "swift", Screen = "SWIFTScreen" },
                new MobileFeature { Name = "Investment Banking", Icon = "investment", Screen = "InvestmentBankingScreen" },
                new MobileFeature { Name = "Approvals", Icon = "approval", Screen = "ApprovalsScreen" },
                new MobileFeature { Name = "Group Reporting", Icon = "report", Screen = "GroupReportingScreen" },
                new MobileFeature { Name = "Entity Management", Icon = "entity", Screen = "EntityManagementScreen" },
                new MobileFeature { Name = "Compliance", Icon = "compliance", Screen = "ComplianceScreen" }
            };
        }

        private List<MobileFeature> GetPublicSectorFeatures()
        {
            return new List<MobileFeature>
            {
                new MobileFeature { Name = "Budget Management", Icon = "budget", Screen = "BudgetScreen" },
                new MobileFeature { Name = "Salary Payments", Icon = "salary", Screen = "SalaryScreen" },
                new MobileFeature { Name = "Pension Disbursements", Icon = "pension", Screen = "PensionScreen" },
                new MobileFeature { Name = "Procurement", Icon = "procurement", Screen = "ProcurementScreen" },
                new MobileFeature { Name = "Revenue Collection", Icon = "revenue", Screen = "RevenueScreen" },
                new MobileFeature { Name = "Tax Integration", Icon = "tax", Screen = "TaxScreen" },
                new MobileFeature { Name = "Grant Management", Icon = "grant", Screen = "GrantScreen" },
                new MobileFeature { Name = "Approval Workflow", Icon = "workflow", Screen = "WorkflowScreen" },
                new MobileFeature { Name = "Audit Trail", Icon = "audit", Screen = "AuditScreen" },
                new MobileFeature { Name = "Compliance Reports", Icon = "compliance", Screen = "ComplianceScreen" },
                new MobileFeature { Name = "Agency Management", Icon = "agency", Screen = "AgencyScreen" },
                new MobileFeature { Name = "IFMIS Integration", Icon = "ifmis", Screen = "IFMISScreen" },
                new MobileFeature { Name = "Transparency Portal", Icon = "transparency", Screen = "TransparencyScreen" }
            };
        }

        /// <summary>
        /// Validate transaction against segment limits
        /// </summary>
        public TransactionValidation ValidateTransaction(decimal amount, string transactionType)
        {
            var validation = new TransactionValidation
            {
                Amount = amount,
                Segment = _segment,
                TransactionType = transactionType,
                IsValid = true,
                Errors = new List<string>()
            };

            // Check single transaction limit
            if (amount > _limits.SingleTransactionLimit)
            {
                validation.IsValid = false;
                validation.Errors.Add($"Amount exceeds single transaction limit of {_limits.SingleTransactionLimit:N0} KES");
            }

            // Check if approval is required
            var workflow = ApprovalWorkflow.GetWorkflow(_segment, amount);
            if (workflow.Levels.Count > 0)
            {
                validation.RequiresApproval = true;
                validation.ApprovalLevels = workflow.Levels.Count;
            }

            return validation;
        }
    }

    public class MobileDashboard
    {
        public CustomerSegment Segment { get; set; }
        public string Title { get; set; }
        public List<DashboardWidget> Widgets { get; set; }
        public List<string> QuickActions { get; set; }
    }

    public class DashboardWidget
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public List<string> Features { get; set; }
    }

    public class MobileFeature
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Screen { get; set; }
    }

    public class TransactionValidation
    {
        public decimal Amount { get; set; }
        public CustomerSegment Segment { get; set; }
        public string TransactionType { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
        public bool RequiresApproval { get; set; }
        public int ApprovalLevels { get; set; }
    }
}
