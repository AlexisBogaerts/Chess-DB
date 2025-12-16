using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using chessdb.Models;
using Microsoft.EntityFrameworkCore;

namespace chessdb.ViewModels;

public partial class CompetitionsViewModel : ViewModelBase
{
    private readonly ChessFedDbContext _db;

    public ObservableCollection<Competition> Competitions { get; } = new();
    public ObservableCollection<Player> AvailablePlayers { get; } = new();
    public ObservableCollection<Player> RegisteredPlayers { get; } = new();

    [ObservableProperty]
    private Competition? selectedCompetition;

    [ObservableProperty]
    private Player? selectedAvailablePlayer;

    [ObservableProperty]
    private Player? selectedRegisteredPlayer;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(7);

    public CompetitionsViewModel(ChessFedDbContext db)
    {
        _db = db;
        _ = LoadCompetitionsAsync();
        _ = LoadPlayersAsync();
    }

    public async Task LoadCompetitionsAsync()
    {
        var comps = await _db.Competitions
            .Include(c => c.Registrations)
            .ThenInclude(r => r.Player)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
        
        Competitions.Clear();
        foreach (var c in comps) Competitions.Add(c);
    }

    private async Task LoadPlayersAsync()
    {
        var players = await _db.Players.OrderBy(p => p.LastName).ToListAsync();
        AvailablePlayers.Clear();
        foreach (var p in players) AvailablePlayers.Add(p);
    }

    partial void OnSelectedCompetitionChanged(Competition? value)
    {
        if (value != null)
        {
            _ = LoadRegisteredPlayersAsync();
        }
    }

    private async Task LoadRegisteredPlayersAsync()
    {
        if (SelectedCompetition == null) return;

        var registrations = await _db.Registrations
            .Include(r => r.Player)
            .Where(r => r.CompetitionId == SelectedCompetition.Id)
            .ToListAsync();

        RegisteredPlayers.Clear();
        foreach (var reg in registrations)
        {
            if (reg.Player != null)
                RegisteredPlayers.Add(reg.Player);
        }
    }

    [RelayCommand]
    private void NewCompetition()
    {
        ClearForm();
        IsEditing = true;
    }

    [RelayCommand]
    private void EditCompetition()
    {
        if (SelectedCompetition == null) return;

        Name = SelectedCompetition.Name;
        StartDate = SelectedCompetition.StartDate;
        EndDate = SelectedCompetition.EndDate;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveCompetitionAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        if (SelectedCompetition != null && Competitions.Contains(SelectedCompetition))
        {
            SelectedCompetition.Name = Name;
            SelectedCompetition.StartDate = StartDate;
            SelectedCompetition.EndDate = EndDate;
            _db.Competitions.Update(SelectedCompetition);
        }
        else
        {
            var newComp = new Competition
            {
                Name = Name,
                StartDate = StartDate,
                EndDate = EndDate
            };
            _db.Competitions.Add(newComp);
            Competitions.Add(newComp);
        }

        await _db.SaveChangesAsync();
        await LoadCompetitionsAsync();
        CancelEdit();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        ClearForm();
    }

    [RelayCommand]
    private async Task DeleteCompetitionAsync()
    {
        if (SelectedCompetition == null) return;

        _db.Competitions.Remove(SelectedCompetition);
        await _db.SaveChangesAsync();
        Competitions.Remove(SelectedCompetition);
        SelectedCompetition = null;
    }

    [RelayCommand]
    private async Task RegisterPlayerAsync()
    {
        if (SelectedCompetition == null || SelectedAvailablePlayer == null) return;

        // Vérifier si déjà inscrit
        var exists = await _db.Registrations.AnyAsync(r =>
            r.CompetitionId == SelectedCompetition.Id &&
            r.PlayerId == SelectedAvailablePlayer.Id);

        if (!exists)
        {
            var registration = new Registration
            {
                CompetitionId = SelectedCompetition.Id,
                PlayerId = SelectedAvailablePlayer.Id,
                RegisteredAt = DateTime.UtcNow
            };
            _db.Registrations.Add(registration);
            await _db.SaveChangesAsync();
            await LoadRegisteredPlayersAsync();
        }
    }

    [RelayCommand]
    private async Task UnregisterPlayerAsync()
    {
        if (SelectedCompetition == null || SelectedRegisteredPlayer == null) return;

        var registration = await _db.Registrations.FirstOrDefaultAsync(r =>
            r.CompetitionId == SelectedCompetition.Id &&
            r.PlayerId == SelectedRegisteredPlayer.Id);

        if (registration != null)
        {
            _db.Registrations.Remove(registration);
            await _db.SaveChangesAsync();
            await LoadRegisteredPlayersAsync();
        }
    }

    private void ClearForm()
    {
        Name = "";
        StartDate = DateTime.Today;
        EndDate = DateTime.Today.AddDays(7);
        SelectedCompetition = null;
    }
}