using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PifeGame.API
{
    public static class JwtHelper
    {
        public static IEnumerable<Claim> GetClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims;
        }
    }
}
