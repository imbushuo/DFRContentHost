using Avalonia.Input;
using Avalonia.Input.Raw;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Avalonia.DfrFrameBuffer
{
    public class Digitizer
    {
        private readonly double _width;
        private readonly double _height;

        private HidDevice _digitizer;
        private ReportDescriptor _reportDescr;
        private HidStream _hidStream;

        private HidDeviceInputReceiver _hidDeviceInputReceiver;
        private DeviceItemInputParser _hiddeviceInputParser;

        private const int FingerIdentifierUsage = 852024;
        private const int FingerTapStatusUsage = 852019;
        private const int FingerTapStatus2usage = 852018;

        public event Action<RawInputEventArgs> Event;

        public Digitizer(double width, double height)
        {
            _width = width;
            _height = height;

            // Discover digitizer device
            _digitizer = DeviceList.Local.GetHidDeviceOrNull(0x05ac, 0x8302);
            if (_digitizer == null)
            {
                throw new Exception("iBridge HID digitizer not found");
            }

            _reportDescr = _digitizer.GetReportDescriptor();
        }

        public void Start()
        {
            if (_digitizer.TryOpen(out _hidStream))
            {
                _hidDeviceInputReceiver = _reportDescr.CreateHidDeviceInputReceiver();
                _hiddeviceInputParser = _reportDescr.DeviceItems[0].CreateDeviceItemInputParser();
                _hidDeviceInputReceiver.Received += OnDigitizerInputReceived;
                _hidDeviceInputReceiver.Start(_hidStream);
            }
            else
            {
                throw new Exception("Failed to open iBridge HID digitizer");
            }
        }

        private void OnDigitizerInputReceived(object sender, EventArgs e)
        {
            var inputReportBuffer = new byte[_digitizer.GetMaxInputReportLength()];
            while (_hidDeviceInputReceiver.TryRead(inputReportBuffer, 0, out Report report))
            {
                // Parse the report if possible.
                // This will return false if (for example) the report applies to a different DeviceItem.
                if (_hiddeviceInputParser.TryParseReport(inputReportBuffer, 0, report))
                {
                    BridgeFrameBufferPlatform.Threading.Send(() => ProcessEvent(report));
                }
            }
        }

        private void ProcessEvent(Report report)
        {
            var flagged = false;

            while (_hiddeviceInputParser.HasChanged)
            {
                int changedIndex = _hiddeviceInputParser.GetNextChangedIndex();
                var previousDataValue = _hiddeviceInputParser.GetPreviousValue(changedIndex);
                var dataValue = _hiddeviceInputParser.GetValue(changedIndex);

                if (flagged) continue;

                if ((Usage) dataValue.Usages.FirstOrDefault() == Usage.GenericDesktopX)
                {
                    var x = dataValue.GetPhysicalValue() / 32767 * _width / 2;

                    Event?.Invoke(new RawMouseEventArgs(BridgeFrameBufferPlatform.MouseDevice,
                        BridgeFrameBufferPlatform.Timestamp,
                        BridgeFrameBufferPlatform.TopLevel.InputRoot, 
                        RawMouseEventType.LeftButtonDown, 
                        new Point(x, 15), 
                        default));

                    Event?.Invoke(new RawMouseEventArgs(BridgeFrameBufferPlatform.MouseDevice,
                        BridgeFrameBufferPlatform.Timestamp,
                        BridgeFrameBufferPlatform.TopLevel.InputRoot,
                        RawMouseEventType.LeftButtonUp,
                        new Point(x, 15),
                        default));

                    flagged = false;
                }
            }
        }
    }
}
