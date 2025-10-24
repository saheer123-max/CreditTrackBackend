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
using BCrypt.Net;

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
                // 1. Check duplicate email
                var existingUser = await _userRepo.GetUserByEmailAsync(userReq.Email);
                if (existingUser != null)
                    return ApiResponse<User>.Fail("Email already exists. Please use a different email.");

                // 2. Generate random password
                string password = GeneratePassword();

                // 3. Hash password with BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                // 4. Map UserReq → User
                var newUser = new User
                {
                    Username = userReq.Username,
                    Email = userReq.Email,
                    PasswordHash = hashedPassword,
                    Role = userReq.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // 5. Save to DB
                int userId = await _userRepo.CreateUserAsync(newUser);

                // 6. Send email
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


        public async Task<ApiResponse<User>> LoginAsync(LoginRequest loginReq)
        {
            try
            {
                // 1. Find user by username or email
                var user = await _userRepo.GetUserByUsernameAsync(loginReq.Username);
                if (user == null)
                    return ApiResponse<User>.Fail("Invalid username or password.");

                // 2. Check if user is active
                if (!user.IsActive)
                    return ApiResponse<User>.Fail("Your account is inactive. Please contact support.");

                // 3. Verify password using BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginReq.Password, user.PasswordHash);
                if (!isPasswordValid)
                    return ApiResponse<User>.Fail("Invalid username or password.");

                // 4. Success response
                return ApiResponse<User>.Ok(user, "Login successful.");
            }
            catch (Exception ex)
            {
                return ApiResponse<User>.Fail("Error: " + ex.Message);
            }
        }































    }
};
