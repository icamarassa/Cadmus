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
}