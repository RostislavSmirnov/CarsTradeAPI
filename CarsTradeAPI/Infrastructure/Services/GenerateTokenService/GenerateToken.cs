using CarsTradeAPI.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarsTradeAPI.Infrastructure.Services.GenerateTokenService
{
    public class GenerateTokenService : IGenerateToken
    {
        private readonly IConfiguration _configuration;
        public GenerateTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <inheritdoc/>
        public string GenerateToken(Employee employee) 
        {
            Claim[] Claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, employee.EmployeeLogin),
                new Claim("employee_id", employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Role, employee.EmployeeRole)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: Claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
