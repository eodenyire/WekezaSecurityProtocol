using System.ComponentModel.DataAnnotations;

namespace WekezaSecurityProtocol.Models
{
    /// <summary>
    /// Authentication request with PIN and optional behavioral data
    /// </summary>
    public class AuthenticationRequest
    {
        /// <summary>
        /// User account identifier (phone number, email, or account number)
        /// </summary>
        [Required]
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// PIN hash (client should pre-hash before transmission)
        /// </summary>
        [Required]
        [StringLength(256, MinimumLength = 4)]
        public string PinHash { get; set; } = string.Empty;

        /// <summary>
        /// Device identifier for tracking trusted devices
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// GPS coordinates (latitude, longitude)
        /// </summary>
        public GpsCoordinates? Location { get; set; }

        /// <summary>
        /// Behavioral analysis data for duress detection
        /// </summary>
        public BehavioralData? BehavioralData { get; set; }
    }

    /// <summary>
    /// GPS coordinates for location tracking
    /// </summary>
    public class GpsCoordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Behavioral analysis data for duress detection
    /// </summary>
    public class BehavioralData
    {
        /// <summary>
        /// Accelerometer variance during PIN entry (high variance = tremor)
        /// </summary>
        public double AccelerometerVariance { get; set; }

        /// <summary>
        /// Time taken to enter PIN (milliseconds)
        /// </summary>
        public int EntryDurationMs { get; set; }

        /// <summary>
        /// Number of backspaces/corrections during entry
        /// </summary>
        public int CorrectionCount { get; set; }
    }
}
