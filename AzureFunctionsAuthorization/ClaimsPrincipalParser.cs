using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AzureFunctionsAuthorization
{
    public static class ClaimsPrincipalHelper
    {
        public static ClaimsPrincipal? ParseFromRequest(HttpRequest req)
        {
            if (!req.Headers.TryGetValue("Authorization", out StringValues authorizationHeader))
            {
                return null;
            }

            string data = string.Empty;
            string json = string.Empty;

            var parts = authorizationHeader.ToString().Split(null) ?? new string[0];
            if (!(parts.Length == 2 && parts[0].Equals("Bearer")))
            {
                return null;
            }

            string bearerToken = parts[1];
            JwtSecurityTokenHandler handler = new();

            if (!handler.CanReadToken(bearerToken))
            {
                return null;
            }

            if (handler.ReadToken(bearerToken) is not JwtSecurityToken jsonToken)
            {
                return null;
            }

            string identityProvider = "AzureAD";
            string nameClaimType = "name";
            string roleClaimType = "role";

            ClaimsIdentity identity = new(identityProvider, nameClaimType, roleClaimType);
            identity.AddClaims(jsonToken.Claims.Select(claim => new Claim(claim.Type, claim.Value)));

            var result = new ClaimsPrincipal(identity);
            return result;
        }
    }
}