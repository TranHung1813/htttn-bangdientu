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
        private const int MAXVALUE = 1000 * 1000 * 1000;

        public Form_Image form_Image = new Form_Image();
        public Page_Image()
        {
            InitializeComponent();
            //_ImageURL = "https://fastly.picsum.photos/id/1026/1200/600.jpg?hmac=JwvbmRinwixVccKkAI-YCSQMCEFZOVWnGE6iReEqEAc";
            //_ImageURL = "https://source.unsplash.com/user/c_v_r/1900x800";
        }

        public void ShowImage(string Url, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            if (form_Image != null)
            {
                try
                {
                    form_Image.CloseForm();
                    form_Image.Dispose();
                }
                catch { }
            }

            form_Image = new Form_Image();
            form_Image.PageImage_FitToContainer(Height, Width);
            form_Image.ShowImage(Url, ScheduleId, Priority, Duration);

            this.Visible = true;
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
            if (form_Image != null)
            {
                try
                {
                    form_Image.CloseForm();
                    form_Image.Dispose();
                }
                catch { }
            }
        }
    }
}
