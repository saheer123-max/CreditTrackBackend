using System;
using System.Text;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
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
        private readonly int _maxFailed = 5;
        private readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(30);

        public AdminService(IDbConnection db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest req)
        {
            var sql = "SELECT * FROM Admins WHERE Username = @Username AND IsActive = 1";
            var admin = await _db.QueryFirstOrDefaultAsync<Admin>(sql, new { Username = req.Username });

            if (admin == null)
                return ApiResponse<LoginResponse>.Fail("Invalid credentials.");

            // check lockout
            if (admin.LockoutUntil.HasValue && admin.LockoutUntil.Value > DateTime.UtcNow)
            {
                var minsLeft = (admin.LockoutUntil.Value - DateTime.UtcNow).TotalMinutes;
                return ApiResponse<LoginResponse>.Fail($"Account locked. Try after {Math.Ceiling(minsLeft)} minutes.");
            }

            // verify password
            var verified = BCrypt.Net.BCrypt.Verify(req.Password, admin.PasswordHash);
            if (!verified)
            {
                // increment failed attempts
                admin.FailedAttempts += 1;
                DateTime? lockoutUntil = null;
                if (admin.FailedAttempts >= _maxFailed)
                {
                    lockoutUntil = DateTime.UtcNow.Add(_lockoutDuration);
                    // reset failed attempts on lockout but you may want to keep value
                    //admin.FailedAttempts = 0;
                }

                var updSql = "UPDATE Admins SET FailedAttempts = @FailedAttempts, LockoutUntil = @LockoutUntil WHERE Id = @Id";
                await _db.ExecuteAsync(updSql, new { FailedAttempts = admin.FailedAttempts, LockoutUntil = lockoutUntil, Id = admin.Id });

                if (lockoutUntil.HasValue)
                    return ApiResponse<LoginResponse>.Fail($"Too many failed attempts. Account locked until {lockoutUntil.Value.ToUniversalTime():u} UTC.");

                return ApiResponse<LoginResponse>.Fail($"Invalid credentials. {_maxFailed - admin.FailedAttempts} attempts left.");
            }

            // success: reset failed attempts and lockout
            var resetSql = "UPDATE Admins SET FailedAttempts = 0, LockoutUntil = NULL WHERE Id = @Id";
            await _db.ExecuteAsync(resetSql, new { Id = admin.Id });

            // generate JWT
            var token = GenerateToken(admin);

            var resp = new LoginResponse
            {
                Token = token.token,
                ExpiresAt = token.expiresAt,
                Username = admin.Username,
                Role = admin.Role
            };

            return ApiResponse<LoginResponse>.Ok(resp, "Login successful.");
        }


        private (string token, DateTime expiresAt) GenerateToken(Admin admin)
        {
            var jwtSection = _cfg.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key")!;
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");
            var expiryMinutes = jwtSection.GetValue<int>("ExpiresMinutes");

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, admin.Username),
        new Claim(ClaimTypes.Role, admin.Role),
        new Claim("uid", admin.Id.ToString())
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

            return (token, expiresAt);
        }

        // Seed admin at startup if not exists
        public async Task EnsureSeedAdminAsync()
        {
            var username = "admin";
            var sql = "SELECT COUNT(1) FROM Admins WHERE Username = @Username";
            var exists = await _db.ExecuteScalarAsync<int>(sql, new { Username = username });
            if (exists > 0) return;

            // generate a secure password - you can change default here or fetch from config/env
            var hash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12);
            var isValid = BCrypt.Net.BCrypt.Verify("Admin@123", hash);

            var insert = @"INSERT INTO Admins (Username, PasswordHash, Role, FailedAttempts, IsActive, CreatedAt)
                       VALUES (@Username, @PasswordHash, @Role, 0, 1, SYSUTCDATETIME())";
            await _db.ExecuteAsync(insert, new { Username = username, PasswordHash = hash, Role = "Admin" });
        }
    }

}
