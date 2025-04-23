using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Security_Bug_Reports.Models;

namespace Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly byte[] _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public TokenService(IConfiguration config)
        {
            _issuer = config["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
            _audience = config["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
            var key = config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
            _secretKey = Encoding.UTF8.GetBytes(key);
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Username)
            };
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(_secretKey),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return _tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = new SymmetricSecurityKey(_secretKey)
                };

                var principal = _tokenHandler.ValidateToken(token, validationParams, out var validatedToken);


                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
