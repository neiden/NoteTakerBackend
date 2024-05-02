
using System.IdentityModel.Tokens.Jwt;

namespace Token.Services;

public class JWTParser
{


    public static string? GetClaim(string token, string claimType)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        return jsonToken?.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value;
    }

}