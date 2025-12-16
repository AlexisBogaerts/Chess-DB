using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using chessdb.Models;
using Microsoft.EntityFrameworkCore;

namespace chessdb.ViewModels;

public partial class PlayersViewModel : ObservableObject
{
    private readonly ChessFedDbContext _db;

    public ObservableCollection<Player> Players { get; } = new();

    [ObservableProperty]
    private Player? selected;

    public PlayersViewModel(ChessFedDbContext db)
    {
        _db = db;
        _ = LoadPlayersAsync();
    }

    public async Task LoadPlayersAsync()
    {
        var players = await _db.Players.ToListAsync();
        Players.Clear();
        foreach (var p in players) Players.Add(p);
    }

    [RelayCommand]
    public async Task AddPlayerAsync(Player p)
    {
        _db.Players.Add(p);
        await _db.SaveChangesAsync();
        Players.Add(p);
    }
}

