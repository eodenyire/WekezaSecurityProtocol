using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WekezaSecurityProtocol.Authentication;
using WekezaSecurityProtocol.Integration;
using WekezaSecurityProtocol.Models;
using WekezaSecurityProtocol.Services;

namespace WekezaSecurityProtocol.Controllers
{
    /// <summary>
    /// Authentication controller for dual-path PIN authentication
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly SalamaAuthenticationService _authService;

        public AuthenticationController(SalamaAuthenticationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login with dual-path PIN validation
        /// Detects normal vs shadow mode based on PIN and behavioral signals
        /// </summary>
        /// <param name="request">Authentication request with PIN and optional behavioral data</param>
        /// <returns>Session token with appropriate scope (standard/shadow)</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticationResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] AuthenticationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.AuthenticateAsync(request);

            if (!response.Success)
            {
                return BadRequest(new { message = response.ErrorMessage });
            }

            // Note: Never reveal to client whether they're in shadow mode
            // Always return same structure regardless of mode
            return Ok(response);
        }
    }

    /// <summary>
    /// Account services controller with shadow mode support
    /// </summary>
    [ApiController]
    [Route("api/v1/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UnifiedBankingService _bankingService;

        public AccountController(UnifiedBankingService bankingService)
        {
            _bankingService = bankingService;
        }

        /// <summary>
        /// Get account balance
        /// Returns mock balance in shadow mode, real balance in standard mode
        /// </summary>
        [HttpGet("balance")]
        [ProducesResponseType(typeof(BalanceResponse), 200)]
        public async Task<ActionResult<BalanceResponse>> GetBalance()
        {
            var accountId = GetAccountIdFromToken();
            var isShadowMode = IsShadowMode();

            var balance = await _bankingService.GetBalanceAsync(accountId, isShadowMode);
            return Ok(balance);
        }

        /// <summary>
        /// Get transaction history
        /// Returns mock transactions in shadow mode, real transactions in standard mode
        /// </summary>
        [HttpGet("transactions")]
        [ProducesResponseType(typeof(TransactionHistoryResponse), 200)]
        public async Task<ActionResult<TransactionHistoryResponse>> GetTransactions(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var accountId = GetAccountIdFromToken();
            var isShadowMode = IsShadowMode();

            var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var to = toDate ?? DateTime.UtcNow;

            var transactions = await _bankingService.GetTransactionHistoryAsync(
                accountId, from, to, isShadowMode);
            
            return Ok(transactions);
        }

        private string GetAccountIdFromToken()
        {
            // Extract from JWT claims in production
            return User.FindFirst("sub")?.Value ?? "unknown";
        }

        private bool IsShadowMode()
        {
            // Check HttpContext items set by middleware
            return HttpContext.Items.ContainsKey("IsShadowMode") && 
                   (bool)HttpContext.Items["IsShadowMode"]!;
        }
    }

    /// <summary>
    /// Transfer controller with shadow mode quarantine support
    /// </summary>
    [ApiController]
    [Route("api/v1/transfers")]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly UnifiedBankingService _bankingService;

        public TransferController(UnifiedBankingService bankingService)
        {
            _bankingService = bankingService;
        }

        /// <summary>
        /// Process mobile money transfer
        /// Quarantines transaction in shadow mode, executes in standard mode
        /// </summary>
        [HttpPost("mobile-money")]
        [ProducesResponseType(typeof(TransferResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<TransferResponse>> MobileMoneyTransfer(
            [FromBody] TransferRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var accountId = GetAccountIdFromToken();
            var isShadowMode = IsShadowMode();

            // Set source account from authenticated user
            request.FromAccountId = accountId;

            var response = await _bankingService.ProcessTransferAsync(request, isShadowMode);

            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }

        /// <summary>
        /// Process internal bank transfer
        /// </summary>
        [HttpPost("internal")]
        [ProducesResponseType(typeof(TransferResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<TransferResponse>> InternalTransfer(
            [FromBody] TransferRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var accountId = GetAccountIdFromToken();
            var isShadowMode = IsShadowMode();

            request.FromAccountId = accountId;
            request.TransferType = "Internal";

            var response = await _bankingService.ProcessTransferAsync(request, isShadowMode);

            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }

        private string GetAccountIdFromToken()
        {
            return User.FindFirst("sub")?.Value ?? "unknown";
        }

        private bool IsShadowMode()
        {
            return HttpContext.Items.ContainsKey("IsShadowMode") && 
                   (bool)HttpContext.Items["IsShadowMode"]!;
        }
    }
}
