using Avalonia.Media.Imaging;
using DFRContentHost.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsInput.Native;

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

        private bool _thumbnailAvailable;
        public bool ThumbnailAvailable
        {
            get => _thumbnailAvailable;
            set => this.RaiseAndSetIfChanged(ref _thumbnailAvailable, value);
        }

        public ObservableCollection<FunctionRowButtonModel> VolumeKeys { get; }

        public SystemMediaTransportControlViewModel()
        {
            VolumeKeys = new ObservableCollection<FunctionRowButtonModel>
            {
                new FunctionRowButtonModel("\uE74F", VirtualKeyCode.VOLUME_MUTE),
                new FunctionRowButtonModel("\uE993", VirtualKeyCode.VOLUME_DOWN),
                new FunctionRowButtonModel("\uE995", VirtualKeyCode.VOLUME_UP)
            };

            ResetSmtc();
            InitializeSmtcAsync();
        }

        private void ResetSmtc()
        {
            ThumbnailAvailable = false;
            MediaTitle = string.Empty;
            MediaArtist = string.Empty;
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
            else
            {
                ResetSmtc();
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

                    ThumbnailAvailable = true;
                }
                else
                {
                    ThumbnailAvailable = false;
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}
