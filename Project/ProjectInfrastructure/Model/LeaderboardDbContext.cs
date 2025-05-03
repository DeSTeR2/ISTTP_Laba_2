using Microsoft.EntityFrameworkCore;

namespace ProjectInfrastructure.Model;

public partial class LeaderboardDbContext : DbContext
{
    public LeaderboardDbContext()
    {
    }

    public LeaderboardDbContext(DbContextOptions<LeaderboardDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.UseSqlServer("Server=Admin\\SQLEXPRESS; Database=LeaderboardDB; Trusted_Connection=True; TrustServerCertificate=True; ");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
