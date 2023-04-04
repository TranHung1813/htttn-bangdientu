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
        private string PathFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string _FileName = "";
        Timer tick;
        System.Timers.Timer Duration_Media_Tmr;

        private int _IdleTime = 0;
        private int _LoopNum = MAXVALUE;
        private int _Duration = MAXVALUE;
        private int CountTimeLoop = 0;
        private int IdleTimeCount = 0;

        public const int MAXVALUE = 1000 * 1000 * 1000;

        private bool _EnableLoop = false;
        private bool _isFileDownloadDone = false;

        public Page_VideoScreen()
        {
            InitializeComponent();
            Init_VLC_Library();

            //_VideoURL = "https://dev-data.radiotech.vn/media/Yêu dấu theo gió bay.mp4";
            //_VideoURL = "https://dev-data.radiotech.vn/media/041d3e6b-29af-4dbc-9ee1-94655ddec328.mp4";
            //_VideoURL = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4";
        }

        private void Init_VLC_Library()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mp = new MediaPlayer(_libVLC);
            //_mp.AspectRatio = "4:3";
            videoView1.MediaPlayer = _mp;
            _mp.EncounteredError += _mp_EncounteredError;

            tick = new Timer();
            tick.Interval = 500;
            tick.Tick += Tick_Tick;

            Duration_Media_Tmr = new System.Timers.Timer();
        }

        private void Tick_Tick(object sender, EventArgs e)
        {
            if (_EnableLoop != true) return;
            if (_mp.IsPlaying == false)
            {
                //Số lần Loop kết thúc
                if (CountTimeLoop >= _LoopNum)
                {
                    Task.Run(() =>
                    {
                        videoView1.Visible = false;
                        _mp.Stop();
                        videoView1.Visible = true;
                    });
                    tick.Stop();
                    return;
                }
                // Xu ly Idle Time
                if (IdleTimeCount > 0)
                {
                    if (_mp.State != VLCState.Stopped)
                    {
                        Task.Run(() =>
                        {
                            videoView1.Visible = false;
                            _mp.Stop();
                            videoView1.Visible = true;
                        });
                    }
                    IdleTimeCount -= tick.Interval;
                    return;
                }
                else
                {
                    IdleTimeCount = _IdleTime;
                }
                // Chay video
                if (_isFileDownloadDone == true)
                {
                    long length = new FileInfo(_FileName).Length;
                    if (length > 1.5 * 1024 * 1024)
                    {
                        string[] @params = new string[] { "input-repeat=0" };
                        try
                        {
                            _mp.Play(new Media(_libVLC, new Uri(_FileName), @params));
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "PlayMedia_Fail: {A}", _FileName);
                        }
                    }
                    //tick.Stop();
                }
                else
                {
                    string[] @params = new string[] { "input-repeat=0" };
                    try
                    {
                        _mp.Play(new Media(_libVLC, new Uri(_VideoUrl), @params));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "PlayMedia_Fail: {A}", _VideoUrl);
                    }
                }
                CountTimeLoop++;
            }
        }
        private void _mp_EncounteredError(object sender, EventArgs e)
        {
            Log.Error("_mp_EncounteredError : {A}", e.ToString());
        }
        public int GetVolume()
        {
            if (_mp.IsPlaying != true) return 0;
            return _mp.Volume;
        }
        public void ShowVideo(string url, int IdleTime = 0, int loopNum = MAXVALUE, int Duration = MAXVALUE)
        {
            Log.Information("ShowVideo: {A}", url);
            _VideoUrl = url;
            _IdleTime = IdleTime; //  ms
            CountTimeLoop = 1;
            IdleTimeCount = _IdleTime;
            _LoopNum = loopNum;
            _Duration = Duration; //  ms
            _EnableLoop = false;

            PlayVideo(url);

            // Duration Handle
            if (_LoopNum == 0 && _Duration > 0)
            {
                Duration_Handle(Duration_Media_Tmr, ref Duration_Media_Tmr, Duration, () =>
                {
                    // Stop Media
                    Task.Run(() =>
                    {
                        videoView1.Visible = false;
                        _mp.Stop();
                        videoView1.Visible = true;
                    });
                    _EnableLoop = false;
                    Log.Information("Video Stop!");
                });
                _LoopNum = MAXVALUE;
            }
        }

        private async void PlayVideo(string url)
        {
            string[] @params = new string[] { "input-repeat=0" };//, "run-time=5" };
            //string[] mediaOptions = { };

            // Stop Media
            Task PlayVideo = Task.Run(() =>
            {
                try
                {
                    videoView1.Visible = false;
                    _mp.Stop();
                    videoView1.Visible = true;
                    _mp.Play(new Media(_libVLC, new Uri(url), @params));
                    _mp.Playing += _mp_Playing;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PlayMedia_Fail: {A}", url);
                }
            });

            await PlayVideo;

            tick.Stop();
            tick.Start();
        }

        private void _mp_Playing(object sender, EventArgs e)
        {
            long VideoLength = _mp.Length;
            Log.Information("PlayMedia_Succeeded: {A}, length: {B}", _VideoUrl, VideoLength);
            if (VideoLength <= 0)
            {
                // Video Stream co length = 0;
                _EnableLoop = false;
            }
            else
            {
                // Link Video khong Stream co length > 0
                DownloadAsync(_VideoUrl);
                _EnableLoop = true;
            }

            try
            {
                _mp.Playing -= _mp_Playing;
            }
            catch { }
        }
        private void Duration_Handle(System.Timers.Timer tmr, ref System.Timers.Timer return_tmr, int Duration, Action action)
        {
            try
            {
                tmr.Stop();
                tmr.Dispose();
            }
            catch { }
            tmr = new System.Timers.Timer();
            // Xu ly Duration cho text content
            tmr.Interval = Duration + 1000;
            tmr.Elapsed += (o, ev) =>
            {
                action();
                // Stop this Timer
                tmr.Stop();
                tmr.Dispose();
            };
            tmr.Start();

            return_tmr = tmr;
        }
        private void DownloadAsync(string Url)
        {
            _isFileDownloadDone = false;

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
            _FileName = Path.Combine(PathFile, "SaveVideo" + fileExtension);

            WebClient webClient = new WebClient();
            webClient.DownloadFileAsync(uri, _FileName);
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
        }
        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _isFileDownloadDone = true;
            Log.Information("DownloadFileCompleted: {A}", _VideoUrl);
        }
        public void StopVideo()
        {
            Task.Run(() =>
            {
                videoView1.Visible = false;
                _mp.Stop();
                videoView1.Visible = true;
            });

            tick.Stop();
            Duration_Media_Tmr.Stop();
            Duration_Media_Tmr.Dispose();
            _EnableLoop = false;
            _isFileDownloadDone = false;

            try
            {
                _mp.Playing -= _mp_Playing;
            }
            catch { }
        }
    }
}
