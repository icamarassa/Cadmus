using Cadmus.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cadmus.Api.Data;

public sealed class CadmusDbContext : DbContext
{
    public CadmusDbContext(DbContextOptions<CadmusDbContext> options)
        : base(options)
    {
    }

    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrintJob>(entity =>
        {
            entity.Property(job => job.SourceEventId)
                .HasMaxLength(200);

            entity.HasIndex(job => job.SourceEventId)
                .IsUnique()
                .HasFilter("[SourceEventId] IS NOT NULL");
        });
    }
}