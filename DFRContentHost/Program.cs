using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.DfrFrameBuffer;
using Avalonia.Controls;
using Avalonia.Media;

namespace DFRContentHost
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            AppBuilder.Configure<App>().InitializeWithBridgeFramebuffer(tl =>
            {
                var grid = new Grid();
                grid.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                grid.Children.Add(new MainView());
                tl.Content = grid;
                tl.Width = 1085;
                tl.Height = 30;
                System.Threading.ThreadPool.QueueUserWorkItem(_ => ConsoleSilencer());
            });
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions
                {
                    AllowEglInitialization = true
                })
                .UseSkia()
                .LogToDebug();

        static void ConsoleSilencer()
        {
            Console.CursorVisible = false;
            Console.ReadLine();
        }
    }
}
