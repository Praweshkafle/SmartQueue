using Microsoft.EntityFrameworkCore;
using SmartQueue.Domain.Entities;

namespace SmartQueue.Infrastructure.Persistence;

public class AppDbContext :DbContext
{
   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
   
   public DbSet<User> Users => Set<User>();
   public DbSet<Service> Services => Set<Service>();
   public DbSet<Token> Tokens => Set<Token>();
   public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      modelBuilder.Entity<Token>()
         .Property(t => t.Status)
         .HasConversion<string>();

      modelBuilder.Entity<Token>()
         .Property(t => t.RowVersion)
         .IsRowVersion();

      modelBuilder.Entity<Token>()
         .HasIndex(t => t.IdempotencyKey)
         .IsUnique();

      modelBuilder.Entity<Token>()
         .HasIndex(t => new { t.ServiceId, t.Status, t.IssuedAt });
   }
}