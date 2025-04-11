using Microsoft.EntityFrameworkCore;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChatServer;
internal class ChatDbContext : DbContext
{
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=chat.db");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>().HasKey(m => m.Id);
        modelBuilder.Entity<User>().HasKey(m => m.Id);
    }
}

internal class ChatMessage
{
    public int Id { get; set; }
    public required string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int UserId { get; set; }
    public required string Sender { get; set; }
    public ChatMessagePacket ToPacket()
    {
        return new ChatMessagePacket
        {
            Message = Message,
            Sender = Sender
        };
    }
}

internal class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
}