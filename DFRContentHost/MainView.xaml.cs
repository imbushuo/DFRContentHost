using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DFRContentHost.Interop;
using DFRContentHost.ViewModels;
using System;
using WindowsInput;
using WindowsInput.Native;

namespace DFRContentHost
{
    public class MainView : UserControl
    {
        private InputSimulator inputSimulator;

        public MainView()
        {
            this.InitializeComponent();
            inputSimulator = new InputSimulator();
            this.DataContext = new MainViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnEscClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
            }
            catch (Exception)
            {
                // ULPI issue
            }
        }

        private void OnLockClicked(object sender, RoutedEventArgs e)
        {
            NativeMethods.LockWorkStation();
        }
    }
}
