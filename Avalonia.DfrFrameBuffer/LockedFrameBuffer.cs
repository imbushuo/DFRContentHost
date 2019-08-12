using Avalonia.DfrFrameBuffer.Device;
using Avalonia.DfrFrameBuffer.Interop;
using Avalonia.Platform;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Avalonia.DfrFrameBuffer
{
    [StructLayout(LayoutKind.Sequential)]
    struct PixelColor
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }

    unsafe class LockedFrameBuffer : ILockedFramebuffer
    {
        private IntPtr _deviceHandle;

        public LockedFrameBuffer(IntPtr instance, Vector dpi)
        {
            _deviceHandle = instance;
            Dpi = dpi;

            // Do double buffering because the driver has not yet implemented mmap
            Address = Marshal.AllocHGlobal(RowBytes * 60);
        }

        public IntPtr Address { get; private set; }

        public PixelSize Size => new PixelSize(2170, 60);

        public int RowBytes => 2170 * 4;

        public Vector Dpi { get; }

        public PixelFormat Format => PixelFormat.Bgra8888;

        unsafe void VSync()
        {
            // Do transfer here
            int requestSize = 2170 * 60 * 3 + Marshal.SizeOf(typeof(DFR_HOSTIO_UPDATE_FRAMEBUFFER_HEADER));
            IntPtr requestPtr = Marshal.AllocHGlobal(requestSize);
            if (requestPtr == IntPtr.Zero)
            {
                throw new Exception("Failed to allocate memory for FrameBuffer");
            }

            byte* requestBytePtr = (byte*) requestPtr.ToPointer();
            PixelColor* content = (PixelColor*) Address.ToPointer();
            UnmanagedMemoryStream requestStream = new UnmanagedMemoryStream(requestBytePtr, requestSize, requestSize, FileAccess.Write);

            using (var binaryWriter = new BinaryWriter(requestStream))
            {
                binaryWriter.Write((ushort) 0);
                binaryWriter.Write((ushort) 0);
                binaryWriter.Write((ushort) 2170);
                binaryWriter.Write((ushort) 60);
                binaryWriter.Write(DfrHostIo.DFR_FRAMEBUFFER_FORMAT);
                binaryWriter.Write((uint)0);

                for (int w = 0; w < 2170; w++)
                {
                    for (int h = 59; h >= 0; h--)
                    {
                        int offset = 2170 * h + w;

                        byte b = content[offset].Blue;
                        byte g = content[offset].Green;
                        byte r = content[offset].Red;

                        binaryWriter.Write(r);
                        binaryWriter.Write(g);
                        binaryWriter.Write(b);
                    }
                }

                binaryWriter.Flush();

                NativeMethods.DeviceIoControl(
                    _deviceHandle,
                    DfrHostIo.IOCTL_DFR_UPDATE_FRAMEBUFFER,
                    requestPtr,
                    requestSize,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    IntPtr.Zero
                );

            }

            Marshal.FreeHGlobal(requestPtr);
        }

        public void Dispose()
        {
            VSync();

            Marshal.FreeHGlobal(Address);
            Address = IntPtr.Zero;
        }
    }
}
