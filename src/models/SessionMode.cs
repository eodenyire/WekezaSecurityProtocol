namespace WekezaSecurityProtocol.Models
{
    /// <summary>
    /// Defines the operational mode of a user session
    /// </summary>
    public enum SessionMode
    {
        /// <summary>
        /// Standard banking session with full functionality
        /// </summary>
        Standard,

        /// <summary>
        /// Shadow/Duress mode with mock data and transaction quarantine
        /// </summary>
        Shadow
    }

    /// <summary>
    /// Trigger source for shadow mode activation
    /// </summary>
    public enum ShadowTrigger
    {
        /// <summary>
        /// User entered reversed PIN
        /// </summary>
        ReversedPin,

        /// <summary>
        /// User entered pre-configured duress code
        /// </summary>
        DuressCode,

        /// <summary>
        /// Behavioral analysis detected duress (tremor, accelerometer)
        /// </summary>
        BehavioralDetection,

        /// <summary>
        /// Manual trigger from trusted device
        /// </summary>
        ManualTrigger
    }
}
