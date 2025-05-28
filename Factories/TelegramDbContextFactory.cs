using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MyWebTelegram
{
    public class TelegramDbContextFactory : IDesignTimeDbContextFactory<TelegramDbContext>
    {
        public TelegramDbContext CreateDbContext(string[] args)
        {
            // Створюємо конфігурацію з appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Отримуємо рядок підключення
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<TelegramDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new TelegramDbContext(optionsBuilder.Options);
        }
    }
}
