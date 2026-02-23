using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PermissionAuth.Models;

namespace PermissionAuth.Services;

public class JwtService(IConfiguration config)
{
    public string GenerateToken(User user)
    {
        var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds= new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry= DateTime.UtcNow.AddHours(double.Parse(config["Jwt:ExpiryHours"]!));

        var claims = new[]
        {
            new Claim("userId",user.Id.ToString()),
            new Claim("email",user.Email),
            new Claim("username",user.Username),
        };

        var token = new JwtSecurityToken(
            issuer:config["Jwt:Issuer"],
            audience:config["Jwt:Audience"],
            claims:claims,
            expires:expiry,
            signingCredentials:creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
