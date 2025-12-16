using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace chessdb.Models;

public partial class Registration : ObservableObject
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    public int CompetitionId { get; set; }
    public Competition? Competition { get; set; }

    [ObservableProperty]
    private DateTime registeredAt = DateTime.UtcNow;
}
