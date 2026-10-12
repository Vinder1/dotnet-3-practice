using ApprovalService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApprovalService.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApprovalRequest>(entity =>
        {
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.RequesterUserId);
        });
    }
}
