using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace chessdb.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase currentView;

    [ObservableProperty]
    private string currentViewTitle = "Joueurs";

    public ObservableCollection<NavigationItem> NavigationItems { get; } = new();

    private readonly PlayersViewModel _playersViewModel;
    private readonly CompetitionsViewModel _competitionsViewModel;
    private readonly GamesViewModel _gamesViewModel;
    private readonly StatisticsViewModel _statisticsViewModel;

    public MainWindowViewModel(
        PlayersViewModel playersViewModel,
        CompetitionsViewModel competitionsViewModel,
        GamesViewModel gamesViewModel,
        StatisticsViewModel statisticsViewModel)
    {
        _playersViewModel = playersViewModel;
        _competitionsViewModel = competitionsViewModel;
        _gamesViewModel = gamesViewModel;
        _statisticsViewModel = statisticsViewModel;

        // Initialiser les items de navigation
        NavigationItems.Add(new NavigationItem("Joueurs", "👤"));
        NavigationItems.Add(new NavigationItem("Compétitions", "🏆"));
        NavigationItems.Add(new NavigationItem("Parties", "♟️"));
        NavigationItems.Add(new NavigationItem("Statistiques", "📊"));

        // Vue initiale
        currentView = _playersViewModel;
    }

    [RelayCommand]
    private async Task NavigateAsync(string view)
    {
        CurrentView = view switch
        {
            "Joueurs" => _playersViewModel,
            "Compétitions" => _competitionsViewModel,
            "Parties" => _gamesViewModel,
            "Statistiques" => _statisticsViewModel,
            _ => _playersViewModel
        };
        CurrentViewTitle = view;

        // Recharger les données quand on change d'onglet
        if (view == "Statistiques")
        {
            await _statisticsViewModel.LoadStatisticsAsync();
        }
        else if (view == "Parties")
        {
            await _gamesViewModel.LoadGamesAsync();
        }
        else if (view == "Compétitions")
        {
            await _competitionsViewModel.LoadCompetitionsAsync();
        }
        else if (view == "Joueurs")
        {
            await _playersViewModel.LoadPlayersAsync();
        }
    }
}

public class NavigationItem
{
    public string Name { get; set; }
    public string Icon { get; set; }

    public NavigationItem(string name, string icon)
    {
        Name = name;
        Icon = icon;
    }
}