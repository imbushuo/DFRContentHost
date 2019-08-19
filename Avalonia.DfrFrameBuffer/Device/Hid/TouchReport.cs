using System;

namespace Avalonia.DfrFrameBuffer.Device.Hid
{
    class TouchReport
    {
        public double X { get; }
        public double XMax { get; }
        public bool FingerStatus { get; }

        public TouchReport(double x, double xMax, bool status)
        {
            if (xMax <= 0)
            {
                throw new ArgumentException("xMax must be larger than zero");
            }

            X = x;
            XMax = xMax;
            FingerStatus = status;
        }

        public double GetXInPercentage() => X / XMax;
    }
}
