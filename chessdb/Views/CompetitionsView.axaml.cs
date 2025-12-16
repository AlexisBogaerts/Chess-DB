using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace chessdb.Views
{
    public partial class CompetitionsView : UserControl
    {
        public CompetitionsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}