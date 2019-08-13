using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using WindowsInput;

namespace DFRContentHost
{
    public class MainView : UserControl
    {
        InputSimulator inputSimulator;

        public MainView()
        {
            this.InitializeComponent();
            inputSimulator = new InputSimulator();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnEscClicked(object sender, RoutedEventArgs e)
        {
            inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.ESCAPE);
        }
    }
}
