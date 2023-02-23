using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Transitions;

namespace Display
{
    public partial class Page_Multi_Image : UserControl
    {
        private PictureBox m_ActivePicture = null;
        private PictureBox m_InactivePicture = null;
        private Random m_Random = new Random();
        private List<Image> imageList = new List<Image>();
        //private ImageList imageList = new ImageList();

        public Page_Multi_Image()
        {
            InitializeComponent();

            m_ActivePicture = pictureBox1;
            m_InactivePicture = pictureBox2;

            //pictureBox1.Load("https://fastly.picsum.photos/id/1026/1200/600.jpg?hmac=JwvbmRinwixVccKkAI-YCSQMCEFZOVWnGE6iReEqEAc");
            //pictureBox2.Load("https://i2.wp.com/beebom.com/wp-content/uploads/2016/01/Reverse-Image-Search-Engines-Apps-And-Its-Uses-2016.jpg");
        }

        public void Show_Multi_Image(string[] ImageURLs, int Number_Image)
        {
            if (Number_Image <= 1) return;

            Transition_Timer.Stop();
            Transition_Timer.Interval = 10000;
            Transition_Timer.Start();
            pictureBox1.Image = null;

            Task<Image>[] result = new Task<Image>[Number_Image];
            //Image_List = new Image[Number_Image];
            for (int CountImage = 0; CountImage < Number_Image; CountImage++)
            {
                result[CountImage] = GetImageAsync(ImageURLs[CountImage]);
                result[CountImage].ContinueWith(task =>
                {
                    imageList.Add(task.Result);
                    if(imageList.Count >= 2)
                    {
                        pictureBox1.Image = imageList[0];
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        pictureBox2.Image = imageList[1];
                        pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                });
            }
        }
        public void transitionPictures()
        {
            // We randomly choose where the current image is going to 
            // slide off to (and where we are going to slide the inactive
            // image in from)...
            int iDestinationLeft = (m_Random.Next(2) == 0) ? Width : -Width;
            int iDestinationTop = (m_Random.Next(3) - 1) * Height;

            // We move the inactive image to this location...
            SuspendLayout();
            m_InactivePicture.Top = iDestinationTop;
            m_InactivePicture.Left = iDestinationLeft;
            m_InactivePicture.BringToFront();
            ResumeLayout();

            // We perform the transition which moves the active image off the
            // screen, and the inactive one onto the screen...
            Transition t = new Transition(new TransitionType_EaseInEaseOut(1000));
            t.add(m_InactivePicture, "Left", 0);
            t.add(m_InactivePicture, "Top", 0);
            t.add(m_ActivePicture, "Left", iDestinationLeft);
            t.add(m_ActivePicture, "Top", iDestinationTop);
            t.run();

            // We swap over which image is active and inactive for next time
            // the function is called...
            PictureBox tmp = m_ActivePicture;
            m_ActivePicture = m_InactivePicture;
            m_InactivePicture = tmp;
        }

        public async Task<Image> GetImageAsync(string url)
        {
            var tcs = new TaskCompletionSource<Image>();
            Image webImage = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            await Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null)
                .ContinueWith(task =>
                {
                    var webResponse = (HttpWebResponse)task.Result;
                    Stream responseStream = webResponse.GetResponseStream();
                    if (webResponse.ContentEncoding.ToLower().Contains("gzip"))
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    else if (webResponse.ContentEncoding.ToLower().Contains("deflate"))
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);

                    if (responseStream != null) webImage = Image.FromStream(responseStream);
                    tcs.TrySetResult(webImage);
                    webResponse.Close();
                    responseStream.Close();
                });
            return tcs.Task.Result;
        }

        private void Transition_Timer_Tick(object sender, EventArgs e)
        {
            transitionPictures();
        }
    }
}
