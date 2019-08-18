using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DFRContentHost.Internal
{
    public class DebugHostedWindow : Window
    {
        public DebugHostedWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
