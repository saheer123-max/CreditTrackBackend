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
            try
            {
                // 1️⃣ Check if it's an Admin
                var adminSql = "SELECT * FROM Admins WHERE Username = @Username AND IsActive = 1";
                var admin = await _db.QueryFirstOrDefaultAsync<Admin>(adminSql, new { Username = req.Username });

                if (admin != null)
                {
                    // ----- ADMIN LOGIN LOGIC -----
                    if (admin.LockoutUntil.HasValue && admin.LockoutUntil.Value > DateTime.UtcNow)
                    {
                        var minsLeft = (admin.LockoutUntil.Value - DateTime.UtcNow).TotalMinutes;
                        return ApiResponse<LoginResponse>.Fail($"Account locked. Try after {Math.Ceiling(minsLeft)} minutes.");
                    }

                    bool verified = BCrypt.Net.BCrypt.Verify(req.Password, admin.PasswordHash);
                    if (!verified)
                    {
                        admin.FailedAttempts += 1;
                        DateTime? lockoutUntil = null;

                        if (admin.FailedAttempts >= _maxFailed)
                        {
                            lockoutUntil = DateTime.UtcNow.Add(_lockoutDuration);
                        }

                        var updSql = "UPDATE Admins SET FailedAttempts = @FailedAttempts, LockoutUntil = @LockoutUntil WHERE Id = @Id";
                        await _db.ExecuteAsync(updSql, new { admin.FailedAttempts, lockoutUntil, admin.Id });

                        if (lockoutUntil.HasValue)
                            return ApiResponse<LoginResponse>.Fail($"Too many failed attempts. Account locked until {lockoutUntil.Value:u} UTC.");

                        return ApiResponse<LoginResponse>.Fail($"Invalid credentials. {_maxFailed - admin.FailedAttempts} attempts left.");
                    }

                    // Success - reset lockout
                    var resetSql = "UPDATE Admins SET FailedAttempts = 0, LockoutUntil = NULL WHERE Id = @Id";
                    await _db.ExecuteAsync(resetSql, new { admin.Id });

                    // JWT generate
                    var token = GenerateToken(admin);

                    var resp = new LoginResponse
                    {
                        Token = token.token,
                        ExpiresAt = token.expiresAt,
                        Username = admin.Username,
                        Role = "Admin"
                    };

                    return ApiResponse<LoginResponse>.Ok(resp, "Admin login successful.");
                }

                // 2️⃣ Otherwise, check for a normal User
                var userSql = "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1";
                var user = await _db.QueryFirstOrDefaultAsync<User>(userSql, new { Username = req.Username });

                if (user == null)
                    return ApiResponse<LoginResponse>.Fail("Invalid username or password.");

                bool userVerified = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
                if (!userVerified)
                    return ApiResponse<LoginResponse>.Fail("Invalid username or password.");

                // ✅ No lockout for user

                // Generate JWT token
                var userToken = GenerateToken(user);

                var userResp = new LoginResponse
                {
                    Token = userToken.token,
                    ExpiresAt = userToken.expiresAt,
                    Username = user.Username,
                    Role = "User",
                     UserId = user.Id
                };

                return ApiResponse<LoginResponse>.Ok(userResp, "User login successful.");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.Fail("Error: " + ex.Message);
            }
        }



        private (string token, DateTime expiresAt) GenerateToken(object entity)
        {
            var jwtSection = _cfg.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key")!;
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");
            var expiryMinutes = jwtSection.GetValue<int>("ExpiresMinutes");

            string id;
            string username;
            string role;

            // entity Admin ആണോ എന്ന് പരിശോധിക്കുന്നു
            if (entity is Admin admin)
            {
                id = admin.Id.ToString();
                username = admin.Username;
                role = admin.Role;
            }
            // entity User ആണോ എന്ന് പരിശോധിക്കുന്നു
            else if (entity is User user)
            {
                id = user.Id.ToString();
                username = user.Username;
                role = user.Role ?? "User"; // role ഇല്ലെങ്കിൽ default "User"
            }
            else
            {
                throw new Exception("Invalid entity type for JWT generation.");
            }

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, id),
        new Claim(JwtRegisteredClaimNames.UniqueName, username),
        new Claim(ClaimTypes.Role, role),
        new Claim("uid", id)
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
