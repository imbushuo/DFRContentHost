using Avalonia.Controls;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Rendering;
using System.Diagnostics;

namespace Avalonia.DfrFrameBuffer
{
    class BridgeFrameBufferPlatform
    {
        BridgeFrameBuffer _fb;

        public static FramebufferToplevelImpl TopLevel;
        public static InternalPlatformThreadingInterface Threading;

        private static readonly Stopwatch m_st = Stopwatch.StartNew();
        internal static uint Timestamp => (uint) m_st.ElapsedTicks;

        public static MouseDevice MouseDevice = new MouseDevice();

        BridgeFrameBufferPlatform()
        {
            _fb = new BridgeFrameBuffer();
        }

        void Initialize()
        {
            Threading = new InternalPlatformThreadingInterface();
            AvaloniaLocator.CurrentMutable
                .Bind<IStandardCursorFactory>().ToTransient<CursorFactoryStub>()
                .Bind<IPlatformSettings>().ToSingleton<PlatformSettings>()
                .Bind<IPlatformThreadingInterface>().ToConstant(Threading)
                .Bind<IRenderLoop>().ToConstant(new RenderLoop())
                .Bind<PlatformHotkeyConfiguration>().ToSingleton<PlatformHotkeyConfiguration>()
                .Bind<IRenderTimer>().ToConstant(Threading);
        }

        internal static TopLevel Initialize<T>(T builder) where T : AppBuilderBase<T>, new()
        {
            var platform = new BridgeFrameBufferPlatform();
            builder.UseSkia()
                .UseWindowingSubsystem(platform.Initialize, "fbdev")
                .SetupWithoutStarting();

            var tl = new EmbeddableControlRoot(TopLevel = new FramebufferToplevelImpl(platform._fb));
            tl.Prepare();

            return tl;
        }
    }
}
