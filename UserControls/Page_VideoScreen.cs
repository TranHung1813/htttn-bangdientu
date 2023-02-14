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
    public partial class Page_VideoScreen : UserControl
    {
        public Page_VideoScreen()
        {
            InitializeComponent();

            //axWindowsMediaPlayer1.URL = "https://dev-data.radiotech.vn/media/Yêu dấu theo gió bay.mp4";
            //axWindowsMediaPlayer1.URL = "https://dev-data.radiotech.vn/media/041d3e6b-29af-4dbc-9ee1-94655ddec328.mp4";
            //axWindowsMediaPlayer1.URL = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4";
            axWindowsMediaPlayer1.Ctlcontrols.play();
            axWindowsMediaPlayer1.settings.setMode("loop", true);
            axWindowsMediaPlayer1.uiMode = "none";

        }

        public void ShowVideo(string url)
        {
            axWindowsMediaPlayer1.URL = url;
            axWindowsMediaPlayer1.URL = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4";
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                if (axWindowsMediaPlayer1.fullScreen == false)
                {
                    //axWindowsMediaPlayer1.fullScreen = true;
                }
            }
        }
    }
}
