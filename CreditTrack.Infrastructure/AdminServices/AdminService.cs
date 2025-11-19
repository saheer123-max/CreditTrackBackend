using System;
using System.Text;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CreditTrack.Application.DTOs;
using CreditTrack.Domain.Common;
using CreditTrack.Domain.Model;

namespace CreditTrack.Application.Interfaces
{
    public class AdminService : IAdminService
    {
        private readonly IDbConnection _db;
        private readonly IConfiguration _cfg;
        private readonly ILogger<AdminService> _logger;

        public AdminService(IDbConnection db, IConfiguration cfg, ILogger<AdminService> logger)
        {
            _db = db;
            _cfg = cfg;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest req)
        {
            try
            {
                _logger.LogInformation("Login attempt started for user: {Username}", req.Username);

            
                var sql = "SELECT * FROM Users WHERE Username = @Username";
                var user = await _db.QueryFirstOrDefaultAsync<User>(sql, new { Username = req.Username });

                if (user == null)
                {
                    _logger.LogWarning("Login failed - user not found: {Username}", req.Username);
                    return ApiResponse<LoginResponse>.Fail("Invalid credentials.");
                }

          
                bool verified = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
                if (!verified)
                {
                    _logger.LogWarning("Login failed - password mismatch for user: {Username}", req.Username);
                    return ApiResponse<LoginResponse>.Fail("Invalid credentials.");
                }

                var token = GenerateToken(user);

                var resp = new LoginResponse
                {
                    Token = token.token,
                    ExpiresAt = token.expiresAt,
                    Username = user.Username,
                    Role = user.Role,

                      UserId = user.Id
                };

                string msg = user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                    ? "Admin login successful."
                    : "Customer login successful.";

                _logger.LogInformation("Login successful for user: {Username} (Role: {Role})", user.Username, user.Role);

                return ApiResponse<LoginResponse>.Ok(resp, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login for user: {Username}", req.Username);
                return ApiResponse<LoginResponse>.Fail($"Login failed: {ex.Message}");
            }
        }


        private (string token, DateTime expiresAt) GenerateToken(User user)
        {
            _logger.LogInformation("Generating JWT token for user: {Username}", user.Username);

            var jwtSection = _cfg.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key")!;
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");
            var expiryMinutes = jwtSection.GetValue<int>("ExpiresMinutes");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("uid", user.Id.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            _logger.LogInformation("JWT token generated for {Username}, expires at {ExpiresAt}", user.Username, expiresAt);

            return (token, expiresAt);
        }

      
        public async Task EnsureSeedAdminAsync()
        {
            _logger.LogInformation("Checking if admin user exists...");

            var username = "admin";
            var sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            var exists = await _db.ExecuteScalarAsync<int>(sql, new { Username = username });
            if (exists > 0)
            {
                _logger.LogInformation("Admin user already exists.");
                return;
            }

            _logger.LogInformation("Creating default admin user...");

            var hash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12);
            var insert = @"INSERT INTO Users (Username, Email, PasswordHash, Role, IsActive, CreatedAt)
                           VALUES (@Username, @Email, @PasswordHash, @Role, 1, SYSUTCDATETIME())";

            await _db.ExecuteAsync(insert, new
            {
                Username = username,
                Email = "admin@example.com",
                PasswordHash = hash,
                Role = "Admin"
            });

            _logger.LogInformation("Default admin user created successfully.");
        }
    }
}
