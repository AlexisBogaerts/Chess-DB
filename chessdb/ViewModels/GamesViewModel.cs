using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using chessdb.Models;
using Microsoft.EntityFrameworkCore;

namespace chessdb.ViewModels;

public partial class GamesViewModel : ViewModelBase
{
    private readonly ChessFedDbContext _db;
    private readonly IRankingCalculator _eloCalculator;

    public ObservableCollection<Game> Games { get; } = new();
    public ObservableCollection<Competition> Competitions { get; } = new();
    public ObservableCollection<Player> Players { get; } = new();

    [ObservableProperty]
    private Game? selectedGame;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private Competition? selectedCompetition;

    [ObservableProperty]
    private Player? whitePlayer;

    [ObservableProperty]
    private Player? blackPlayer;

    [ObservableProperty]
    private string result = "1-0";

    [ObservableProperty]
    private string movesPgn = "";

    [ObservableProperty]
    private DateTime playedAt = DateTime.Now;

    public ObservableCollection<string> ResultOptions { get; } = new() { "1-0", "0-1", "1/2-1/2" };

    public GamesViewModel(ChessFedDbContext db, IRankingCalculator eloCalculator)
    {
        _db = db;
        _eloCalculator = eloCalculator;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadGamesAsync();
        await LoadCompetitionsAsync();
        await LoadPlayersAsync();
    }

    public async Task LoadGamesAsync()
    {
        var games = await _db.Games
            .Include(g => g.Competition)
            .Include(g => g.White)
            .Include(g => g.Black)
            .OrderByDescending(g => g.PlayedAt)
            .ToListAsync();

        Games.Clear();
        foreach (var g in games) Games.Add(g);
    }

    private async Task LoadCompetitionsAsync()
    {
        var comps = await _db.Competitions.OrderBy(c => c.Name).ToListAsync();
        Competitions.Clear();
        foreach (var c in comps) Competitions.Add(c);
    }

    private async Task LoadPlayersAsync()
    {
        var players = await _db.Players.OrderBy(p => p.LastName).ToListAsync();
        Players.Clear();
        foreach (var p in players) Players.Add(p);
    }

    [RelayCommand]
    private void NewGame()
    {
        ClearForm();
        IsEditing = true;
    }

    [RelayCommand]
    private void EditGame()
    {
        if (SelectedGame == null) return;

        SelectedCompetition = Competitions.FirstOrDefault(c => c.Id == SelectedGame.CompetitionId);
        WhitePlayer = Players.FirstOrDefault(p => p.Id == SelectedGame.WhiteId);
        BlackPlayer = Players.FirstOrDefault(p => p.Id == SelectedGame.BlackId);
        Result = SelectedGame.WhiteScore == 1.0 ? "1-0" : 
                 SelectedGame.WhiteScore == 0.0 ? "0-1" : "1/2-1/2";
        MovesPgn = SelectedGame.MovesPgn;
        PlayedAt = SelectedGame.PlayedAt;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveGameAsync()
    {
        if (SelectedCompetition == null || WhitePlayer == null || BlackPlayer == null)
            return;

        if (WhitePlayer.Id == BlackPlayer.Id)
            return; // Un joueur ne peut pas jouer contre lui-même

        double whiteScore = Result switch
        {
            "1-0" => 1.0,
            "0-1" => 0.0,
            _ => 0.5
        };

        if (SelectedGame != null && Games.Contains(SelectedGame))
        {
            // Mise à jour
            SelectedGame.CompetitionId = SelectedCompetition.Id;
            SelectedGame.WhiteId = WhitePlayer.Id;
            SelectedGame.BlackId = BlackPlayer.Id;
            SelectedGame.WhiteScore = whiteScore;
            SelectedGame.MovesPgn = MovesPgn;
            SelectedGame.PlayedAt = PlayedAt;
            _db.Games.Update(SelectedGame);
        }
        else
        {
            // Nouvelle partie
            var newGame = new Game
            {
                CompetitionId = SelectedCompetition.Id,
                WhiteId = WhitePlayer.Id,
                BlackId = BlackPlayer.Id,
                WhiteScore = whiteScore,
                MovesPgn = MovesPgn,
                PlayedAt = PlayedAt
            };
            _db.Games.Add(newGame);
        }

        // Calculer les nouveaux ELO
        var (newWhiteElo, newBlackElo) = _eloCalculator.Calculate(
            WhitePlayer.Elo,
            BlackPlayer.Elo,
            whiteScore
        );

        WhitePlayer.Elo = newWhiteElo;
        BlackPlayer.Elo = newBlackElo;

        _db.Players.UpdateRange(WhitePlayer, BlackPlayer);
        await _db.SaveChangesAsync();
        
        // IMPORTANT: Recharger les listes pour avoir les nouvelles valeurs
        await LoadGamesAsync();
        await LoadPlayersAsync();  // Pour mettre à jour les ELO affichés
        
        CancelEdit();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        ClearForm();
    }

    [RelayCommand]
    private async Task DeleteGameAsync()
    {
        if (SelectedGame == null) return;

        _db.Games.Remove(SelectedGame);
        await _db.SaveChangesAsync();
        Games.Remove(SelectedGame);
        SelectedGame = null;
    }

    private void ClearForm()
    {
        SelectedCompetition = null;
        WhitePlayer = null;
        BlackPlayer = null;
        Result = "1-0";
        MovesPgn = "";
        PlayedAt = DateTime.Now;
        SelectedGame = null;
    }
}