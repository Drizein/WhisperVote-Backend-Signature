using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Domain.Entities;

namespace Infrastructure.Persistence;

public class CDbContext : DbContext
{
    private readonly ILogger<CDbContext> _logger = default!;

    public CDbContext()
    {
    }

    public CDbContext(DbContextOptions<CDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettingsMigrations.json", false)
            .Build();
        options.UseMySQL(configuration.GetConnectionString("Default")!);
    }

    public DbSet<UserVote> UserVotes => Set<UserVote>();
    public DbSet<KeyPair> KeyPairs => Set<KeyPair>();

    public async Task<bool> SaveAllChangesAsync()
    {
        _logger?.LogDebug("\n{output}", ChangeTracker.DebugView.LongView);

        var result = await SaveChangesAsync();

        _logger?.LogDebug("SaveChanges {result}", result);
        _logger?.LogDebug("\n{output}", ChangeTracker.DebugView.LongView);
        return result > 0;
    }
}