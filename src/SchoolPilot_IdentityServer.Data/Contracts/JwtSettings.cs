

using System.Text.Json.Serialization;

namespace SchoolPilot_IdentityServer.Data.Contracts
{
    public class JwtSettings
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("validIssuer")]
        public string ValidIssuer { get; set; }

        [JsonPropertyName("validAudience")]
        public string ValidAudience { get; set; }

        [JsonPropertyName("expires")]
        public double Expires { get; set; }
    }
}
