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
        private string _VideoURL = "";
        public Page_VideoScreen()
        {
            InitializeComponent();

            //_VideoURL = "https://dev-data.radiotech.vn/media/Yêu dấu theo gió bay.mp4";
            //_VideoURL = "https://dev-data.radiotech.vn/media/041d3e6b-29af-4dbc-9ee1-94655ddec328.mp4";
            _VideoURL = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4";
        }

        public void ShowVideo(string url)
        {
            string[] @params = new string[] { "input-repeat=65535" };

            try
            {
                vlcControl1.Play(new Uri(_VideoURL), @params);
            }
            catch
            { }
        }
        public void StopVideo()
        {
            vlcControl1.Stop();
        }
    }
}
