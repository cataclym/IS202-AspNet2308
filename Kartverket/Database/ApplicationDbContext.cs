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
    public DbSet<PinnedReport> PinnedReports { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<Reports>()
            .HasOne(r => r.AssignedAdmin)
            .WithMany(u => u.AssignedReports) // You need to add AssignedReports to Users
            .HasForeignKey(r => r.AssignedAdminId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Messages>()
            .HasOne(m => m.User)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.UserId);
        
        // Composite primary key
        modelBuilder.Entity<PinnedReport>()
            .HasKey(pr => new { pr.UserID, pr.ReportID });  

        modelBuilder.Entity<PinnedReport>()
            .HasOne(pr => pr.User)
            .WithMany(u => u.PinnedReports)
            .HasForeignKey(pr => pr.UserID);
        modelBuilder.Entity<PinnedReport>()
            .HasOne(pr => pr.Report)
            .WithMany(r => r.PinnedReports)
            .HasForeignKey(pr => pr.ReportID);
        
        base.OnModelCreating(modelBuilder);
        
        // Definerer MySQL til å bruke CURRENT_TIMESTAMP på databasenivå
        modelBuilder.Entity<Users>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        modelBuilder.Entity<Reports>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        modelBuilder.Entity<Messages>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
