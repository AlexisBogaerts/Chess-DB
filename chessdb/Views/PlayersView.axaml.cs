using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace chessdb.Views
{
    public partial class PlayersView : UserControl
    {
        public PlayersView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
