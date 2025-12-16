using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace chessdb.Models;

public partial class Player : ObservableObject
{
    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private int elo = 1200;

    [ObservableProperty]
    private DateTime dateOfBirth;
    
    [ObservableProperty]
    private int id;
}

