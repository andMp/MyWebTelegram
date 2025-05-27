using Microsoft.EntityFrameworkCore;
using MyWebTelegram.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using MyWebTelegram.Models;

public class TelegramDbContext : DbContext
{
    public TelegramDbContext(DbContextOptions<TelegramDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatUser> ChatUsers => Set<ChatUser>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChatUser>()
            .HasOne(cu => cu.Chat)
            .WithMany(c => c.ChatUsers)
            .HasForeignKey(cu => cu.ChatId);

        modelBuilder.Entity<ChatUser>()
            .HasOne(cu => cu.User)
            .WithMany()
            .HasForeignKey(cu => cu.UserId);
    }
}
