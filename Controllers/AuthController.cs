using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebTelegram.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly TelegramDbContext _context;

    public AuthController(TelegramDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto register)
    {
        if (string.IsNullOrWhiteSpace(register.PhoneNumber))
            return BadRequest(new { message = "Номер телефону обов’язковий" });

        if (string.IsNullOrWhiteSpace(register.Password))
            return BadRequest(new { message = "Пароль обов’язковий" });

        if (_context.Users.Any(u => u.PhoneNumber == register.PhoneNumber))
            return BadRequest(new { message = "Такий номер вже зареєстрований" });

        var user = new User
        {
            PhoneNumber = register.PhoneNumber,
            PasswordHash = register.Password,
            Username = "user" + Guid.NewGuid().ToString("N").Substring(0, 8),
            DisplayName = "Новий користувач"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(new { message = "Реєстрація успішна" });
    }




    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(login.PhoneNumber))
                return BadRequest(new { message = "Номер телефону обов’язковий" });

            if (string.IsNullOrWhiteSpace(login.Password))
                return BadRequest(new { message = "Пароль обов’язковий" });

            var user = _context.Users.FirstOrDefault(u =>
                u.PhoneNumber == login.PhoneNumber &&
                u.PasswordHash == login.Password);

            if (user == null)
                return Unauthorized(new { message = "Невірний номер або пароль" });

            // Завантажити чати користувача з усіма необхідними включеннями
            var chats = await _context.Chats
    .Where(c => c.ChatUsers.Any(cu => cu.UserId == user.Id)) // користувач у чаті
    .Where(c => c.Messages.Any()) // тільки з повідомленнями
    .Include(c => c.ChatUsers)
        .ThenInclude(cu => cu.User)
    .Include(c => c.Messages)
    .ThenInclude(m => m.Sender)
    .ToListAsync();


            var chatDtos = chats.Select(chat => new
            {
                chatId = chat.Id,
                isGroup = chat.IsGroup,
                chatName = chat.ChatName,
                participants = chat.ChatUsers.Select(cu => new
                {
                    cu.User.Id,
                    cu.User.DisplayName,
                    cu.User.Username,
                    cu.User.PhoneNumber
                }),
                messages = chat.Messages
            .OrderByDescending(m => m.SentAt)
            .Take(10)
            .OrderBy(m => m.SentAt)
            .Select(m => new
            {
                m.Id,
                m.Text,
                m.SentAt,
                senderId = m.SenderId,
                senderName = m.Sender.DisplayName
            })
            });

            return Ok(new
            {
                message = "Успішний вхід",
                userId = user.Id,
                displayName = user.DisplayName,
                username = user.Username,
                chats = chatDtos
            });

            //return Ok(new
            //{
            //    message = "Успішний вхід",
            //    userId = user.Id,
            //    displayName = user.DisplayName,
            //    username = user.Username,
            //    chats = chatDtos
            //});
        }
        catch (Exception ex)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                message = "Внутрішня помилка сервера",
                error = ex.Message,
                stackTrace = ex.StackTrace
            }));

            return StatusCode(500, new
            {
                message = "Внутрішня помилка сервера",
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }


}

public class LoginDto
{
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterDto
{
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
}
