using Html2Sql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using trvotes.Controllers;

namespace trvotes
{
    public class JwtService
    {
        private const int EXPIRATION_MINUTES = 60 * 24 * 7;

        private readonly string issuer ;
        private readonly string audience ;
        private readonly string key ;

        public JwtService(IConfiguration configuration)
        {
            issuer = configuration["Jwt:Issuer"];
            audience = configuration["Jwt:Issuer"];
            key = configuration["Jwt:Key"];
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
            new JwtSecurityToken(
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
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            );
    }
}
