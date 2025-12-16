using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using chessdb.Models;
using Microsoft.EntityFrameworkCore;

namespace chessdb.ViewModels;

public partial class PlayersViewModel : ViewModelBase
{
    private readonly ChessFedDbContext _db;

    public ObservableCollection<Player> Players { get; } = new();

    [ObservableProperty]
    private Player? selectedPlayer;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string searchText = "";

    // Champs pour le formulaire d'édition
    [ObservableProperty]
    private string firstName = "";

    [ObservableProperty]
    private string lastName = "";

    [ObservableProperty]
    private string email = "";

    [ObservableProperty]
    private string phone = "";

    [ObservableProperty]
    private int elo = 1200;

    [ObservableProperty]
    private DateTime dateOfBirth = DateTime.Today.AddYears(-20);

    public PlayersViewModel(ChessFedDbContext db)
    {
        _db = db;
        _ = LoadPlayersAsync();
    }

    public async Task LoadPlayersAsync()
    {
        var players = await _db.Players.OrderByDescending(p => p.Elo).ToListAsync();
        Players.Clear();
        foreach (var p in players) Players.Add(p);
    }

    [RelayCommand]
    private void NewPlayer()
    {
        ClearForm();
        IsEditing = true;
    }

    [RelayCommand]
    private void EditPlayer()
    {
        if (SelectedPlayer == null) return;

        FirstName = SelectedPlayer.FirstName;
        LastName = SelectedPlayer.LastName;
        Email = SelectedPlayer.Email;
        Phone = SelectedPlayer.Phone;
        Elo = SelectedPlayer.Elo;
        DateOfBirth = SelectedPlayer.DateOfBirth;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SavePlayerAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            return;

        if (SelectedPlayer != null && Players.Contains(SelectedPlayer))
        {
            // Mise à jour
            SelectedPlayer.FirstName = FirstName;
            SelectedPlayer.LastName = LastName;
            SelectedPlayer.Email = Email;
            SelectedPlayer.Phone = Phone;
            SelectedPlayer.Elo = Elo;
            SelectedPlayer.DateOfBirth = DateOfBirth;
            _db.Players.Update(SelectedPlayer);
        }
        else
        {
            // Nouveau joueur
            var newPlayer = new Player
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Phone = Phone,
                Elo = Elo,
                DateOfBirth = DateOfBirth
            };
            _db.Players.Add(newPlayer);
            Players.Add(newPlayer);
        }

        await _db.SaveChangesAsync();
        await LoadPlayersAsync();
        CancelEdit();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        ClearForm();
    }

    [RelayCommand]
    private async Task DeletePlayerAsync()
    {
        if (SelectedPlayer == null) return;

        _db.Players.Remove(SelectedPlayer);
        await _db.SaveChangesAsync();
        Players.Remove(SelectedPlayer);
        SelectedPlayer = null;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadPlayersAsync();
            return;
        }

        var search = SearchText.ToLower();
        var filtered = await _db.Players
            .Where(p => p.FirstName.ToLower().Contains(search) ||
                       p.LastName.ToLower().Contains(search) ||
                       p.Email.ToLower().Contains(search))
            .OrderByDescending(p => p.Elo)
            .ToListAsync();

        Players.Clear();
        foreach (var p in filtered) Players.Add(p);
    }

    private void ClearForm()
    {
        FirstName = "";
        LastName = "";
        Email = "";
        Phone = "";
        Elo = 1200;
        DateOfBirth = DateTime.Today.AddYears(-20);
        SelectedPlayer = null;
    }
}
