using System;
using System.Collections.Generic;
using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Channels.SegmentSupport
{
    /// <summary>
    /// USSD channel support for all customer segments
    /// </summary>
    public class UssdChannelSegmentSupport
    {
        private readonly CustomerSegment _segment;
        private readonly SegmentLimits _limits;

        public UssdChannelSegmentSupport(CustomerSegment segment)
        {
            _segment = segment;
            _limits = SegmentLimits.GetLimits(segment);
        }

        /// <summary>
        /// Get segment-specific USSD menu
        /// </summary>
        public UssdMenu GetMenu(string sessionId, string input = "")
        {
            return _segment switch
            {
                CustomerSegment.Personal => GetPersonalMenu(sessionId, input),
                CustomerSegment.SME => GetSMEMenu(sessionId, input),
                CustomerSegment.Corporate => GetCorporateMenu(sessionId, input),
                CustomerSegment.PublicSector => GetPublicSectorMenu(sessionId, input),
                _ => throw new ArgumentException($"Unknown segment: {_segment}")
            };
        }

        private UssdMenu GetPersonalMenu(string sessionId, string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new UssdMenu
                {
                    SessionId = sessionId,
                    Text = "Personal Banking\n" +
                           "1. Check Balance\n" +
                           "2. Mini Statement\n" +
                           "3. Send Money\n" +
                           "4. Pay Bills\n" +
                           "5. Buy Airtime\n" +
                           "6. Loans\n" +
                           "7. My Account",
                    ContinueSession = true
                };
            }

            return new UssdMenu
            {
                SessionId = sessionId,
                Text = "Please wait...",
                ContinueSession = false
            };
        }

        private UssdMenu GetSMEMenu(string sessionId, string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new UssdMenu
                {
                    SessionId = sessionId,
                    Text = "SME Banking (*234*2#)\n" +
                           "1. Account Balance\n" +
                           "2. Pay Supplier\n" +
                           "3. Process Payroll\n" +
                           "4. Cash Flow\n" +
                           "5. Approve Payments\n" +
                           "6. Business Loans\n" +
                           "7. Reports",
                    ContinueSession = true
                };
            }

            return new UssdMenu
            {
                SessionId = sessionId,
                Text = "Please wait...",
                ContinueSession = false
            };
        }

        private UssdMenu GetCorporateMenu(string sessionId, string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new UssdMenu
                {
                    SessionId = sessionId,
                    Text = "Corporate Banking (*234*3#)\n" +
                           "1. Treasury Position\n" +
                           "2. FX Rates\n" +
                           "3. Bulk Payments\n" +
                           "4. Approve Transactions\n" +
                           "5. Trade Finance\n" +
                           "6. Reports\n" +
                           "7. Support",
                    ContinueSession = true
                };
            }

            return new UssdMenu
            {
                SessionId = sessionId,
                Text = "Please wait...",
                ContinueSession = false
            };
        }

        private UssdMenu GetPublicSectorMenu(string sessionId, string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new UssdMenu
                {
                    SessionId = sessionId,
                    Text = "Government Banking (*234*4#)\n" +
                           "1. Budget Status\n" +
                           "2. Process Salaries\n" +
                           "3. Pension Payments\n" +
                           "4. Approve Payments\n" +
                           "5. Revenue Collection\n" +
                           "6. Compliance\n" +
                           "7. Support",
                    ContinueSession = true
                };
            }

            return new UssdMenu
            {
                SessionId = sessionId,
                Text = "Please wait...",
                ContinueSession = false
            };
        }

        /// <summary>
        /// Get quick balance via USSD
        /// </summary>
        public UssdResponse GetQuickBalance(string accountNumber)
        {
            return new UssdResponse
            {
                SessionId = Guid.NewGuid().ToString(),
                Text = $"Account Balance\n" +
                       $"Available: KES 500,000.00\n" +
                       $"Limit: KES {_limits.SingleTransactionLimit:N0}",
                ContinueSession = false
            };
        }

        /// <summary>
        /// Process USSD payment
        /// </summary>
        public UssdResponse ProcessPayment(string sessionId, decimal amount, string recipient)
        {
            if (amount > _limits.SingleTransactionLimit)
            {
                return new UssdResponse
                {
                    SessionId = sessionId,
                    Text = $"Amount exceeds limit of KES {_limits.SingleTransactionLimit:N0}",
                    ContinueSession = false
                };
            }

            if (_limits.RequiresMultipleApprovals)
            {
                return new UssdResponse
                {
                    SessionId = sessionId,
                    Text = $"Payment of KES {amount:N0} queued for approval",
                    ContinueSession = false
                };
            }

            return new UssdResponse
            {
                SessionId = sessionId,
                Text = $"Payment of KES {amount:N0} sent successfully",
                ContinueSession = false
            };
        }

        /// <summary>
        /// Get segment-specific USSD features
        /// </summary>
        public UssdFeatures GetFeatures()
        {
            return _segment switch
            {
                CustomerSegment.Personal => new UssdFeatures
                {
                    Segment = CustomerSegment.Personal,
                    ShortCode = "*234*1#",
                    QuickBalanceCode = "*234*0#",
                    SupportsApprovals = false,
                    MaxAmount = 250000m,
                    Features = new List<string>
                    {
                        "Check Balance", "Mini Statement", "Send Money",
                        "Pay Bills", "Buy Airtime", "Loans"
                    }
                },
                CustomerSegment.SME => new UssdFeatures
                {
                    Segment = CustomerSegment.SME,
                    ShortCode = "*234*2#",
                    QuickBalanceCode = "*234*2*0#",
                    SupportsApprovals = true,
                    MaxAmount = 1000000m,
                    Features = new List<string>
                    {
                        "Account Balance", "Pay Supplier", "Process Payroll",
                        "Cash Flow", "Approve Payments", "Business Loans", "Reports"
                    }
                },
                CustomerSegment.Corporate => new UssdFeatures
                {
                    Segment = CustomerSegment.Corporate,
                    ShortCode = "*234*3#",
                    QuickBalanceCode = "*234*3*0#",
                    SupportsApprovals = true,
                    MaxAmount = 10000000m,
                    Features = new List<string>
                    {
                        "Treasury Position", "FX Rates", "Bulk Payments",
                        "Approve Transactions", "Trade Finance", "Reports"
                    }
                },
                CustomerSegment.PublicSector => new UssdFeatures
                {
                    Segment = CustomerSegment.PublicSector,
                    ShortCode = "*234*4#",
                    QuickBalanceCode = "*234*4*0#",
                    SupportsApprovals = true,
                    MaxAmount = decimal.MaxValue,
                    Features = new List<string>
                    {
                        "Budget Status", "Process Salaries", "Pension Payments",
                        "Approve Payments", "Revenue Collection", "Compliance"
                    }
                },
                _ => throw new ArgumentException($"Unknown segment: {_segment}")
            };
        }
    }

    public class UssdMenu
    {
        public string SessionId { get; set; }
        public string Text { get; set; }
        public bool ContinueSession { get; set; }
    }

    public class UssdResponse
    {
        public string SessionId { get; set; }
        public string Text { get; set; }
        public bool ContinueSession { get; set; }
    }

    public class UssdFeatures
    {
        public CustomerSegment Segment { get; set; }
        public string ShortCode { get; set; }
        public string QuickBalanceCode { get; set; }
        public bool SupportsApprovals { get; set; }
        public decimal MaxAmount { get; set; }
        public List<string> Features { get; set; }
    }
}
