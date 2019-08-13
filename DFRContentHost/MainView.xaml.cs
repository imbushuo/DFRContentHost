using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;

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
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
        }

        [DllImport("user32")]
        public static extern void LockWorkStation();

        private void OnLockClicked(object sender, RoutedEventArgs e)
        {
            LockWorkStation();
        }

        private void OnVolumeClicked(object sender, RoutedEventArgs e)
        {
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_UP);
        }

        private void OnPlaybackClicked(object sender, RoutedEventArgs e)
        {
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
        }
    }
}
