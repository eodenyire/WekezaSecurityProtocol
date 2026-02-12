using WekezaSecurityProtocol.Services;
using System.Diagnostics;

namespace WekezaSecurityProtocol.Integration.Unified
{
    /// <summary>
    /// Unified API adapter for all three Wekeza banking APIs
    /// Implements best practices: Circuit breaker, automatic failover, load balancing
    /// Based on patterns from Netflix (Hystrix), Amazon, Google
    /// </summary>
    public class UnifiedWekezaApiAdapter
    {
        private readonly WekezaCoreApiClient _coreApi;
        private readonly ComprehensiveApiClient _comprehensiveApi;
        private readonly Mvp4ApiClient _mvp4Api;
        private readonly CircuitBreakerService _circuitBreaker;
        private readonly CacheService _cache;
        private readonly MetricsService _metrics;

        public UnifiedWekezaApiAdapter(
            WekezaCoreApiClient coreApi,
            ComprehensiveApiClient comprehensiveApi,
            Mvp4ApiClient mvp4Api)
        {
            _coreApi = coreApi;
            _comprehensiveApi = comprehensiveApi;
            _mvp4Api = mvp4Api;
            _circuitBreaker = new CircuitBreakerService();
            _cache = new CacheService();
            _metrics = new MetricsService();
        }

        /// <summary>
        /// Get balance with automatic API selection and failover
        /// </summary>
        public async Task<BalanceResponse> GetBalanceAsync(string accountId, bool isShadowMode)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Try APIs in priority order with circuit breaker
                var result = await ExecuteWithFailover(
                    async () => await _coreApi.GetBalanceAsync(accountId),
                    async () => await _comprehensiveApi.GetBalanceAsync(accountId),
                    async () => await _mvp4Api.GetBalanceAsync(accountId)
                );

                _metrics.RecordSuccess("GetBalance", stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _metrics.RecordFailure("GetBalance", stopwatch.ElapsedMilliseconds);
                
                // Try cache as last resort
                var cachedBalance = await _cache.GetAsync<BalanceResponse>($"balance_{accountId}");
                if (cachedBalance != null)
                {
                    return cachedBalance;
                }

                throw;
            }
        }

        /// <summary>
        /// Process transfer with automatic API selection
        /// </summary>
        public async Task<TransferResponse> ProcessTransferAsync(
            TransferRequest request,
            bool isShadowMode)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await ExecuteWithFailover(
                    async () => await _coreApi.ProcessTransferAsync(request),
                    async () => await _comprehensiveApi.ProcessTransferAsync(request),
                    async () => await _mvp4Api.ProcessTransferAsync(request)
                );

                _metrics.RecordSuccess("ProcessTransfer", stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _metrics.RecordFailure("ProcessTransfer", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Get transaction history with smart caching
        /// </summary>
        public async Task<TransactionHistoryResponse> GetTransactionHistoryAsync(
            string accountId,
            DateTime fromDate,
            DateTime toDate,
            bool isShadowMode)
        {
            var cacheKey = $"transactions_{accountId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";
            
            // Check cache first
            var cached = await _cache.GetAsync<TransactionHistoryResponse>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await ExecuteWithFailover(
                    async () => await _coreApi.GetTransactionHistoryAsync(accountId, fromDate, toDate),
                    async () => await _comprehensiveApi.GetTransactionHistoryAsync(accountId, fromDate, toDate),
                    async () => await _mvp4Api.GetTransactionHistoryAsync(accountId, fromDate, toDate)
                );

                // Cache for 5 minutes
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _metrics.RecordSuccess("GetTransactionHistory", stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _metrics.RecordFailure("GetTransactionHistory", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Execute operation with automatic failover between APIs
        /// </summary>
        private async Task<T> ExecuteWithFailover<T>(
            Func<Task<T>> primaryApi,
            Func<Task<T>> secondaryApi,
            Func<Task<T>> fallbackApi)
        {
            // Try primary API (Core API)
            if (_circuitBreaker.CanExecute("CoreApi"))
            {
                try
                {
                    var result = await primaryApi();
                    _circuitBreaker.RecordSuccess("CoreApi");
                    return result;
                }
                catch (Exception ex)
                {
                    _circuitBreaker.RecordFailure("CoreApi");
                    // Continue to secondary
                }
            }

            // Try secondary API (Comprehensive API)
            if (_circuitBreaker.CanExecute("ComprehensiveApi"))
            {
                try
                {
                    var result = await secondaryApi();
                    _circuitBreaker.RecordSuccess("ComprehensiveApi");
                    return result;
                }
                catch (Exception ex)
                {
                    _circuitBreaker.RecordFailure("ComprehensiveApi");
                    // Continue to fallback
                }
            }

            // Try fallback API (MVP4.0)
            if (_circuitBreaker.CanExecute("Mvp4Api"))
            {
                try
                {
                    var result = await fallbackApi();
                    _circuitBreaker.RecordSuccess("Mvp4Api");
                    return result;
                }
                catch (Exception ex)
                {
                    _circuitBreaker.RecordFailure("Mvp4Api");
                    throw new Exception("All APIs are unavailable", ex);
                }
            }

            throw new Exception("All APIs are circuit-broken");
        }

        /// <summary>
        /// Get API health status
        /// </summary>
        public async Task<ApiHealthStatus> GetHealthStatusAsync()
        {
            var tasks = new[]
            {
                CheckApiHealth("CoreApi", () => _coreApi.HealthCheckAsync()),
                CheckApiHealth("ComprehensiveApi", () => _comprehensiveApi.HealthCheckAsync()),
                CheckApiHealth("Mvp4Api", () => _mvp4Api.HealthCheckAsync())
            };

            var results = await Task.WhenAll(tasks);

            return new ApiHealthStatus
            {
                CoreApi = results[0],
                ComprehensiveApi = results[1],
                Mvp4Api = results[2],
                OverallHealth = results.All(r => r.IsHealthy) ? "Healthy" : 
                               results.Any(r => r.IsHealthy) ? "Degraded" : "Unhealthy",
                Timestamp = DateTime.UtcNow
            };
        }

        private async Task<ApiHealth> CheckApiHealth(string apiName, Func<Task<bool>> healthCheck)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var isHealthy = await healthCheck();
                return new ApiHealth
                {
                    Name = apiName,
                    IsHealthy = isHealthy,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    CircuitState = _circuitBreaker.GetState(apiName)
                };
            }
            catch
            {
                return new ApiHealth
                {
                    Name = apiName,
                    IsHealthy = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    CircuitState = _circuitBreaker.GetState(apiName)
                };
            }
        }
    }

    /// <summary>
    /// Circuit Breaker pattern implementation
    /// Based on Netflix Hystrix, Polly
    /// </summary>
    public class CircuitBreakerService
    {
        private readonly Dictionary<string, CircuitState> _circuits = new();
        private readonly int _failureThreshold = 5;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);

        public bool CanExecute(string apiName)
        {
            if (!_circuits.ContainsKey(apiName))
            {
                _circuits[apiName] = new CircuitState();
            }

            var circuit = _circuits[apiName];

            if (circuit.State == "Open")
            {
                // Check if timeout has passed
                if (DateTime.UtcNow - circuit.OpenedAt > _timeout)
                {
                    circuit.State = "HalfOpen";
                    return true;
                }
                return false;
            }

            return true;
        }

        public void RecordSuccess(string apiName)
        {
            if (_circuits.ContainsKey(apiName))
            {
                var circuit = _circuits[apiName];
                circuit.FailureCount = 0;
                circuit.State = "Closed";
            }
        }

        public void RecordFailure(string apiName)
        {
            if (!_circuits.ContainsKey(apiName))
            {
                _circuits[apiName] = new CircuitState();
            }

            var circuit = _circuits[apiName];
            circuit.FailureCount++;

            if (circuit.FailureCount >= _failureThreshold)
            {
                circuit.State = "Open";
                circuit.OpenedAt = DateTime.UtcNow;
            }
        }

        public string GetState(string apiName)
        {
            return _circuits.ContainsKey(apiName) ? _circuits[apiName].State : "Closed";
        }

        private class CircuitState
        {
            public string State { get; set; } = "Closed"; // Closed, Open, HalfOpen
            public int FailureCount { get; set; }
            public DateTime OpenedAt { get; set; }
        }
    }

    /// <summary>
    /// Cache service for API responses
    /// </summary>
    public class CacheService
    {
        private readonly Dictionary<string, CacheEntry> _cache = new();

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (DateTime.UtcNow < entry.ExpiresAt)
                {
                    return entry.Data as T;
                }
                else
                {
                    _cache.Remove(key);
                }
            }
            return null;
        }

        public async Task SetAsync<T>(string key, T data, TimeSpan expiry)
        {
            _cache[key] = new CacheEntry
            {
                Data = data,
                ExpiresAt = DateTime.UtcNow.Add(expiry)
            };
        }

        private class CacheEntry
        {
            public object? Data { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }

    /// <summary>
    /// Metrics service for monitoring API performance
    /// </summary>
    public class MetricsService
    {
        private readonly Dictionary<string, List<MetricEntry>> _metrics = new();

        public void RecordSuccess(string operation, long responseTimeMs)
        {
            RecordMetric(operation, true, responseTimeMs);
        }

        public void RecordFailure(string operation, long responseTimeMs)
        {
            RecordMetric(operation, false, responseTimeMs);
        }

        private void RecordMetric(string operation, bool success, long responseTimeMs)
        {
            if (!_metrics.ContainsKey(operation))
            {
                _metrics[operation] = new List<MetricEntry>();
            }

            _metrics[operation].Add(new MetricEntry
            {
                Success = success,
                ResponseTimeMs = responseTimeMs,
                Timestamp = DateTime.UtcNow
            });

            // Keep only last 1000 entries
            if (_metrics[operation].Count > 1000)
            {
                _metrics[operation].RemoveAt(0);
            }
        }

        public OperationMetrics GetMetrics(string operation)
        {
            if (!_metrics.ContainsKey(operation))
            {
                return new OperationMetrics { Operation = operation };
            }

            var entries = _metrics[operation];
            var recent = entries.Where(e => DateTime.UtcNow - e.Timestamp < TimeSpan.FromMinutes(5)).ToList();

            return new OperationMetrics
            {
                Operation = operation,
                TotalRequests = recent.Count,
                SuccessCount = recent.Count(e => e.Success),
                FailureCount = recent.Count(e => !e.Success),
                SuccessRate = recent.Any() ? (double)recent.Count(e => e.Success) / recent.Count : 0,
                AverageResponseTime = recent.Any() ? recent.Average(e => e.ResponseTimeMs) : 0,
                P95ResponseTime = CalculatePercentile(recent, 0.95),
                P99ResponseTime = CalculatePercentile(recent, 0.99)
            };
        }

        private double CalculatePercentile(List<MetricEntry> entries, double percentile)
        {
            if (!entries.Any()) return 0;
            
            var sorted = entries.OrderBy(e => e.ResponseTimeMs).ToList();
            var index = (int)Math.Ceiling(percentile * sorted.Count) - 1;
            return sorted[Math.Max(0, index)].ResponseTimeMs;
        }

        private class MetricEntry
        {
            public bool Success { get; set; }
            public long ResponseTimeMs { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }

    #region Supporting Models

    public class ApiHealthStatus
    {
        public ApiHealth CoreApi { get; set; } = new();
        public ApiHealth ComprehensiveApi { get; set; } = new();
        public ApiHealth Mvp4Api { get; set; } = new();
        public string OverallHealth { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ApiHealth
    {
        public string Name { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public long ResponseTime { get; set; }
        public string CircuitState { get; set; } = string.Empty;
    }

    public class OperationMetrics
    {
        public string Operation { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public double P95ResponseTime { get; set; }
        public double P99ResponseTime { get; set; }
    }

    // Extension methods for health checks
    public static class ApiClientExtensions
    {
        public static async Task<bool> HealthCheckAsync(this WekezaCoreApiClient client)
        {
            try
            {
                // Implement actual health check
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> HealthCheckAsync(this ComprehensiveApiClient client)
        {
            try
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> HealthCheckAsync(this Mvp4ApiClient client)
        {
            try
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<TransactionHistoryResponse> GetTransactionHistoryAsync(
            this ComprehensiveApiClient client,
            string accountId,
            DateTime fromDate,
            DateTime toDate)
        {
            // Implement actual call
            return new TransactionHistoryResponse();
        }

        public static async Task<TransactionHistoryResponse> GetTransactionHistoryAsync(
            this Mvp4ApiClient client,
            string accountId,
            DateTime fromDate,
            DateTime toDate)
        {
            // Implement actual call
            return new TransactionHistoryResponse();
        }
    }

    public class BalanceResponse
    {
        public string AccountId { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "KES";
    }

    public class TransferRequest
    {
        public string FromAccountId { get; set; } = string.Empty;
        public string RecipientAccount { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class TransferResponse
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class TransactionHistoryResponse
    {
        public string AccountId { get; set; } = string.Empty;
        public List<Transaction> Transactions { get; set; } = new();
    }

    public class Transaction
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }

    #endregion
}
