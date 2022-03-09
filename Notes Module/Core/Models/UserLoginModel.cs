using System;
using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace Nekres.Notes.Core.Models
{
    internal class UserLoginModel
    {
        /// <summary>
        /// JWT with a JSON payload.
        /// Claims:
        ///     sub   - string   - A unique identifier for the GW2Auth-Account of the user. This value is not consistent across different clients.
        ///     scope - string[] - The list of authorized scopes
        ///     iss   - string   - A URL of the issuer which created this Access-Token
        ///     iat   - long     - UNIX-Timestamp (seconds): Timestamp at which this token was issued
        ///     exp   - long     - UNIX-Timestamp (seconds): Timestamp at which this token will expire
        ///     gw2:tokens       - Dictionary[string, object] - A JSON-Object containing all subtokens (and some additional information) for all by the user authorized GW2-Accounts.
        ///                        Key: GW2-Account-ID ; Value: <see cref="JwtSubtokenModel"/>.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "permissions", ItemConverterType = typeof(StringEnumConverter))]
        public IEnumerable<TokenPermission> Permissions { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }

        public UserLoginModel() { }

        public static UserLoginModel FromResponse(string json)
        {
            return JsonConvert.DeserializeObject<UserLoginModel>(json);
        }
    }
}
