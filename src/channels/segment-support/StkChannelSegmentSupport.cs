using System;
using System.Collections.Generic;
using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Channels.SegmentSupport
{
    /// <summary>
    /// STK Push channel support for all customer segments
    /// </summary>
    public class StkChannelSegmentSupport
    {
        private readonly CustomerSegment _segment;
        private readonly SegmentLimits _limits;

        public StkChannelSegmentSupport(CustomerSegment segment)
        {
            _segment = segment;
            _limits = SegmentLimits.GetLimits(segment);
        }

        /// <summary>
        /// Initiate STK Push payment with segment-specific limits
        /// </summary>
        public StkPushResult InitiatePayment(decimal amount, string phoneNumber, string reference)
        {
            var result = new StkPushResult
            {
                Segment = _segment,
                Amount = amount,
                PhoneNumber = phoneNumber,
                Reference = reference,
                Timestamp = DateTime.UtcNow
            };

            // Validate against segment limits
            if (amount > _limits.SingleTransactionLimit)
            {
                result.Success = false;
                result.ErrorMessage = $"Amount exceeds {_segment} limit of {_limits.SingleTransactionLimit:N0} KES";
                return result;
            }

            // Check if approval is required
            if (_limits.RequiresMultipleApprovals)
            {
                var workflow = ApprovalWorkflow.GetWorkflow(_segment, amount);
                if (workflow.Levels.Count > 0)
                {
                    result.RequiresApproval = true;
                    result.ApprovalLevels = workflow.Levels.Count;
                    result.Status = "PendingApproval";
                    result.Success = true;
                    result.Message = $"Payment queued for {workflow.Levels.Count}-level approval";
                    return result;
                }
            }

            // Process payment
            result.Success = true;
            result.Status = "Sent";
            result.Message = "STK Push sent to customer";
            result.TransactionId = $"STK{DateTime.UtcNow.Ticks}";

            return result;
        }

        /// <summary>
        /// Get segment-specific STK features
        /// </summary>
        public StkFeatures GetFeatures()
        {
            return _segment switch
            {
                CustomerSegment.Personal => new StkFeatures
                {
                    Segment = CustomerSegment.Personal,
                    MaxAmount = 250000m,
                    SupportsRecurring = true,
                    SupportsBulk = false,
                    PaymentCategories = new List<string>
                    {
                        "Bills", "Shopping", "Transport", "Food",
                        "Entertainment", "Utilities", "Other"
                    }
                },
                CustomerSegment.SME => new StkFeatures
                {
                    Segment = CustomerSegment.SME,
                    MaxAmount = 1000000m,
                    SupportsRecurring = true,
                    SupportsBulk = true,
                    MaxBulkRecipients = 1000,
                    PaymentCategories = new List<string>
                    {
                        "Supplier Payment", "Employee Salary", "Utility Bills",
                        "Rent", "Insurance", "Loan Repayment", "Other"
                    }
                },
                CustomerSegment.Corporate => new StkFeatures
                {
                    Segment = CustomerSegment.Corporate,
                    MaxAmount = 10000000m,
                    SupportsRecurring = true,
                    SupportsBulk = true,
                    MaxBulkRecipients = int.MaxValue,
                    SupportsScheduling = true,
                    PaymentCategories = new List<string>
                    {
                        "Supplier Payment", "Payroll", "Corporate Services",
                        "Professional Fees", "Investments", "Loan Repayment", "Other"
                    }
                },
                CustomerSegment.PublicSector => new StkFeatures
                {
                    Segment = CustomerSegment.PublicSector,
                    MaxAmount = decimal.MaxValue,
                    SupportsRecurring = true,
                    SupportsBulk = true,
                    MaxBulkRecipients = int.MaxValue,
                    SupportsScheduling = true,
                    RequiresMandatoryApproval = true,
                    PaymentCategories = new List<string>
                    {
                        "Salary Payment", "Pension", "Procurement",
                        "Subsidy", "Grant", "Tax Refund", "Other Government Payment"
                    }
                },
                _ => throw new ArgumentException($"Unknown segment: {_segment}")
            };
        }

        /// <summary>
        /// Process bulk STK push (for SME, Corporate, Public Sector)
        /// </summary>
        public BulkStkResult ProcessBulkSTK(List<StkPaymentRequest> payments)
        {
            if (!_limits.SupportsBulkPayments)
            {
                return new BulkStkResult
                {
                    Success = false,
                    Error = "Bulk STK payments not supported for this segment"
                };
            }

            var result = new BulkStkResult
            {
                Segment = _segment,
                TotalPayments = payments.Count,
                ProcessedAt = DateTime.UtcNow,
                RequiresApproval = _limits.RequiresMultipleApprovals
            };

            foreach (var payment in payments)
            {
                if (payment.Amount <= _limits.SingleTransactionLimit)
                {
                    result.SuccessfulPayments++;
                }
                else
                {
                    result.FailedPayments++;
                }
            }

            result.Success = result.FailedPayments == 0;
            return result;
        }
    }

    public class StkPushResult
    {
        public CustomerSegment Segment { get; set; }
        public bool Success { get; set; }
        public decimal Amount { get; set; }
        public string PhoneNumber { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public string TransactionId { get; set; }
        public bool RequiresApproval { get; set; }
        public int ApprovalLevels { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class StkFeatures
    {
        public CustomerSegment Segment { get; set; }
        public decimal MaxAmount { get; set; }
        public bool SupportsRecurring { get; set; }
        public bool SupportsBulk { get; set; }
        public int MaxBulkRecipients { get; set; }
        public bool SupportsScheduling { get; set; }
        public bool RequiresMandatoryApproval { get; set; }
        public List<string> PaymentCategories { get; set; }
    }

    public class StkPaymentRequest
    {
        public string PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string Category { get; set; }
    }

    public class BulkStkResult
    {
        public CustomerSegment Segment { get; set; }
        public bool Success { get; set; }
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public bool RequiresApproval { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string Error { get; set; }
    }
}
