using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.DfrFrameBuffer;
using Avalonia.Controls;
using Avalonia.Media;
using System.Linq;
using DFRContentHost.Internal;

namespace DFRContentHost
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            var builder = AppBuilder.Configure<App>().UseReactiveUI();

            if (args.Contains("--self-host"))
            {
                builder.UsePlatformDetect()
                    .UseDirect2D1()
                    .LogToDebug()
                    .Start<DebugHostedWindow>();
            }
            else
            {
                builder.InitializeWithBridgeFramebuffer(tl =>
                {
                    var grid = new Grid();
                    grid.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    grid.Children.Add(new MainView());
                    tl.Content = grid;
                    tl.Width = 1085;
                    tl.Height = 30;
                });
            }
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
                .UseReactiveUI()
                .LogToDebug();
    }
}
