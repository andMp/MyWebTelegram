using Microsoft.AspNetCore.Mvc;
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
    public IActionResult Login([FromBody] LoginDto login)
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

        return Ok(new { message = "Успішний вхід", userId = user.Id });
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
