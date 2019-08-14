using DFRContentHost.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using WindowsInput.Native;

namespace DFRContentHost.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public ObservableCollection<FunctionRowButtonModel> FnKeys { get; }

        public MainViewModel()
        {
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
        }
    }
}
