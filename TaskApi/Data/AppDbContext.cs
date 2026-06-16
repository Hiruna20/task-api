using Microsoft.EntityFrameworkCore;
using TaskApi.Models;

namespace TaskApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(120);
            entity.Property(t => t.Description).HasMaxLength(1000);

            entity.Property(t => t.Status).IsRequired()
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<TaskItemStatus>(v, ignoreCase: true)
                );
            entity.Property(t => t.Priority).IsRequired()
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<TaskItemPriority>(v, ignoreCase: true)
                );
        });
    }
}