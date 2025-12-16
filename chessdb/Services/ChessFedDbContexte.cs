using Microsoft.EntityFrameworkCore;
using chessdb.Models; 

public class ChessFedDbContext : DbContext
{
    public ChessFedDbContext(DbContextOptions<ChessFedDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Player>().HasIndex(p => p.Email).IsUnique(false);
        modelBuilder.Entity<Game>()
            .HasOne(g => g.White)
            .WithMany()
            .HasForeignKey(g => g.WhiteId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Black)
            .WithMany()
            .HasForeignKey(g => g.BlackId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}