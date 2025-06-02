using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebTelegram.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebTelegram.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly TelegramDbContext _context;

        public UsersController(TelegramDbContext context)
        {
            _context = context;
        }

        [HttpPost("find-or-create-chat")]
        public async Task<IActionResult> FindOrCreateChat([FromQuery] string phone, [FromQuery] int currentUserId)
        {
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (targetUser == null)
                return NotFound("Користувача з таким телефоном не знайдено");

            var existingChat = await _context.Chats
                .Include(c => c.ChatUsers)
                .Where(c => !c.IsGroup &&
                            c.ChatUsers.Any(cu => cu.UserId == currentUserId) &&
                            c.ChatUsers.Any(cu => cu.UserId == targetUser.Id))
                .FirstOrDefaultAsync();

            if (existingChat != null)
            {
                return Ok(new { chatId = existingChat.Id, displayName = targetUser.DisplayName });
            }

            var newChat = new Chat
            {
                IsGroup = false,
                ChatUsers = new List<ChatUser>
                {
                    new ChatUser { UserId = currentUserId },
                    new ChatUser { UserId = targetUser.Id }
                }
            };

            _context.Chats.Add(newChat);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                chatId = newChat.Id,
                displayName = targetUser.DisplayName,
                username = targetUser.Username,
                phoneNumber = targetUser.PhoneNumber
            });
        }

        [HttpGet("user-chats/{userId}")]
        public async Task<IActionResult> GetUserChats(int userId)
        {
            var chats = await _context.Chats
                .Include(c => c.ChatUsers)
                    .ThenInclude(cu => cu.User)
                .Where(c => c.ChatUsers.Any(cu => cu.UserId == userId))
                .Select(c => new
                {
                    chatId = c.Id,
                    chatName = c.ChatName,
                    isGroup = c.IsGroup,
                    displayName = c.IsGroup
                        ? (c.ChatName ?? "Груповий чат")
                        : c.ChatUsers
                            .Where(cu => cu.UserId != userId)
                            .Select(cu => cu.User.DisplayName)
                            .FirstOrDefault() ?? "Невідомо"
                })
                .ToListAsync();

            return Ok(chats);
        }

        //[HttpGet("chat-messages/{chatId}")]
        //public async Task<IActionResult> GetChatMessages(int chatId)
        //{
        //    var messages = await _context.Messages
        //        .Include(m => m.Sender)
        //        .Where(m => m.ChatId == chatId)
        //        .OrderBy(m => m.SentAt)
        //        .Select(m => new
        //        {
        //            messageId = m.Id,
        //            chatId = m.ChatId,
        //            senderId = m.SenderId,
        //            senderDisplayName = m.Sender.DisplayName,
        //            text = m.Text,
        //            attachmentUrl = m.AttachmentUrl,
        //            sentAt = m.SentAt
        //        })
        //        .ToListAsync();

        //    if (messages == null || messages.Count == 0)
        //        return NotFound("Повідомлень не знайдено");
        //    return Ok(messages);
        //}
        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(int chatId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    //m.Id,
                    //m.Text,
                    //m.SentAt,
                    //senderName = m.Sender.DisplayName
                    messageId = m.Id,
                    chatId = m.ChatId,
                    senderId = m.SenderId,
                    senderDisplayName = m.Sender.DisplayName,
                    text = m.Text,
                    attachmentUrl = m.AttachmentUrl,
                    sentAt = m.SentAt
                })
                .ToListAsync();

            if (messages == null || messages.Count == 0)
                return NotFound("Повідомлень не знайдено");
            return Ok(messages);
        }

        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(int chatId, [FromBody] SendMessageDto dto)
        {
            var chat = await _context.Chats.FindAsync(chatId);
            if (chat == null) return NotFound("Чат не знайдено");

            var message = new Message
            {
                ChatId = chatId,
                SenderId = dto.SenderId,
                Text = dto.Text,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message.Id,
                message.Text,
                message.SentAt,
                senderName = (await _context.Users.FindAsync(dto.SenderId))?.DisplayName ?? "Unknown"
            });
        }
    }

public class SendMessageDto
    {
        [Required]
        public int SenderId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;
    }

}