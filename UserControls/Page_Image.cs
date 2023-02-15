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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class Page_Image : UserControl
    {
        private string _ImageURL = "";
        public Page_Image()
        {
            InitializeComponent();
            //_ImageURL = "http://www.gravatar.com/avatar/6810d91caff032b202c50701dd3af745?d=identicon&r=PG";
            _ImageURL = "https://source.unsplash.com/user/c_v_r/1900x800";
        }

        public void ShowImage(string Url)
        {
            pictureBox1.Load(_ImageURL);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            //var result = GetImageAsync(_ImageURL);
            //result.ContinueWith(task =>
            //{
            //    pictureBox1.Image = task.Result;
            //    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            //});
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
    }
}
