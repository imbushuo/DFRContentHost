using Avalonia.DfrFrameBuffer.Device.Hid;
using Avalonia.Input;
using Avalonia.Input.Raw;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using System;
using System.Linq;

namespace Avalonia.DfrFrameBuffer
{
    public class Digitizer
    {
        private readonly double _scale;
        private readonly double _width;
        private readonly double _height;
        private readonly Vector _dpi;

        private HidDevice _digitizer;
        private ReportDescriptor _reportDescr;
        private HidStream _hidStream;

        private HidDeviceInputReceiver _hidDeviceInputReceiver;
        private DeviceItemInputParser _hiddeviceInputParser;

        // Pre-allocated slots
        private TouchReport[] _prevReports;
        private int _prevTappedSlotIndex;

        public event Action<RawInputEventArgs> Event;

        public Digitizer(double physicalWidth, double physicalHeight, Vector? dpi = null)
        {
            // 96 DPI is the standard 100% scale
            _dpi = dpi ?? new Vector(192, 192);
            _scale = 96 / _dpi.X;
            _width = physicalWidth * _scale;
            _height = physicalHeight * _scale;

            _prevTappedSlotIndex = -1;
            _prevReports = new TouchReport[11];
            for (int i = 0; i < 11; i++)
            {
                _prevReports[i] = new TouchReport(0, 32767, false);
            }

            // Discover DFR digitizer device
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
                    BridgeFrameBufferPlatform.Threading.Send(() => ProcessEvent());
                }
            }
        }

        private void ProcessEvent()
        {
            if (_hiddeviceInputParser.HasChanged)
            {
                int j = -1;
                TouchReport[] currentReports = new TouchReport[11];

                for (int i = 0; i < _hiddeviceInputParser.ValueCount; i++)
                {
                    var data = _hiddeviceInputParser.GetValue(i);
                    if (data.Usages.FirstOrDefault() == VendorUsage.FingerIdentifier)
                    {
                        j++;
                    }
                    else
                    {
                        continue;
                    }

                    // Only 11 slots are statically allocated
                    if (j >= 11) break;

                    // This is defined by the descriptor, we just take the assumption
                    var fingerTapData1 = _hiddeviceInputParser.GetValue(i + 1);
                    var fingerTapData2 = _hiddeviceInputParser.GetValue(i + 2);
                    var xData = _hiddeviceInputParser.GetValue(i + 3);
                    // Y is discarded but being read anyway
                    var yData = _hiddeviceInputParser.GetValue(i + 4);

                    // Register this
                    currentReports[j] = new TouchReport(xData.GetPhysicalValue(), 
                        xData.DataItem.PhysicalMaximum, fingerTapData1.GetPhysicalValue() != 0);
                }

                // Check if need to raise touch leave event for prev slot
                if (_prevTappedSlotIndex >= 0)
                {
                    var scaledX = currentReports[_prevTappedSlotIndex].GetXInPercentage() * _width;

                    if (!currentReports[_prevTappedSlotIndex].FingerStatus)
                    {
                        Event?.Invoke(new RawMouseEventArgs(
                            BridgeFrameBufferPlatform.MouseDevice,
                            BridgeFrameBufferPlatform.Timestamp,
                            BridgeFrameBufferPlatform.TopLevel.InputRoot,
                            RawMouseEventType.LeftButtonUp,
                            new Point(scaledX, _height / 2),
                            default));

                        _prevTappedSlotIndex = -1;
                    }
                    else
                    {
                        // Update cache, raise move event and complete routine
                        if (BridgeFrameBufferPlatform.MouseDevice.Captured != null)
                        {
                            Event?.Invoke(new RawMouseEventArgs(
                                BridgeFrameBufferPlatform.MouseDevice,
                                BridgeFrameBufferPlatform.Timestamp,
                                BridgeFrameBufferPlatform.TopLevel.InputRoot,
                                RawMouseEventType.Move,
                                new Point(scaledX, _height / 2),
                                InputModifiers.LeftMouseButton));
                        }
                    }
                }

                // Can raise new tap event
                if (_prevTappedSlotIndex == -1)
                {
                    for (int i = 0; i < 11; i++)
                    {
                        if (currentReports[i].FingerStatus)
                        {
                            var scaledX = currentReports[i].GetXInPercentage() * _width;
                            Event?.Invoke(new RawMouseEventArgs(
                                BridgeFrameBufferPlatform.MouseDevice,
                                BridgeFrameBufferPlatform.Timestamp,
                                BridgeFrameBufferPlatform.TopLevel.InputRoot,
                                RawMouseEventType.LeftButtonDown,
                                new Point(scaledX, _height / 2),
                               default));

                            _prevTappedSlotIndex = i;
                            break;
                        }
                    }
                }

                // Update cache
                _prevReports = currentReports;
            }
        }
    }
}
