namespace fAzureHelper
{
    public class JwtOptions
    {
        /// <summary>
        /// Securely sign the token
        /// </summary>
        public string SecretKey { get; set; }

        public int ExpiryMinutes { get; set; }
        /// <summary>
        /// Which service created the token
        /// </summary>
        public string Issuer { get; set; }
    }
}