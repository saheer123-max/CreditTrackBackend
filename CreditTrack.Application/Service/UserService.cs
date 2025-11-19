using CreditTrack.Domain.Common;
using CreditTrack.Application.Interfaces;
using CreditTrack.Application.IRepo;
using CreditTrack.Domain.Model;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreditTrack.Application.DTOs;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace CreditTrack.Application.Service
{
    public class UserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepo, IEmailService emailService)
        {
            _userRepo = userRepo;
            _emailService = emailService;
        }

        public async Task<ApiResponse<User>> CreateUserAndSendEmailAsync(UserReq userReq)
        {
            try
            {
            
                var existingUser = await _userRepo.GetUserByEmailAsync(userReq.Email);
                if (existingUser != null)
                    return ApiResponse<User>.Fail("Email already exists. Please use a different email.");

              
                string password = GeneratePassword();

           
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

          
                var newUser = new User
                {
                    Username = userReq.Username,
                    Email = userReq.Email,
                    PasswordHash = hashedPassword,
                    Role = userReq.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

          
                int userId = await _userRepo.CreateUserAsync(newUser);

           
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
                    Username = u.Username,
                    Role=u.Role
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







        public async Task<object> GetUserCountsAsync()
        {
            var (totalCustomers, totalSuppliers) = await _userRepo.GetUserCountsAsync();

            return new
            {
                TotalCustomers = totalCustomers,
                TotalSuppliers = totalSuppliers
            };
        }




        public async Task<object> GetTransactionTotalsAsync()
        {
            var (totalGiven, totalReceived) = await _userRepo.GetTransactionTotalsAsync();

            return new
            {
                TotalGiven = totalGiven,
                TotalReceived = totalReceived
            };
        }





        public async Task<IEnumerable<User>> GetCustomersAsync()
        {
            try
            {
                return await _userRepo.GetCustomersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customers");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetSuppliersAsync()
        {
            try
            {
                return await _userRepo.GetSuppliersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suppliers");
                throw;
            }
        }
    }


























};
