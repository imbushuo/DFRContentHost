using System;
using System.Runtime.InteropServices;
using Avalonia.DfrFrameBuffer.Interop;

namespace Avalonia.DfrFrameBuffer.Device
{
    static class DfrHostIo
    {
        public const uint IOCTL_DFR_UPDATE_FRAMEBUFFER = 0x8086a004;
        public const uint IOCTL_DFR_CLEAR_FRAMEBUFFER = 0x8086a008;
        public const uint DFR_FRAMEBUFFER_FORMAT = 0x52474241;

        public static bool ClearDfrFrameBuffer(IntPtr deviceHandle)
        {
            return NativeMethods.DeviceIoControl(
                deviceHandle,
                IOCTL_DFR_CLEAR_FRAMEBUFFER,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                IntPtr.Zero
            );
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DFR_HOSTIO_UPDATE_FRAMEBUFFER_HEADER
    {
        public ushort BeginX;
        public ushort BeginY;
        public ushort Width;
        public ushort Height;
        public uint FrameBufferPixelFormat;
        public uint RequireVertFlip;
    }
}
