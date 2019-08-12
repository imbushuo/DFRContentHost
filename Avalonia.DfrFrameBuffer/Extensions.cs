using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace Avalonia.DfrFrameBuffer
{
    public static class BridgeFramebufferPlatformExtensions
    {
        class TokenClosable : ICloseable
        {
            public event EventHandler Closed;

            public TokenClosable(CancellationToken token)
            {
                token.Register(() => Dispatcher.UIThread.Post(() => Closed?.Invoke(this, new EventArgs())));
            }
        }

        public static void InitializeWithBridgeFramebuffer<T>(this T builder, Action<TopLevel> setup,
            CancellationToken stop = default)
            where T : AppBuilderBase<T>, new()
        {
            setup(BridgeFrameBufferPlatform.Initialize(builder));
            builder.BeforeStartCallback(builder);
            builder.Instance.Run(new TokenClosable(stop));
        }
    }
}
