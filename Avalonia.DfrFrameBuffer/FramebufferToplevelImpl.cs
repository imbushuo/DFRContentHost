using System;
using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace Avalonia.DfrFrameBuffer
{
    class FramebufferToplevelImpl : IEmbeddableWindowImpl
    {
        private readonly BridgeFrameBuffer _fb;
        private bool _renderQueued;
        public IInputRoot InputRoot { get; private set; }

        public FramebufferToplevelImpl(BridgeFrameBuffer fb)
        {
            _fb = fb;
            Invalidate(default(Rect));
        }

        public IRenderer CreateRenderer(IRenderRoot root)
        {
            return new ImmediateRenderer(root);
        }

        public void Dispose()
        {
            throw new NotSupportedException();
        }

        
        public void Invalidate(Rect rect)
        {
            if(_renderQueued)
                return;
            _renderQueued = true;
            Dispatcher.UIThread.Post(() =>
            {
                Paint?.Invoke(new Rect(default(Point), ClientSize));
                _renderQueued = false;
            });
        }

        public void SetInputRoot(IInputRoot inputRoot)
        {
            InputRoot = inputRoot;
        }

        public Point PointToClient(PixelPoint p) => p.ToPoint(1);

        public PixelPoint PointToScreen(Point p) => PixelPoint.FromPoint(p, 1);

        public void SetCursor(IPlatformHandle cursor)
        {
        }

        public Size ClientSize => new Size(2170, 60);
        public IMouseDevice MouseDevice => BridgeFrameBufferPlatform.MouseDevice;
        public double Scaling => 1;
        public IEnumerable<object> Surfaces => new object[] {_fb};
        public Action<RawInputEventArgs> Input { get; set; }
        public Action<Rect> Paint { get; set; }
        public Action<Size> Resized { get; set; }
        public Action<double> ScalingChanged { get; set; }
        public Action Closed { get; set; }
        public event Action LostFocus
        {
            add {}
            remove {}
        }
    }
}
