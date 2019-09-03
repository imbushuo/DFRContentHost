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

        private FunctionRowButtonModel _playPauseButton;

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

        private bool _isSmtcActive;
        public bool IsSmtcActive
        {
            get => _isSmtcActive;
            set => this.RaiseAndSetIfChanged(ref _isSmtcActive, value);
        }

        public ObservableCollection<FunctionRowButtonModel> VolumeKeys { get; }
        public ObservableCollection<FunctionRowButtonModel> PlayControlKeys { get; }

        public SystemMediaTransportControlViewModel()
        {
            _playPauseButton = new FunctionRowButtonModel("\uE102", VirtualKeyCode.MEDIA_PLAY_PAUSE)
            {
                Enabled = false
            };

            PlayControlKeys = new ObservableCollection<FunctionRowButtonModel>
            {
                new FunctionRowButtonModel("\uE100", VirtualKeyCode.MEDIA_PREV_TRACK),
                _playPauseButton,
                new FunctionRowButtonModel("\uE101", VirtualKeyCode.MEDIA_NEXT_TRACK)
            };

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
            _playPauseButton.Enabled = false;
            ThumbnailAvailable = false;
            IsSmtcActive = false;
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
                _glbSmtcSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                await LoadMediaPropertiesAsync();
                UpdatePlayButtonStatus();
                IsSmtcActive = true;
            }

            _glbSmtcMgr.CurrentSessionChanged += OnSmtcSessionChanged;
        }

        private async void OnSmtcSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender,
            CurrentSessionChangedEventArgs args)
        {
            if (_glbSmtcSession != null)
            {
                _glbSmtcSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                _glbSmtcSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
            }

            _glbSmtcSession = _glbSmtcMgr.GetCurrentSession();
            if (_glbSmtcSession != null)
            {
                _glbSmtcSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                _glbSmtcSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                await LoadMediaPropertiesAsync();
                UpdatePlayButtonStatus();
                IsSmtcActive = true;
            }
            else
            {
                ResetSmtc();
            }
        }

        private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            UpdatePlayButtonStatus();
        }

        private async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            await LoadMediaPropertiesAsync();
        }

        private void UpdatePlayButtonStatus()
        {
            if (_glbSmtcSession == null) return;
            try
            {
                var playbackInfo = _glbSmtcSession.GetPlaybackInfo();
                switch (playbackInfo.PlaybackStatus)
                {
                    case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                        _playPauseButton.Enabled = true;
                        _playPauseButton.Content = "\uE103";
                        break;
                    case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
                    case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped:
                        _playPauseButton.Enabled = true;
                        _playPauseButton.Content = "\uE102";
                        break;
                    default:
                        _playPauseButton.Enabled = false;
                        _playPauseButton.Content = "\uE102";
                        break;
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
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
