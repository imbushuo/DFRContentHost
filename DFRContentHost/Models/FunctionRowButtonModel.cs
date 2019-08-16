using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Reactive;
using WindowsInput;
using WindowsInput.Native;

namespace DFRContentHost.Models
{
    public class FunctionRowButtonModel : ReactiveObject
    {
        private string _keyContent;
        private VirtualKeyCode _keyCode;

        public string Content
        {
            get => _keyContent;
            set => this.RaiseAndSetIfChanged(ref _keyContent, value);
        }

        public VirtualKeyCode Code
        {
            get => _keyCode;
            set => this.RaiseAndSetIfChanged(ref _keyCode, value);
        }

        public ReactiveCommand<Unit, Unit> KeyCommand => ReactiveCommand.Create(SendKeyItem);

        private void SendKeyItem()
        {
            try
            {
                var sim = new InputSimulator();
                sim.Keyboard.KeyPress(_keyCode);
            }
            catch (Exception)
            {
                // ULPI issue
            }
        }

        public FunctionRowButtonModel() { }
        public FunctionRowButtonModel(string content, VirtualKeyCode code)
        {
            _keyContent = content;
            _keyCode = code;
        }
    }
}
