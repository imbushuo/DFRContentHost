using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace DFRContentHost.ViewModels
{
    public class SystemMediaTransportControlViewModel : ReactiveObject
    {
        private GlobalSystemMediaTransportControlsSession _glbSmtcSession;
        private GlobalSystemMediaTransportControlsSessionManager _glbSmtcMgr;

        private string _mediaTitle;
        public string MediaTitle
        {
            get => _mediaTitle;
            set => this.RaiseAndSetIfChanged(ref _mediaTitle, value);
        }

        private string _mediaArtist;
        public string MediaArtist
        {
            get => _mediaArtist;
            set => this.RaiseAndSetIfChanged(ref _mediaArtist, value);
        }

        private Bitmap _bitmap;
        public Bitmap MediaThumbnail
        {
            get => _bitmap;
            set => this.RaiseAndSetIfChanged(ref _bitmap, value);
        }

        public SystemMediaTransportControlViewModel()
        {
            InitializeSmtcAsync();
        }

        private async void InitializeSmtcAsync()
        {
            _glbSmtcMgr = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            _glbSmtcSession = _glbSmtcMgr.GetCurrentSession();
            if (_glbSmtcSession != null)
            {
                _glbSmtcSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                await LoadMediaPropertiesAsync();
            }

            _glbSmtcMgr.CurrentSessionChanged += OnSmtcSessionChanged;
        }

        private async void OnSmtcSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender,
            CurrentSessionChangedEventArgs args)
        {
            if (_glbSmtcSession != null)
            {
                _glbSmtcSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            }

            _glbSmtcSession = _glbSmtcMgr.GetCurrentSession();
            if (_glbSmtcSession != null)
            {
                _glbSmtcSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                await LoadMediaPropertiesAsync();
            }
        }

        private async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            await LoadMediaPropertiesAsync();
        }

        private async Task LoadMediaPropertiesAsync()
        {
            try
            {
                var prop = await _glbSmtcSession.TryGetMediaPropertiesAsync();
                MediaTitle = prop.Title;
                MediaArtist = prop.Artist;

                if (prop.Thumbnail != null)
                {
                    using (var thumbnailRuntimeStream = await prop.Thumbnail.OpenReadAsync())
                    using (var thumbnailStream = thumbnailRuntimeStream.AsStreamForRead())
                    {
                        MediaThumbnail = new Bitmap(thumbnailStream);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}
