using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace Display
{
    public partial class Page_VideoScreen : UserControl
    {
        private string _VideoURL = "";

        private LibVLC _libVLC;
        private MediaPlayer _mp;

        public Page_VideoScreen()
        {
            InitializeComponent();
            Init_VLC_Library();

            //_VideoURL = "https://dev-data.radiotech.vn/media/Yêu dấu theo gió bay.mp4";
            //_VideoURL = "https://dev-data.radiotech.vn/media/041d3e6b-29af-4dbc-9ee1-94655ddec328.mp4";
            _VideoURL = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4";
        }

        private void Init_VLC_Library()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mp = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mp;
        }

        public void ShowVideo(string url)
        {
            string[] @params = new string[] { "input-repeat=65535" };

            try
            {
                _mp.Play(new Media(_libVLC, new Uri(_VideoURL), @params));
            }
            catch
            { }
        }
        public void StopVideo()
        {
            _mp.Stop();
        }
    }
}
