using Avalonia.DfrFrameBuffer;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using DFRContentHost.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using WindowsInput.Native;

namespace DFRContentHost.ViewModels
{
    public class FunctionRowButtonCollectionViewModel : ReactiveObject
    {
        public ObservableCollection<FunctionRowButtonModel> FnKeys { get; }
        private FnKeyNotifier _fnKeyNotifier;
        private DateTime _lastFnPressedTime;

        private bool _fnVisible;
        public bool FnVisible
        {
            get => _fnVisible;
            set => this.RaiseAndSetIfChanged(ref _fnVisible, value);
        }

        public FunctionRowButtonCollectionViewModel()
        {
            _lastFnPressedTime = DateTime.MinValue;

            FnVisible = false;
            FnKeys = new ObservableCollection<FunctionRowButtonModel>
            {
                new FunctionRowButtonModel("F1", VirtualKeyCode.F1),
                new FunctionRowButtonModel("F2", VirtualKeyCode.F2),
                new FunctionRowButtonModel("F3", VirtualKeyCode.F3),
                new FunctionRowButtonModel("F4", VirtualKeyCode.F4),
                new FunctionRowButtonModel("F5", VirtualKeyCode.F5),
                new FunctionRowButtonModel("F6", VirtualKeyCode.F6),
                new FunctionRowButtonModel("F7", VirtualKeyCode.F7),
                new FunctionRowButtonModel("F8", VirtualKeyCode.F8),
                new FunctionRowButtonModel("F9", VirtualKeyCode.F9),
                new FunctionRowButtonModel("F10", VirtualKeyCode.F10),
                new FunctionRowButtonModel("F11", VirtualKeyCode.F11),
                new FunctionRowButtonModel("F12", VirtualKeyCode.F12)
            };

            _fnKeyNotifier = new FnKeyNotifier(Dispatcher.UIThread);
            _fnKeyNotifier.Event += OnFnKeyStateChanged;
        }

        private void OnFnKeyStateChanged(RawInputEventArgs obj)
        {
            var args = (RawKeyEventArgs) obj;

            if (args.Key == Key.F23 && args.Type == RawKeyEventType.KeyDown)
            {
                if (DateTime.Now.Subtract(_lastFnPressedTime) < TimeSpan.FromMilliseconds(200))
                {
                    FnVisible = !FnVisible;
                }

                _lastFnPressedTime = DateTime.Now;
            }

            obj.Handled = true;
        }
    }
}
