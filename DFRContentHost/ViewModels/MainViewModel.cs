using ReactiveUI;

namespace DFRContentHost.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public FunctionRowButtonCollectionViewModel FnKeyViewModel { get; }
        public SystemMediaTransportControlViewModel SmtcViewModel { get; }

        public MainViewModel()
        {
            FnKeyViewModel = new FunctionRowButtonCollectionViewModel();
            SmtcViewModel = new SystemMediaTransportControlViewModel();
        }
    }
}
