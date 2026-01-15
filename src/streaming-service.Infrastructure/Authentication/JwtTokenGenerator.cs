using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using streaming_service.Domain.Ports;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Infrastructure.Authentication
{
    public class JwtTokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AccessToken GenerateToken(Guid userId, Guid sessionId, Guid reservationId, Dictionary<string, object>? claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "super_secret_key_1234567890_min_length"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;

            var jwtClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("sessionId", sessionId.ToString()),
                new Claim("reservationId", reservationId.ToString())
            };

            if (claims != null)
            {
                foreach (var claim in claims)
                {
                    jwtClaims.Add(new Claim(claim.Key, claim.Value.ToString() ?? string.Empty));
                }
            }

            // Create Access Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(jwtClaims),
                Expires = now.AddMinutes(60), // Match default in AccessToken.Create
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            // Create Refresh Token (in a real app, this might be a random string persisted in DB)
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return AccessToken.Create(tokenString, refreshToken, userId, sessionId);
        }

        public AccessToken RefreshToken(AccessToken expiredToken)
        {
            // Simplified: Just generate a new one with same details
            return GenerateToken(expiredToken.UserId, expiredToken.SessionId, Guid.Empty, null); 
        }
    }
}
