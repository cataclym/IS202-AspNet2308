using Microsoft.EntityFrameworkCore;
using Kartverket.Database.Models;
using Kartverket.Models;

namespace Kartverket.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Reports> Reports { get; set; }
    public DbSet<Users> Users { get; set; }
    public DbSet<Messages> Messages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<Reports>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reports)  // Assuming Users have a navigation property ICollection<Reports> Reports
            .HasForeignKey(r => r.UserId);
        
        modelBuilder.Entity<Messages>()
            .HasOne(m => m.User)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.UserId);
        
        base.OnModelCreating(modelBuilder);
    }
}
