using CreditTrack.Domain.Common;
using CreditTrack.Application.Interfaces;
using CreditTrack.Domain.IRepo;
using CreditTrack.Domain.Model;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreditTrack.Application.DTOs;

namespace CreditTrack.Application.Service
{
    public class UserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepo, IEmailService emailService)
        {
            _userRepo = userRepo;
            _emailService = emailService;
        }

        public async Task<ApiResponse<User>> CreateUserAndSendEmailAsync(UserReq userReq)
        {
            try
            {
                // Check duplicate email
                var existingUser = await _userRepo.GetUserByEmailAsync(userReq.Email);
                if (existingUser != null)
                    return ApiResponse<User>.Fail("Email already exists. Please use a different email.");

                // Generate random password
                string password = GeneratePassword();

                // Map UserReq → User
                var newUser = new User
                {
                    Username = userReq.Username,
                    Email = userReq.Email,
                    PasswordHash = HashPassword(password),
                    Role = userReq.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Save to DB
                int userId = await _userRepo.CreateUserAsync(newUser);

                // Send email
                string subject = "Your Account Details";
                string body = $"Username: {newUser.Username}\nPassword: {password}";
                await _emailService.SendEmailAsync(newUser.Email, subject, body);

                return ApiResponse<User>.Ok(newUser, "User created and email sent successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<User>.Fail("Error: " + ex.Message);
            }
        }

        private string GeneratePassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new char[length];
            for (int i = 0; i < length; i++)
                password[i] = chars[random.Next(chars.Length)];
            return new string(password);
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }





        public async Task<ApiResponse<IEnumerable<UserRes>>> GetAllUsernamesAsync()
        {
            try
            {
                var users = await _userRepo.GetAllUsersAsync();



                var UserRes = users.Select(u => new UserRes
                {
                    Id = u.Id,
                    Username = u.Username
                });
                return ApiResponse<IEnumerable<UserRes>>.Ok(UserRes, "Users fetched successfully.");
            }
            catch (Exception ex)
            {
                // Corrected type here
                return ApiResponse<IEnumerable<UserRes>>.Fail("Error: " + ex.Message);
            }
        }

    }
};
