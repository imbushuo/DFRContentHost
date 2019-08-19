using Avalonia.DfrFrameBuffer.Device;
using Avalonia.DfrFrameBuffer.Interop;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Avalonia.DfrFrameBuffer
{
    public class FnKeyNotifier
    {
        private IntPtr _fd;
        private Dispatcher _dispatcher;

        public event Action<RawInputEventArgs> Event;

        public FnKeyNotifier(Dispatcher dispatcher)
        {
            var instancePath = Locator.FindDfrDevice();
            if (instancePath == null)
            {
                throw new Exception("DFR Display Device not found");
            }

            _dispatcher = dispatcher;

            _fd = NativeMethods.CreateFile(
                instancePath, FileAccess.ReadWrite, FileShare.None,
                IntPtr.Zero, FileMode.Open, FileOptions.None,
                IntPtr.Zero
            );

            if (_fd == IntPtr.Zero)
            {
                throw new Exception("Error: " + Marshal.GetLastWin32Error());
            }

            ThreadPool.UnsafeQueueUserWorkItem(_ => PollNextFnStatus(), null);
        }

        public void PollNextFnStatus()
        {
            while (true)
            {
                var ioCtlResult = DfrHostIo.GetNextFnKeyStatus(_fd, out bool pressed);
                _dispatcher?.InvokeAsync(() => ProcessEvent(pressed), DispatcherPriority.Input);

                if (!ioCtlResult)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }

        private void ProcessEvent(bool pressed)
        {
            // We use F23 as the Fn indicator
            Event?.Invoke(new RawKeyEventArgs(
                BridgeFrameBufferPlatform.KbdDevice,
                BridgeFrameBufferPlatform.Timestamp,
                pressed ? RawKeyEventType.KeyDown : RawKeyEventType.KeyUp,
                Key.F23,
                InputModifiers.None));
        }
    }
}
