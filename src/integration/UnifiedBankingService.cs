using WekezaSecurityProtocol.Services;

namespace WekezaSecurityProtocol.Integration
{
    /// <summary>
    /// Unified banking service that routes to either Wekeza API or Shadow service
    /// based on session mode
    /// </summary>
    public class UnifiedBankingService
    {
        private readonly WekezaCoreApiClient _coreApiClient;
        private readonly ComprehensiveApiClient _comprehensiveApiClient;
        private readonly Mvp4ApiClient _mvp4ApiClient;
        private readonly ShadowBankingService _shadowService;

        public UnifiedBankingService(
            WekezaCoreApiClient coreApiClient,
            ComprehensiveApiClient comprehensiveApiClient,
            Mvp4ApiClient mvp4ApiClient,
            ShadowBankingService shadowService)
        {
            _coreApiClient = coreApiClient;
            _comprehensiveApiClient = comprehensiveApiClient;
            _mvp4ApiClient = mvp4ApiClient;
            _shadowService = shadowService;
        }

        /// <summary>
        /// Get account balance - routes to shadow service in duress mode
        /// </summary>
        public async Task<BalanceResponse> GetBalanceAsync(string accountId, bool isShadowMode)
        {
            if (isShadowMode)
            {
                return await _shadowService.GetBalanceAsync(accountId);
            }

            // Route to primary Wekeza Core API
            return await _coreApiClient.GetBalanceAsync(accountId);
        }

        /// <summary>
        /// Get transaction history - routes to shadow service in duress mode
        /// </summary>
        public async Task<TransactionHistoryResponse> GetTransactionHistoryAsync(
            string accountId, 
            DateTime fromDate, 
            DateTime toDate,
            bool isShadowMode)
        {
            if (isShadowMode)
            {
                return await _shadowService.GetTransactionHistoryAsync(accountId, fromDate, toDate);
            }

            // Route to primary Wekeza Core API
            return await _coreApiClient.GetTransactionHistoryAsync(accountId, fromDate, toDate);
        }

        /// <summary>
        /// Process transfer - quarantines in shadow mode, executes in standard mode
        /// </summary>
        public async Task<TransferResponse> ProcessTransferAsync(
            TransferRequest request, 
            bool isShadowMode)
        {
            if (isShadowMode)
            {
                // Quarantine transaction, return fake success
                return await _shadowService.ProcessTransferAsync(request);
            }

            // Execute real transfer via Wekeza Core API
            return await _coreApiClient.ProcessTransferAsync(request);
        }
    }

    #region Wekeza API Clients

    /// <summary>
    /// Client for Wekeza.Core.Api (primary production API)
    /// </summary>
    public class WekezaCoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public WekezaCoreApiClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task<BalanceResponse> GetBalanceAsync(string accountId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/accounts/{accountId}/balance");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            return content ?? throw new Exception("Failed to retrieve balance");
        }

        public async Task<TransactionHistoryResponse> GetTransactionHistoryAsync(
            string accountId, 
            DateTime fromDate, 
            DateTime toDate)
        {
            var url = $"{_baseUrl}/api/transactions/statement?accountId={accountId}&from={fromDate:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadFromJsonAsync<TransactionHistoryResponse>();
            return content ?? throw new Exception("Failed to retrieve transaction history");
        }

        public async Task<TransferResponse> ProcessTransferAsync(TransferRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/transactions/transfer", request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadFromJsonAsync<TransferResponse>();
            return content ?? throw new Exception("Failed to process transfer");
        }
    }

    /// <summary>
    /// Client for ComprehensiveWekezaApi
    /// </summary>
    public class ComprehensiveApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ComprehensiveApiClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task<BalanceResponse> GetBalanceAsync(string accountId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/account/balance/{accountId}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            return content ?? throw new Exception("Failed to retrieve balance");
        }

        public async Task<TransferResponse> ProcessTransferAsync(TransferRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/transfer/execute", request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadFromJsonAsync<TransferResponse>();
            return content ?? throw new Exception("Failed to process transfer");
        }
    }

    /// <summary>
    /// Client for MVP4.0 (legacy API)
    /// </summary>
    public class Mvp4ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public Mvp4ApiClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task<BalanceResponse> GetBalanceAsync(string accountId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/balance/{accountId}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            return content ?? throw new Exception("Failed to retrieve balance");
        }

        public async Task<TransferResponse> ProcessTransferAsync(TransferRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/transfer", request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadFromJsonAsync<TransferResponse>();
            return content ?? throw new Exception("Failed to process transfer");
        }
    }

    #endregion
}
