using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace chessdb.Models;

public partial class Competition : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private DateTime startDate;

    [ObservableProperty]
    private DateTime endDate;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public ICollection<Game> Games { get; set; } = new List<Game>();
}
