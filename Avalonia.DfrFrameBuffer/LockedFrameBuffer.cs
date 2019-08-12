using Avalonia.DfrFrameBuffer.Device;
using Avalonia.DfrFrameBuffer.Interop;
using Avalonia.Platform;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Avalonia.DfrFrameBuffer
{
    unsafe class LockedFrameBuffer : ILockedFramebuffer
    {
        private IntPtr _deviceHandle;
        private int _bufferSize;

        public LockedFrameBuffer(IntPtr instance, Vector dpi)
        {
            // Check CPU capability
            if (!Sse2.IsSupported || !Ssse3.IsSupported || !Avx2.IsSupported)
            {
                throw new Exception("Outdated CPU detected");
            }

            _deviceHandle = instance;
            Dpi = dpi;

            // Do double buffering because the driver has not yet implemented mmap
            _bufferSize = RowBytes * 60;
            Address = Marshal.AllocHGlobal(_bufferSize);
        }

        public IntPtr Address { get; private set; }

        public PixelSize Size => new PixelSize(2170, 60);

        public int RowBytes => 2170 * 4;

        public Vector Dpi { get; }

        public PixelFormat Format => PixelFormat.Rgba8888;

        unsafe void VSync()
        {
            int requestSize = 2170 * 60 * 3 + Marshal.SizeOf(typeof(DFR_HOSTIO_UPDATE_FRAMEBUFFER_HEADER));
            IntPtr RequestMemory = Marshal.AllocHGlobal(requestSize);
            if (RequestMemory == IntPtr.Zero)
            {
                throw new Exception("Failed to allocate memory for FrameBuffer");
            }

            byte* pRequest = (byte*) RequestMemory.ToPointer();
            byte* pFbContent = (byte*)Address.ToPointer();

            NativeMethods.ZeroMemory(pRequest, requestSize);
            UnmanagedMemoryStream requestStream = new UnmanagedMemoryStream(pRequest, requestSize, requestSize, FileAccess.Write);
            using (var binaryWriter = new BinaryWriter(requestStream))
            {
                binaryWriter.Write((ushort) 0);
                binaryWriter.Write((ushort) 0);
                binaryWriter.Write((ushort) 2170);
                binaryWriter.Write((ushort) 60);
                binaryWriter.Write(DfrHostIo.DFR_FRAMEBUFFER_FORMAT);
                binaryWriter.Write((uint) 0);
                binaryWriter.Flush();

                for (int w = 0; w < 2170; w++)
                {
                    for (int h = 59; h >= 0; h--)
                    {
                        byte* p = pFbContent + (2170 * h + w) * 4;
                        binaryWriter.Write(*p);
                        binaryWriter.Write(*(p + 1));
                        binaryWriter.Write(*(p + 2));
                    }
                }

                binaryWriter.Flush();
                NativeMethods.DeviceIoControl(
                    _deviceHandle,
                    DfrHostIo.IOCTL_DFR_UPDATE_FRAMEBUFFER,
                    RequestMemory,
                    requestSize,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    IntPtr.Zero
                );

            }

            Marshal.FreeHGlobal(RequestMemory);
        }

        public void Dispose()
        {
            VSync();

            Marshal.FreeHGlobal(Address);
            Address = IntPtr.Zero;
        }
    }
}
