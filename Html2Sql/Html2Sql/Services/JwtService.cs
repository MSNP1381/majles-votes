using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Html2Sql;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Resturant_managment.Services;

public class JwtService
{
    private const int EXPIRATION_MINUTES = 60 * 24 * 7;

    private readonly IConfiguration _configuration;
    private readonly string? issuer = null;
    private readonly string? audience = null;
    private readonly string? key = null;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtService(string Issuer, string Audience, string Key)
    {
        issuer = Issuer;
        audience = Audience;
        key = Key;
    }

    public Token CreateToken(IdentityUser user)
    {
        var expiration = DateTime.UtcNow.AddMinutes(
            user.UserName == "admin" ? 10000000 : EXPIRATION_MINUTES
        );

        var token = CreateJwtToken(CreateClaims(user), CreateSigningCredentials(), expiration);

        var tokenHandler = new JwtSecurityTokenHandler();

        return new Token { Value = tokenHandler.WriteToken(token), ExpiryDate = expiration };
    }

    private JwtSecurityToken CreateJwtToken(
        Claim[] claims,
        SigningCredentials credentials,
        DateTime expiration
    ) =>
        issuer == null
            ? new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiration,
                signingCredentials: credentials
            )
            : new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expiration,
                signingCredentials: credentials
            );

    private Claim[] CreateClaims(IdentityUser user) =>
        new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

    private SigningCredentials CreateSigningCredentials() =>
        new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(String.IsNullOrEmpty(key) ? _configuration["Jwt:Key"] : key)
            ),
            SecurityAlgorithms.HmacSha256
        );
}
