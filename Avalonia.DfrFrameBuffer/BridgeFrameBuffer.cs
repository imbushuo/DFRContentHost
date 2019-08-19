using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.DfrFrameBuffer.Device;
using Avalonia.DfrFrameBuffer.Interop;
using Avalonia.Platform;

namespace Avalonia.DfrFrameBuffer
{
    public class BridgeFrameBuffer : IFramebufferPlatformSurface, IDisposable
    {
        private readonly Vector _dpi;
        private IntPtr _fd;

        public BridgeFrameBuffer(Vector? dpi = null)
        {
            _dpi = dpi ?? new Vector(192, 192);

            var instancePath = Locator.FindDfrDevice();
            if (instancePath == null)
            {
                throw new Exception("DFR Display Device not found");
            }

            _fd = NativeMethods.CreateFile(
                instancePath, FileAccess.Write, FileShare.None,
                IntPtr.Zero, FileMode.Open, FileOptions.None,
                IntPtr.Zero
            );

            if (_fd == IntPtr.Zero)
            {
                throw new Exception("Error: " + Marshal.GetLastWin32Error());
            }

            // At least for now, the DFR display spec is fully known and determined across all devices.
            // Should this changes, issue an IOCTL to retrieve device spec in the future.
        }

        public Vector Dpi => _dpi;

        public ILockedFramebuffer Lock() => new LockedFrameBuffer(_fd, _dpi);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                NativeMethods.CloseHandle(_fd);

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~BridgeFrameBuffer()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
