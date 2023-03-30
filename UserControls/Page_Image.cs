using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class Page_Image : UserControl
    {
        //private string _ImageURL = "";
        private string PathFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string _ImageName = "";
        System.Timers.Timer Duration_HinhAnh_Tmr;
        public Page_Image()
        {
            InitializeComponent();
            //_ImageURL = "https://fastly.picsum.photos/id/1026/1200/600.jpg?hmac=JwvbmRinwixVccKkAI-YCSQMCEFZOVWnGE6iReEqEAc";
            //_ImageURL = "https://source.unsplash.com/user/c_v_r/1900x800";

            Duration_HinhAnh_Tmr = new System.Timers.Timer();
        }

        public void ShowImage(string Url, int Duration)
        {
            Log.Information("ShowImage: {A}", Url);
            pictureBox1.Image = null;
            DownloadAsync_Image(Url);

            // Duration Handle
            Duration_Handle(Duration_HinhAnh_Tmr, ref Duration_HinhAnh_Tmr, Duration, () =>
            {
                // Stop Media
                pictureBox1.Image = null;
                Log.Information("Image Stop!");
            });
            //var result = GetImageAsync(_ImageURL);
            //result.ContinueWith(task =>
            //{
            //    pictureBox1.Image = task.Result;
            //    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            //});
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

        private void DownloadAsync_Image(string Url)
        {
            string fileExtension = "";
            Uri uri = new Uri(Url);
            try
            {
                fileExtension = Path.GetExtension(uri.LocalPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Image_GetExtension: {Url}", Url);
            }
            _ImageName = Path.Combine(PathFile, "SaveImage" + fileExtension);

            WebClient webClient = new WebClient();
            webClient.DownloadFileAsync(uri, _ImageName);
            webClient.DownloadFileCompleted += (sender, e) =>
            {
                Log.Information("DownloadImageCompleted: {A}", Url);
                try
                {
                    pictureBox1.Load(_ImageName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "DownloadAsync_Image_Completed");
                }
                webClient.Dispose();
            };
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
        public void Close()
        {
            Duration_HinhAnh_Tmr.Stop();
            Duration_HinhAnh_Tmr.Dispose();
            pictureBox1.Image = null;
        }
    }
}
