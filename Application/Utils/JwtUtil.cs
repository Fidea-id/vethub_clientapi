using Domain.Entities.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Utils
{
    public static class JwtUtil
    {
        public static string Issuer = "VetHubAPIAuthentication";
        public static string Audience = "VetHubAPI";
        public static string Key = "wD7q7RPi9OjQAGP16Bi49hwiUmE2fpUW";

        public static string SetSessionToken(Profile profile)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, "JwtUser_" + profile.Id.ToString()),
                new Claim (ClaimTypes.Role, profile.Roles),
                new Claim (ClaimTypes.Email, profile.Email),
                new Claim (ClaimTypes.NameIdentifier, profile.Entity),
                new Claim("Id", profile.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(Issuer,
                Audience,
                claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddYears(1),
                signingCredentials: signIn
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
