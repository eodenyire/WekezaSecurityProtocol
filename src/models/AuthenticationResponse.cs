namespace WekezaSecurityProtocol.Models
{
    /// <summary>
    /// Authentication response with session token and metadata
    /// </summary>
    public class AuthenticationResponse
    {
        /// <summary>
        /// JWT session token
        /// </summary>
        public string SessionToken { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration timestamp
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Session mode (standard or shadow)
        /// </summary>
        public SessionMode SessionMode { get; set; }

        /// <summary>
        /// User account information
        /// </summary>
        public UserInfo UserInfo { get; set; } = new();

        /// <summary>
        /// Indicates if this is a successful authentication
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if authentication failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Basic user information returned after authentication
    /// </summary>
    public class UserInfo
    {
        public string AccountId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Currency { get; set; } = "KES";
    }
}
