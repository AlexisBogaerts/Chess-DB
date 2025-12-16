using Avalonia;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using chessdb.ViewModels;
using chessdb.Models;

namespace chessdb;

sealed class Program
{
    public static ServiceProvider? ServiceProvider { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        // Configure services before starting the app
        ConfigureServices();
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configure DbContext with SQLite
        services.AddDbContext<ChessFedDbContext>(options =>
            options.UseSqlite("Data Source=chessfed.db"));

        // Register repositories
        services.AddScoped<IPlayerRepository, PlayerRepository>();

        // Register services
        services.AddSingleton<IRankingCalculator, EloCalculator>();

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PlayersViewModel>();
        services.AddTransient<CompetitionsViewModel>();
        services.AddTransient<GamesViewModel>();
        services.AddTransient<StatisticsViewModel>();

        ServiceProvider = services.BuildServiceProvider();

        // Ensure database is created and seed initial data
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChessFedDbContext>();
            db.Database.EnsureCreated();
            
            // Seed sample data if database is empty
            if (!db.Players.Any())
            {
                SeedSampleData(db);
            }
        }
    }

    private static void SeedSampleData(ChessFedDbContext db)
    {
        // Create players
        var players = new[]
        {
            new Player 
            { 
                FirstName = "Magnus", 
                LastName = "Carlsen", 
                Email = "magnus@chess.com", 
                Phone = "+47 123 456 78",
                Elo = 2850,
                DateOfBirth = new DateTime(1990, 11, 30)
            },
            new Player 
            { 
                FirstName = "Hikaru", 
                LastName = "Nakamura", 
                Email = "hikaru@chess.com",
                Phone = "+1 555 123 456", 
                Elo = 2800,
                DateOfBirth = new DateTime(1987, 12, 9)
            },
            new Player 
            { 
                FirstName = "Fabiano", 
                LastName = "Caruana", 
                Email = "fabiano@chess.com",
                Phone = "+1 555 789 012", 
                Elo = 2820,
                DateOfBirth = new DateTime(1992, 7, 30)
            },
            new Player 
            { 
                FirstName = "Ding", 
                LastName = "Liren", 
                Email = "ding@chess.com",
                Phone = "+86 555 321 654", 
                Elo = 2810,
                DateOfBirth = new DateTime(1992, 10, 24)
            },
            new Player 
            { 
                FirstName = "Ian", 
                LastName = "Nepomniachtchi", 
                Email = "ian@chess.com",
                Phone = "+7 555 987 654", 
                Elo = 2795,
                DateOfBirth = new DateTime(1990, 7, 14)
            },
            new Player 
            { 
                FirstName = "Alireza", 
                LastName = "Firouzja", 
                Email = "alireza@chess.com",
                Phone = "+33 555 456 789", 
                Elo = 2785,
                DateOfBirth = new DateTime(2003, 6, 18)
            }
        };
        db.Players.AddRange(players);
        db.SaveChanges();

        // Create competitions
        var competition1 = new Competition
        {
            Name = "Championnat de Belgique 2025",
            StartDate = new DateTime(2025, 1, 15),
            EndDate = new DateTime(2025, 1, 22)
        };
        var competition2 = new Competition
        {
            Name = "Tournoi de Bruxelles - Hiver",
            StartDate = new DateTime(2024, 12, 1),
            EndDate = new DateTime(2024, 12, 8)
        };
        db.Competitions.AddRange(competition1, competition2);
        db.SaveChanges();

        // Register players to competitions
        var registrations = new[]
        {
            new Registration { PlayerId = players[0].Id, CompetitionId = competition1.Id },
            new Registration { PlayerId = players[1].Id, CompetitionId = competition1.Id },
            new Registration { PlayerId = players[2].Id, CompetitionId = competition1.Id },
            new Registration { PlayerId = players[3].Id, CompetitionId = competition1.Id },
            new Registration { PlayerId = players[0].Id, CompetitionId = competition2.Id },
            new Registration { PlayerId = players[1].Id, CompetitionId = competition2.Id },
            new Registration { PlayerId = players[4].Id, CompetitionId = competition2.Id },
            new Registration { PlayerId = players[5].Id, CompetitionId = competition2.Id }
        };
        db.Registrations.AddRange(registrations);
        db.SaveChanges();

        // Create some games
        var games = new[]
        {
            new Game
            {
                CompetitionId = competition2.Id,
                WhiteId = players[0].Id,
                BlackId = players[1].Id,
                WhiteScore = 1.0,
                MovesPgn = "1.e4 e5 2.Nf3 Nc6 3.Bb5 a6 4.Ba4 Nf6 5.O-O Be7 6.Re1 b5 7.Bb3 d6 8.c3 O-O 9.h3",
                PlayedAt = new DateTime(2024, 12, 2, 14, 30, 0)
            },
            new Game
            {
                CompetitionId = competition2.Id,
                WhiteId = players[4].Id,
                BlackId = players[5].Id,
                WhiteScore = 0.5,
                MovesPgn = "1.d4 Nf6 2.c4 e6 3.Nf3 d5 4.Nc3 Be7 5.Bg5 h6 6.Bh4 O-O 7.e3 Ne4",
                PlayedAt = new DateTime(2024, 12, 3, 10, 0, 0)
            },
            new Game
            {
                CompetitionId = competition2.Id,
                WhiteId = players[1].Id,
                BlackId = players[4].Id,
                WhiteScore = 1.0,
                MovesPgn = "1.e4 c5 2.Nf3 d6 3.d4 cxd4 4.Nxd4 Nf6 5.Nc3 a6 6.Be3 e5 7.Nb3 Be6",
                PlayedAt = new DateTime(2024, 12, 4, 15, 30, 0)
            },
            new Game
            {
                CompetitionId = competition2.Id,
                WhiteId = players[0].Id,
                BlackId = players[5].Id,
                WhiteScore = 1.0,
                MovesPgn = "1.e4 e5 2.Nf3 Nc6 3.Bc4 Bc5 4.c3 Nf6 5.d4 exd4 6.cxd4 Bb4+ 7.Bd2",
                PlayedAt = new DateTime(2024, 12, 5, 11, 0, 0)
            }
        };
        db.Games.AddRange(games);
        db.SaveChanges();

        Console.WriteLine("✅ Base de données initialisée avec des données d'exemple");
        Console.WriteLine($"   - {players.Length} joueurs créés");
        Console.WriteLine($"   - {2} compétitions créées");
        Console.WriteLine($"   - {registrations.Length} inscriptions");
        Console.WriteLine($"   - {games.Length} parties jouées");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}