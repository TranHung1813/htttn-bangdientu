using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Serilog;

namespace Display
{
    public partial class Page_VideoScreen : UserControl
    {
        private LibVLC _libVLC;
        private MediaPlayer _mp;
        private string _VideoUrl = "";
        private string PathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Files");
        private string _FileName = "";

        public string ScheduleID_Video = "";
        public int _Priority_Video = 1000;
        public bool _is_VideoAvailable = false;

        public const int MAXVALUE = 1000 * 1000 * 1000;

        public Page_VideoScreen()
        {
            InitializeComponent();
            //Init_VLC_Library();

            //_VideoURL = "https://dev-data.radiotech.vn/media/Yêu dấu theo gió bay.mp4";
            //_VideoURL = "https://dev-data.radiotech.vn/media/041d3e6b-29af-4dbc-9ee1-94655ddec328.mp4";
            //_VideoURL = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4";
        }

        private void Init_VLC_Library()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            if(_mp != null)
            {
                try
                {
                    videoView1.Visible = false;
                    videoView1.MediaPlayer = null;
                    _mp?.Stop();
                    _mp.EncounteredError -= _mp_EncounteredError;
                    _mp.EndReached -= _mp_EndReached;
                    _mp.Playing -= _mp_Playing;
                    // _mp = null;
                    //_mp.Dispose();
                }
                catch { }
            }
            _mp = new MediaPlayer(_libVLC);
            _mp.AspectRatio = ((int)Screen.PrimaryScreen.Bounds.Size.Width).ToString() + ":" + ((int)Screen.PrimaryScreen.Bounds.Size.Height).ToString();
            videoView1.MediaPlayer = _mp;
            _mp.EncounteredError += _mp_EncounteredError;
            _mp.EndReached += _mp_EndReached;
            videoView1.Visible = true;
        }

        private void _mp_EndReached(object sender, EventArgs e)
        {
            Log.Information("Video EndReached, Id: {A}", ScheduleID_Video);
            videoView1.Visible = false;
            _is_VideoAvailable = false;
        }

        private void _mp_EncounteredError(object sender, EventArgs e)
        {
            Log.Error("_mp_EncounteredError : {A}", e.ToString());
        }
        public void GetScheduleInfo(ref string ScheduleID, ref string PlayingFile, ref int PlayState, ref bool IsSpkOn, ref int Volume)
        {
            if (_mp == null) return;
            ScheduleID = ScheduleID_Video;
            PlayingFile = _VideoUrl;
            switch (_mp.State)
            {
                case VLCState.NothingSpecial:
                    PlayState = 0;
                    break;
                case VLCState.Opening:
                    PlayState = 1;
                    break;
                case VLCState.Buffering:
                    PlayState = 2;
                    break;
                case VLCState.Playing:
                    PlayState = 3;
                    break;
                case VLCState.Stopped:
                    PlayState = 4;
                    break;
                case VLCState.Error:
                    PlayState = 5;
                    break;
            }
            IsSpkOn = !_mp.Mute;
            Volume = _mp.Volume;
        }
        public void SetVolume(int Value)
        {
            if (_mp == null) return;
            try
            {
                Log.Information("Set Volume: {A}", Value);
                _mp.Volume = Value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SetVolume");
            }
        }
        public void ShowVideo(string url, string ScheduleID, int Priority = 0, int StartPos = 0)
        {
            _VideoUrl = url;
            ScheduleID_Video = ScheduleID;
            _Priority_Video = Priority;
            _is_VideoAvailable = true;

            List<DataUser_SavedFiles> SavedFiles = SqLiteDataAccess.Load_SavedFiles_Info();

            if (SavedFiles != null)
            {
                // Kiem tra xem File da download chua, neu roi thi Play
                int index = SavedFiles.FindIndex(s => (s.ScheduleId == ScheduleID) && (s.Link == url));
                if (index != -1)
                {
                    if (File.Exists(SavedFiles[index].PathLocation))
                    {
                        PlayVideo(SavedFiles[index].PathLocation, StartPos);
                    }
                    else
                    {
                        PlayVideo(url, StartPos);
                    }
                }
                else
                {
                    // Neu chua download thi play link nhu binh thuong
                    PlayVideo(url, StartPos);
                }
            }
            else
            {
                // Neu chua download thi play link nhu binh thuong
                PlayVideo(url, StartPos);
            }
        }

        private async void PlayVideo(string url, int StartPos = 0)
        {
            Log.Information("PlayVideo: {A}, StartPosition: {B} seconds", url, StartPos);
            string[] @params = new string[] { "input-repeat=0" , "start-time=" + StartPos.ToString() };//, "run-time=5" };
            //string[] mediaOptions = { };

            // Stop Media
            Task PlayVideo = Task.Run(() =>
            {
                try
                {
                    Init_VLC_Library();
                    _mp.Play(new Media(_libVLC, new Uri(url), @params));
                    _mp.Playing += _mp_Playing;

                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PlayMedia_Fail: {A}", url);
                }
            });

            await PlayVideo;
            await Task.Delay(5000);
            if (_is_VideoAvailable == true)
            {
                if (_mp.State != VLCState.Playing)
                {
                    Log.Error("PlayMedia_Fail: {A}, TimeWait: {B}ms, VLC_State: {C}", url, 5000, _mp.State);
                    //Log.Information("Retry Play Video 1 time!");
                    //await PlayVideo;
                }
            }
        }

        private void _mp_Playing(object sender, EventArgs e)
        {
            long VideoLength = _mp.Length;
            Log.Information("PlayMedia_Succeeded: {A}, length: {B} ms", _VideoUrl, VideoLength);
            if (VideoLength <= 0)
            {
                // Video Stream co length = 0;
            }
            else
            {
                // Link Video khong Stream co length > 0
                List<DataUser_SavedFiles> SavedFiles = SqLiteDataAccess.Load_SavedFiles_Info();

                if (SavedFiles != null)
                {
                    // Kiem tra xem File da download chua, neu roi thi khong can Download
                    int index = SavedFiles.FindIndex(s => (s.ScheduleId == ScheduleID_Video) && (s.Link == _VideoUrl));
                    if (index != -1)
                    {
                        // Da Download
                        if (File.Exists(SavedFiles[index].PathLocation))
                        {
                            
                        }
                        else
                        {
                            // Neu chua download thi Download
                            //DownloadAsync(_VideoUrl, ScheduleID_Video);
                        }
                    }
                    else
                    {
                        // Neu chua download thi Download
                        //DownloadAsync(_VideoUrl, ScheduleID_Video);
                    }
                }
                else
                {
                    // Neu chua download thi Download
                    //DownloadAsync(_VideoUrl, ScheduleID_Video);
                }
            }

            try
            {
                _mp.Playing -= _mp_Playing;
            }
            catch { }
        }
        private void DownloadAsync(string Url, string ScheduleId)
        {
            string fileExtension = "";
            Uri uri = new Uri(Url);
            try
            {
                fileExtension = Path.GetExtension(uri.LocalPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Path_GetExtension: {Url}", Url);
            }
            _FileName = Path.Combine(PathFile, "SaveVideo-" + ScheduleId + fileExtension);
        }
        
        public void StopVideo()
        {
            Task.Run(() =>
            {
                try
                {
                    //videoView1.Visible = false;
                    videoView1.MediaPlayer = null;
                    _mp.Stop();
                    _mp.EncounteredError -= _mp_EncounteredError;
                    _mp.EndReached -= _mp_EndReached;
                    _mp.Playing -= _mp_Playing;
                    //_mp = null;
                }
                catch { }
            });
            //videoView1.Visible = true;
            _is_VideoAvailable = false;
            _Priority_Video = 1000;
        }
    }
}
