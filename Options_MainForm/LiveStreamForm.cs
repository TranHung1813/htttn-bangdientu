using LibVLCSharp.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class LiveStreamForm : UserControl
    {
        private LibVLC _libVLC;
        private MediaPlayer _mp;
        private string _VideoUrl = "";

        private string _masterImei = "";
        public LiveStreamForm()
        {
            InitializeComponent();
            Init_VLC_Library();
        }

        private void Init_VLC_Library()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mp = new MediaPlayer(_libVLC);
            //_mp.AspectRatio = "4:3";
            videoView1.MediaPlayer = _mp;
            _mp.EncounteredError += _mp_EncounteredError;
        }
        private void _mp_EncounteredError(object sender, EventArgs e)
        {
            Log.Error("_mp_EncounteredError : {A}", e.ToString());
        }

        public void ShowLiveStream(string url, int Volume, string masterImei)
        {
            Log.Information("ShowLiveStream: {A}", url);
            _VideoUrl = url;
            _masterImei = masterImei;

            PlayVideo(url, Volume);
        }

        private async void PlayVideo(string url, int Volume)
        {
            string[] @params = new string[] { "input-repeat=0" };
            try
            {
                _mp.Volume = Volume;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Set Volume LiveStream");
            }

            Task PlayVideo = Task.Run(() =>
            {
                try
                {
                    // Stop Media
                    videoView1.Visible = false;
                    _mp.Stop();
                    videoView1.Visible = true;
                    // Play New
                    _mp.Play(new Media(_libVLC, new Uri(url), @params));
                    _mp.Playing += _mp_Playing;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PlayLiveStream_Fail: {A}", url);
                }
            });

            await PlayVideo;
        }
        private void _mp_Playing(object sender, EventArgs e)
        {
            Log.Information("PlayLiveStream_Succeeded: {A}", _VideoUrl);

            try
            {
                _mp.Playing -= _mp_Playing;
            }
            catch { }
        }

        public void GetStreamInfo(ref string MasterImei, ref string StreamUrl, ref int StreamState, ref bool IsSpkOn, ref int Volume)
        {
            if (_mp == null) return;
            MasterImei = _masterImei;
            StreamUrl = _VideoUrl;
            switch (_mp.State)
            {
                case VLCState.NothingSpecial:
                    StreamState = 0;
                    break;
                case VLCState.Opening:
                    StreamState = 1;
                    break;
                case VLCState.Buffering:
                    StreamState = 2;
                    break;
                case VLCState.Playing:
                    StreamState = 3;
                    break;
                case VLCState.Stopped:
                    StreamState = 4;
                    break;
                case VLCState.Error:
                    StreamState = 5;
                    break;
            }
            IsSpkOn = !_mp.Mute;
            Volume = _mp.Volume;
        }

        public void SetVolume(int value)
        {
            if (_mp == null) return;
            try
            {
                Log.Information("Set Volume: {A}", value);
                _mp.Volume = value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SetVolume");
            }
        }

        public void Close()
        {
            Task.Run(() =>
            {
                videoView1.Visible = false;
                _mp.Stop();
                videoView1.Visible = true;
            });

            try
            {
                _mp.Playing -= _mp_Playing;
            }
            catch { }
        }

        public void LiveStreamForm_FitToContainer(int Height, int Width)
        {
            Utility.FitUserControlToContainer(this, Height, Width);
        }
    }
}
