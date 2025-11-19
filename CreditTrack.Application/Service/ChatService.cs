using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CreditTrack.Application.IRepo;
using CreditTrack.Application.DTOs;
using CreditTrack.Domain.Model;
using CreditTrack.Domain.Common;
using Microsoft.Extensions.Logging;
using CreditTrack.Application.Interfaces;


namespace CreditTrack.Application.Service
{
    public class ChatService : IChatService
    
    {
        private readonly IChatRepository _repository;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IChatRepository repository, ILogger<ChatService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> SaveMessageAsync(ChatMessageDto dto)
        {
            try
            {
                var chatMessage = new ChatMessage
                {
                    SenderId = dto.SenderId,
                    ReceiverId = dto.ReceiverId,
                    Message = dto.Message,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.AddMessageAsync(chatMessage);

                _logger.LogInformation($"Message saved successfully from {dto.SenderId} to {dto.ReceiverId}");

                return ApiResponse<string>.Ok("Message saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving message");
                return ApiResponse<string>.Fail("Failed to save message");
            }
        }


        public async Task<IEnumerable<string>> GetAllChatUsersAsync()
        {
            try
            {
                var users = await _repository.GetAllChatUsersAsync();
                _logger.LogInformation(" Retrieved all chat users");
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error while fetching chat users");
                return new List<string>();
            }
        }


        public async Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(string userId)
        {
            try
            {
                var history = await _repository.GetChatHistoryAsync(userId);


                var response = history.Select(m => new ChatMessageDto
                {
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Message = m.Message,
                    CreatedAt = m.CreatedAt
                }).ToList();

                _logger.LogInformation($" Fetched chat history for user: {userId}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $" Error while fetching chat history for user {userId}");
                return new List<ChatMessageDto>();
            }
        }




    }
    }

