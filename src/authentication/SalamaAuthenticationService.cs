using WekezaSecurityProtocol.Models;
using System.Security.Cryptography;
using System.Text;

namespace WekezaSecurityProtocol.Authentication
{
    /// <summary>
    /// Core authentication service implementing dual-path PIN validation
    /// Detects normal vs shadow mode based on PIN entry and behavioral signals
    /// </summary>
    public class SalamaAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAlertService _alertService;
        private readonly double _behavioralThreshold = 0.5; // Accelerometer variance threshold

        public SalamaAuthenticationService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IAlertService alertService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _alertService = alertService;
        }

        /// <summary>
        /// Authenticate user with dual-path PIN validation
        /// Detects shadow mode via reversed PIN or behavioral signals
        /// </summary>
        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            // Retrieve user from repository
            var user = await _userRepository.GetUserByAccountIdAsync(request.AccountId);
            if (user == null)
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid account or PIN"
                };
            }

            // Determine session mode based on PIN and behavioral data
            var sessionMode = DetermineSessionMode(request, user);

            // For shadow mode, always return "success" to maintain deception
            // For standard mode, validate actual PIN
            bool isAuthenticated = sessionMode == SessionMode.Shadow || 
                                  ValidatePin(request.PinHash, user.PinHash);

            if (!isAuthenticated)
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid account or PIN"
                };
            }

            // Generate session token with appropriate scope
            var scope = sessionMode == SessionMode.Shadow ? new[] { "shadow" } : new[] { "standard" };
            var token = _jwtTokenService.GenerateToken(user.AccountId, scope);

            // If shadow mode, trigger silent alert to SOC
            if (sessionMode == SessionMode.Shadow)
            {
                await TriggerSilentAlertAsync(user, request);
            }

            return new AuthenticationResponse
            {
                Success = true,
                SessionToken = token,
                ExpiresAt = DateTime.UtcNow.AddHours(6),
                SessionMode = sessionMode,
                UserInfo = new UserInfo
                {
                    AccountId = user.AccountId,
                    AccountName = user.AccountName,
                    AccountType = user.AccountType,
                    Currency = user.Currency
                }
            };
        }

        /// <summary>
        /// Determine session mode based on PIN entry and behavioral signals
        /// </summary>
        private SessionMode DetermineSessionMode(AuthenticationRequest request, User user)
        {
            // Check for reversed PIN
            if (IsReversedPin(request.PinHash, user.PinHash, user.PlainPin))
            {
                return SessionMode.Shadow;
            }

            // Check for pre-configured duress code
            if (user.DuressCodeHash != null && request.PinHash == user.DuressCodeHash)
            {
                return SessionMode.Shadow;
            }

            // Check behavioral signals (tremor detection)
            if (request.BehavioralData != null && 
                request.BehavioralData.AccelerometerVariance > _behavioralThreshold)
            {
                return SessionMode.Shadow;
            }

            return SessionMode.Standard;
        }

        /// <summary>
        /// Check if entered PIN is the reverse of user's actual PIN
        /// </summary>
        private bool IsReversedPin(string enteredPinHash, string actualPinHash, string plainPin)
        {
            if (string.IsNullOrEmpty(plainPin))
            {
                return false;
            }

            // Reverse the plain PIN and hash it
            var reversedPin = new string(plainPin.Reverse().ToArray());
            var reversedPinHash = HashPin(reversedPin);

            return enteredPinHash == reversedPinHash;
        }

        /// <summary>
        /// Validate PIN hash matches stored hash
        /// </summary>
        private bool ValidatePin(string enteredPinHash, string storedPinHash)
        {
            return enteredPinHash == storedPinHash;
        }

        /// <summary>
        /// Hash PIN using SHA256
        /// </summary>
        private string HashPin(string pin)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Trigger silent alert to Security Operations Center
        /// </summary>
        private async Task TriggerSilentAlertAsync(User user, AuthenticationRequest request)
        {
            var alert = new SecurityAlert
            {
                UserId = user.AccountId,
                AlertType = "DURESS_MODE_ACTIVATED",
                Timestamp = DateTime.UtcNow,
                Location = request.Location,
                DeviceId = request.DeviceId,
                Severity = "HIGH",
                Description = $"Duress mode activated for user {user.AccountId}"
            };

            await _alertService.SendAlertAsync(alert);
        }
    }

    #region Repository and Service Interfaces

    /// <summary>
    /// User repository interface for data access
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetUserByAccountIdAsync(string accountId);
        Task UpdateUserAsync(User user);
    }

    /// <summary>
    /// User model with authentication data
    /// </summary>
    public class User
    {
        public string AccountId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Currency { get; set; } = "KES";
        public string PinHash { get; set; } = string.Empty;
        public string PlainPin { get; set; } = string.Empty; // Stored encrypted in real implementation
        public string? DuressCodeHash { get; set; }
        public DateTime? ShadowModeActivatedAt { get; set; }
        public bool IsInShadowMode { get; set; }
    }

    /// <summary>
    /// JWT token service interface
    /// </summary>
    public interface IJwtTokenService
    {
        string GenerateToken(string accountId, string[] scopes);
        bool ValidateToken(string token);
        Dictionary<string, string> GetTokenClaims(string token);
    }

    /// <summary>
    /// Alert service interface for SOC notifications
    /// </summary>
    public interface IAlertService
    {
        Task SendAlertAsync(SecurityAlert alert);
    }

    /// <summary>
    /// Security alert model
    /// </summary>
    public class SecurityAlert
    {
        public string UserId { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public GpsCoordinates? Location { get; set; }
        public string? DeviceId { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    #endregion
}
