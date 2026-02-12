using System;
using System.Collections.Generic;
using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Channels.SegmentSupport
{
    /// <summary>
    /// Web portal channel support for all customer segments
    /// Based on best practices from HSBC, Bank of America, Capital One
    /// </summary>
    public class WebChannelSegmentSupport
    {
        private readonly CustomerSegment _segment;
        private readonly SegmentLimits _limits;
        private readonly SegmentFeatures _features;

        public WebChannelSegmentSupport(CustomerSegment segment)
        {
            _segment = segment;
            _limits = SegmentLimits.GetLimits(segment);
            _features = SegmentFeatures.GetFeatures(segment);
        }

        /// <summary>
        /// Get segment-specific web portal configuration
        /// </summary>
        public WebPortalConfiguration GetPortalConfiguration()
        {
            return _segment switch
            {
                CustomerSegment.Personal => GetPersonalPortal(),
                CustomerSegment.SME => GetSMEPortal(),
                CustomerSegment.Corporate => GetCorporatePortal(),
                CustomerSegment.PublicSector => GetPublicSectorPortal(),
                _ => throw new ArgumentException($"Unknown segment: {_segment}")
            };
        }

        private WebPortalConfiguration GetPersonalPortal()
        {
            return new WebPortalConfiguration
            {
                Segment = CustomerSegment.Personal,
                Title = "Personal Banking Portal",
                Theme = "personal",
                Modules = new List<WebModule>
                {
                    new WebModule
                    {
                        Name = "Dashboard",
                        Path = "/dashboard",
                        Features = new List<string>
                        {
                            "Account Overview", "Recent Transactions",
                            "Spending Analysis", "Savings Progress"
                        }
                    },
                    new WebModule
                    {
                        Name = "Accounts",
                        Path = "/accounts",
                        Features = new List<string>
                        {
                            "View Accounts", "Download Statements",
                            "Account Details", "Transaction History"
                        }
                    },
                    new WebModule
                    {
                        Name = "Transfers",
                        Path = "/transfers",
                        Features = new List<string>
                        {
                            "Internal Transfer", "External Transfer",
                            "Standing Orders", "Beneficiary Management"
                        }
                    },
                    new WebModule
                    {
                        Name = "Payments",
                        Path = "/payments",
                        Features = new List<string>
                        {
                            "Bill Payments", "Mobile Money",
                            "Airtime Top-Up", "Payment History"
                        }
                    },
                    new WebModule
                    {
                        Name = "Loans",
                        Path = "/loans",
                        Features = new List<string>
                        {
                            "View Loans", "Apply for Loan",
                            "Loan Calculator", "Payment Schedule"
                        }
                    },
                    new WebModule
                    {
                        Name = "Cards",
                        Path = "/cards",
                        Features = new List<string>
                        {
                            "View Cards", "Card Controls",
                            "Request New Card", "Transaction Limits"
                        }
                    },
                    new WebModule
                    {
                        Name = "Investments",
                        Path = "/investments",
                        Features = new List<string>
                        {
                            "View Investments", "Market Updates",
                            "Buy/Sell", "Portfolio Analysis"
                        }
                    }
                },
                Reports = new List<string>
                {
                    "Account Statement", "Transaction History",
                    "Tax Certificate", "Loan Statement"
                },
                SupportsDocumentUpload = false,
                SupportsBulkUpload = false,
                MaxDocumentSize = 5 * 1024 * 1024 // 5MB
            };
        }

        private WebPortalConfiguration GetSMEPortal()
        {
            return new WebPortalConfiguration
            {
                Segment = CustomerSegment.SME,
                Title = "SME Banking Portal",
                Theme = "business",
                Modules = new List<WebModule>
                {
                    new WebModule
                    {
                        Name = "Dashboard",
                        Path = "/dashboard",
                        Features = new List<string>
                        {
                            "Business Accounts Overview", "Cash Flow Analysis",
                            "Pending Approvals", "Payroll Status"
                        }
                    },
                    new WebModule
                    {
                        Name = "Accounts",
                        Path = "/accounts",
                        Features = new List<string>
                        {
                            "Business Accounts", "Multi-Currency",
                            "Statements", "Transaction History"
                        }
                    },
                    new WebModule
                    {
                        Name = "Payments",
                        Path = "/payments",
                        Features = new List<string>
                        {
                            "Supplier Payments", "Bulk Payments",
                            "Schedule Payments", "Payment Templates"
                        }
                    },
                    new WebModule
                    {
                        Name = "Payroll",
                        Path = "/payroll",
                        Features = new List<string>
                        {
                            "Upload Payroll", "Process Payroll",
                            "Payroll History", "Employee Management"
                        }
                    },
                    new WebModule
                    {
                        Name = "Invoices",
                        Path = "/invoices",
                        Features = new List<string>
                        {
                            "Create Invoice", "View Invoices",
                            "Send to Customers", "Payment Tracking"
                        }
                    },
                    new WebModule
                    {
                        Name = "Expenses",
                        Path = "/expenses",
                        Features = new List<string>
                        {
                            "Record Expenses", "Expense Categories",
                            "Petty Cash", "Expense Reports"
                        }
                    },
                    new WebModule
                    {
                        Name = "Loans",
                        Path = "/loans",
                        Features = new List<string>
                        {
                            "Business Loans", "Overdraft",
                            "Trade Finance", "Loan Applications"
                        }
                    },
                    new WebModule
                    {
                        Name = "Reports",
                        Path = "/reports",
                        Features = new List<string>
                        {
                            "Profit & Loss", "Cash Flow Statement",
                            "Balance Sheet", "Tax Reports"
                        }
                    },
                    new WebModule
                    {
                        Name = "Users",
                        Path = "/users",
                        Features = new List<string>
                        {
                            "User Management", "Role Assignment",
                            "Access Control", "Audit Log"
                        }
                    },
                    new WebModule
                    {
                        Name = "Integrations",
                        Path = "/integrations",
                        Features = new List<string>
                        {
                            "QuickBooks", "Xero", "Sage",
                            "KRA iTax", "API Keys"
                        }
                    }
                },
                Reports = new List<string>
                {
                    "Account Statement", "Transaction Report",
                    "Payroll Report", "Tax Report",
                    "Cash Flow Report", "Profit & Loss"
                },
                SupportsDocumentUpload = true,
                SupportsBulkUpload = true,
                MaxDocumentSize = 50 * 1024 * 1024 // 50MB
            };
        }

        private WebPortalConfiguration GetCorporatePortal()
        {
            return new WebPortalConfiguration
            {
                Segment = CustomerSegment.Corporate,
                Title = "Corporate Banking Portal",
                Theme = "corporate",
                Modules = new List<WebModule>
                {
                    new WebModule
                    {
                        Name = "Treasury Dashboard",
                        Path = "/treasury",
                        Features = new List<string>
                        {
                            "Multi-Currency Positions", "FX Exposure",
                            "Liquidity Management", "Cash Pooling"
                        }
                    },
                    new WebModule
                    {
                        Name = "FX Trading",
                        Path = "/fx-trading",
                        Features = new List<string>
                        {
                            "Live FX Rates", "Execute Trades",
                            "Forward Contracts", "Hedging Strategies"
                        }
                    },
                    new WebModule
                    {
                        Name = "Bulk Payments",
                        Path = "/bulk-payments",
                        Features = new List<string>
                        {
                            "Upload Payment Files", "Payment Templates",
                            "Schedule Payments", "Payment Status"
                        }
                    },
                    new WebModule
                    {
                        Name = "Virtual Accounts",
                        Path = "/virtual-accounts",
                        Features = new List<string>
                        {
                            "Create Virtual Accounts", "Account Management",
                            "Reconciliation", "Reporting"
                        }
                    },
                    new WebModule
                    {
                        Name = "Trade Finance",
                        Path = "/trade-finance",
                        Features = new List<string>
                        {
                            "Letters of Credit", "Bank Guarantees",
                            "Documentary Collections", "Supply Chain Finance"
                        }
                    },
                    new WebModule
                    {
                        Name = "SWIFT",
                        Path = "/swift",
                        Features = new List<string>
                        {
                            "International Transfers", "SWIFT Messages",
                            "Status Tracking", "Fee Calculator"
                        }
                    },
                    new WebModule
                    {
                        Name = "Approvals",
                        Path = "/approvals",
                        Features = new List<string>
                        {
                            "Approval Queue", "Multi-Level Workflow",
                            "Approval History", "Delegation"
                        }
                    },
                    new WebModule
                    {
                        Name = "Group Reporting",
                        Path = "/reporting",
                        Features = new List<string>
                        {
                            "Consolidated Statements", "Group Cash Position",
                            "Multi-Entity Reports", "Custom Reports"
                        }
                    },
                    new WebModule
                    {
                        Name = "Entity Management",
                        Path = "/entities",
                        Features = new List<string>
                        {
                            "Entity List", "Entity Hierarchy",
                            "Cross-Entity Transfers", "Consolidation"
                        }
                    },
                    new WebModule
                    {
                        Name = "Compliance",
                        Path = "/compliance",
                        Features = new List<string>
                        {
                            "Compliance Dashboard", "Regulatory Reports",
                            "Audit Trail", "Risk Management"
                        }
                    },
                    new WebModule
                    {
                        Name = "API Banking",
                        Path = "/api",
                        Features = new List<string>
                        {
                            "API Documentation", "API Keys",
                            "Usage Analytics", "Webhooks"
                        }
                    }
                },
                Reports = new List<string>
                {
                    "Treasury Report", "FX Position Report",
                    "Cash Flow Forecast", "Group Consolidated Statement",
                    "Multi-Entity Report", "Compliance Report"
                },
                SupportsDocumentUpload = true,
                SupportsBulkUpload = true,
                MaxDocumentSize = 100 * 1024 * 1024 // 100MB
            };
        }

        private WebPortalConfiguration GetPublicSectorPortal()
        {
            return new WebPortalConfiguration
            {
                Segment = CustomerSegment.PublicSector,
                Title = "Government Banking Portal",
                Theme = "government",
                Modules = new List<WebModule>
                {
                    new WebModule
                    {
                        Name = "Budget Dashboard",
                        Path = "/budget",
                        Features = new List<string>
                        {
                            "Budget Overview", "Expenditure Tracking",
                            "Budget by Department", "Variance Analysis"
                        }
                    },
                    new WebModule
                    {
                        Name = "Payments",
                        Path = "/payments",
                        Features = new List<string>
                        {
                            "Salary Payments", "Pension Disbursements",
                            "Procurement Payments", "Subsidies"
                        }
                    },
                    new WebModule
                    {
                        Name = "Revenue Collection",
                        Path = "/revenue",
                        Features = new List<string>
                        {
                            "Tax Collections", "License Fees",
                            "Fines & Penalties", "Service Charges"
                        }
                    },
                    new WebModule
                    {
                        Name = "Procurement",
                        Path = "/procurement",
                        Features = new List<string>
                        {
                            "IFMIS Integration", "Procurement Payments",
                            "Vendor Management", "Payment Tracking"
                        }
                    },
                    new WebModule
                    {
                        Name = "Grant Management",
                        Path = "/grants",
                        Features = new List<string>
                        {
                            "Grant Tracking", "Donor Funds",
                            "Disbursements", "Grant Reports"
                        }
                    },
                    new WebModule
                    {
                        Name = "Approval Workflow",
                        Path = "/approvals",
                        Features = new List<string>
                        {
                            "4-Level Approval", "Approval Queue",
                            "Workflow Status", "Audit Trail"
                        }
                    },
                    new WebModule
                    {
                        Name = "Compliance",
                        Path = "/compliance",
                        Features = new List<string>
                        {
                            "Audit Trail", "Compliance Reports",
                            "Regulatory Submissions", "Transparency Portal"
                        }
                    },
                    new WebModule
                    {
                        Name = "Agency Management",
                        Path = "/agencies",
                        Features = new List<string>
                        {
                            "Agency List", "Multi-Agency Access",
                            "Inter-Agency Transfers", "Agency Reports"
                        }
                    },
                    new WebModule
                    {
                        Name = "Reports",
                        Path = "/reports",
                        Features = new List<string>
                        {
                            "Budget Reports", "Expenditure Reports",
                            "Revenue Reports", "Compliance Reports"
                        }
                    },
                    new WebModule
                    {
                        Name = "Integrations",
                        Path = "/integrations",
                        Features = new List<string>
                        {
                            "IFMIS", "KRA iTax", "NHIF", "NSSF",
                            "e-Citizen", "County Systems"
                        }
                    }
                },
                Reports = new List<string>
                {
                    "Budget Report", "Expenditure Report",
                    "Revenue Report", "Compliance Report",
                    "Audit Trail Report", "Transparency Report"
                },
                SupportsDocumentUpload = true,
                SupportsBulkUpload = true,
                MaxDocumentSize = 100 * 1024 * 1024 // 100MB
            };
        }

        /// <summary>
        /// Generate segment-specific report
        /// </summary>
        public WebReport GenerateReport(string reportType, DateTime startDate, DateTime endDate)
        {
            return new WebReport
            {
                Segment = _segment,
                ReportType = reportType,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow,
                Format = "PDF",
                CanExport = true,
                ExportFormats = new List<string> { "PDF", "Excel", "CSV" }
            };
        }

        /// <summary>
        /// Upload bulk payment file
        /// </summary>
        public BulkUploadResult UploadBulkPaymentFile(byte[] fileContent, string fileName)
        {
            if (!_limits.SupportsBulkPayments)
            {
                return new BulkUploadResult
                {
                    Success = false,
                    Error = "Bulk payments not supported for this segment"
                };
            }

            return new BulkUploadResult
            {
                Success = true,
                FileName = fileName,
                RecordsProcessed = 100, // Example
                RecordsValid = 95,
                RecordsInvalid = 5,
                RequiresApproval = _limits.RequiresMultipleApprovals
            };
        }
    }

    public class WebPortalConfiguration
    {
        public CustomerSegment Segment { get; set; }
        public string Title { get; set; }
        public string Theme { get; set; }
        public List<WebModule> Modules { get; set; }
        public List<string> Reports { get; set; }
        public bool SupportsDocumentUpload { get; set; }
        public bool SupportsBulkUpload { get; set; }
        public int MaxDocumentSize { get; set; }
    }

    public class WebModule
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<string> Features { get; set; }
    }

    public class WebReport
    {
        public CustomerSegment Segment { get; set; }
        public string ReportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Format { get; set; }
        public bool CanExport { get; set; }
        public List<string> ExportFormats { get; set; }
    }

    public class BulkUploadResult
    {
        public bool Success { get; set; }
        public string FileName { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsValid { get; set; }
        public int RecordsInvalid { get; set; }
        public bool RequiresApproval { get; set; }
        public string Error { get; set; }
    }
}
