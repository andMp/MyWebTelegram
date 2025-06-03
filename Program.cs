using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MyWebTelegram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Підключення до SQL Server з appsettings.json
            builder.Services.AddDbContext<TelegramDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.Configure<AzureSpeechOptions>(builder.Configuration.GetSection("AzureSpeech"));

            // Додати контролери, Swagger тощо
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Запуск міграцій під час запуску застосунку
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TelegramDbContext>();
                dbContext.Database.Migrate(); // Автоматично створить або оновить базу
            }

            // Налаштування пайплайну
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles(); // Якщо планується підключення frontend

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        public class AzureSpeechOptions
        {
            public string? Key { get; set; }
            public string? Region { get; set; }
        }
    }
}
