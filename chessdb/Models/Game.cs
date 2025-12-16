using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace chessdb.Models;

public partial class Game : ObservableObject
{
    public int Id { get; set; }

    public int CompetitionId { get; set; }
    public Competition? Competition { get; set; }

    public int WhiteId { get; set; }
    public Player? White { get; set; }

    public int BlackId { get; set; }
    public Player? Black { get; set; }

    [ObservableProperty]
    private double whiteScore;

    public double BlackScore => 1.0 - WhiteScore;

    [ObservableProperty]
    private string movesPgn = "";

    [ObservableProperty]
    private DateTime playedAt = DateTime.UtcNow;
}
