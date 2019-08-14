using System.Runtime.InteropServices;

namespace DFRContentHost.Interop
{
    static class NativeMethods
    {
        [DllImport("user32")]
        public static extern void LockWorkStation();
    }
}
