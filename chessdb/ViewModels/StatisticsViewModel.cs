using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using chessdb.Models;
using Microsoft.EntityFrameworkCore;

namespace chessdb.ViewModels;

public partial class StatisticsViewModel : ViewModelBase
{
    private readonly ChessFedDbContext _db;

    [ObservableProperty]
    private int totalPlayers;

    [ObservableProperty]
    private int totalCompetitions;

    [ObservableProperty]
    private int totalGames;

    [ObservableProperty]
    private int averageElo;

    [ObservableProperty]
    private Player? topPlayer;

    public ObservableCollection<PlayerStatistic> TopPlayers { get; } = new();
    public ObservableCollection<CompetitionStatistic> RecentCompetitions { get; } = new();

    public StatisticsViewModel(ChessFedDbContext db)
    {
        _db = db;
        _ = LoadStatisticsAsync();
    }

    public async Task LoadStatisticsAsync()
    {
        // Statistiques générales
        TotalPlayers = await _db.Players.CountAsync();
        TotalCompetitions = await _db.Competitions.CountAsync();
        TotalGames = await _db.Games.CountAsync();
        
        if (TotalPlayers > 0)
        {
            AverageElo = (int)await _db.Players.AverageAsync(p => p.Elo);
            TopPlayer = await _db.Players.OrderByDescending(p => p.Elo).FirstOrDefaultAsync();
        }

        // Top 10 joueurs
        var topPlayers = await _db.Players
            .OrderByDescending(p => p.Elo)
            .Take(10)
            .ToListAsync();

        TopPlayers.Clear();
        int rank = 1;
        foreach (var player in topPlayers)
        {
            var wins = await _db.Games.CountAsync(g =>
                (g.WhiteId == player.Id && g.WhiteScore == 1.0) ||
                (g.BlackId == player.Id && g.WhiteScore == 0.0));

            var losses = await _db.Games.CountAsync(g =>
                (g.WhiteId == player.Id && g.WhiteScore == 0.0) ||
                (g.BlackId == player.Id && g.WhiteScore == 1.0));

            var draws = await _db.Games.CountAsync(g =>
                (g.WhiteId == player.Id || g.BlackId == player.Id) &&
                g.WhiteScore == 0.5);

            TopPlayers.Add(new PlayerStatistic
            {
                Rank = rank++,
                PlayerName = $"{player.FirstName} {player.LastName}",
                Elo = player.Elo,
                Wins = wins,
                Losses = losses,
                Draws = draws,
                TotalGames = wins + losses + draws
            });
        }

        // Compétitions récentes
        var recentComps = await _db.Competitions
            .Include(c => c.Registrations)
            .Include(c => c.Games)
            .OrderByDescending(c => c.StartDate)
            .Take(5)
            .ToListAsync();

        RecentCompetitions.Clear();
        foreach (var comp in recentComps)
        {
            RecentCompetitions.Add(new CompetitionStatistic
            {
                Name = comp.Name,
                StartDate = comp.StartDate,
                EndDate = comp.EndDate,
                ParticipantCount = comp.Registrations.Count,
                GameCount = comp.Games.Count
            });
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadStatisticsAsync();
    }
}

public class PlayerStatistic
{
    public int Rank { get; set; }
    public string PlayerName { get; set; } = "";
    public int Elo { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int TotalGames { get; set; }
    public string WinRate => TotalGames > 0 ? $"{(Wins * 100.0 / TotalGames):F1}%" : "0%";
}

public class CompetitionStatistic
{
    public string Name { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ParticipantCount { get; set; }
    public int GameCount { get; set; }
}