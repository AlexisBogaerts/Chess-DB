using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using chessdb.ViewModels;
using System;

namespace chessdb.Views
{
    public partial class StatisticsView : UserControl
    {
        public StatisticsView()
        {
            InitializeComponent();
            Console.WriteLine("📊 StatisticsView créée");
            
            // Afficher le DataContext quand il change
            this.DataContextChanged += (s, e) =>
            {
                Console.WriteLine($"   DataContext changé: {DataContext?.GetType().Name ?? "NULL"}");
                if (DataContext is StatisticsViewModel vm)
                {
                    Console.WriteLine($"   ✅ StatisticsViewModel correctement assigné");
                    Console.WriteLine($"   TotalPlayers: {vm.TotalPlayers}");
                    Console.WriteLine($"   TotalGames: {vm.TotalGames}");
                    Console.WriteLine($"   TopPlayers.Count: {vm.TopPlayers.Count}");
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}