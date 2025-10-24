//using CreditTrack.Domain.Model; // User model
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.JsonWebTokens;
//using Microsoft.IdentityModel.Tokens;
//using System;

//using System.Security.Claims;
//using System.Text;

//namespace CreditTrack.Application.Service
//{
//    // Interface (optional, for clean architecture)


//    public class JwtService 
//    {
//        private readonly IConfiguration _config;

//        public JwtService(IConfiguration config)
//        {
//            _config = config;
//        }

//        public string GenerateToken(User user)
//        {
//            // 1️⃣ Add claims
//            var claims = new[]
//            {
//                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
//                new Claim("userId", user.Id.ToString()),
//                new Claim("role", user.Role),
//                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//            };

//            // 2️⃣ Create symmetric key from appsettings.json
//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

//            // 3️⃣ Signing credentials using HMAC SHA256
//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//            // 4️⃣ Generate JWT token
//            var token = new JwtSecurityToken(
//                issuer: _config["Jwt:Issuer"],
//                audience: _config["Jwt:Audience"],
//                claims: claims,
//                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresMinutes"])),
//                signingCredentials: creds
//            );

//            // 5️⃣ Return string token
//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }
//    }
//}
