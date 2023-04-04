using LibVLCSharp.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Display
{
    public partial class DefaultForm : UserControl
    {
        private LibVLC _libVLC;
        private MediaPlayer _mp;
        private string _VideoUrl = "";
        private string PathFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string _FileName = "";
        private string _ImageName = "";
        Timer tick;
        System.Timers.Timer Duration_Media_Tmr;
        System.Timers.Timer Duration_ThongBao_Tmr;
        System.Timers.Timer Duration_VanBan_Tmr;
        System.Timers.Timer Duration_HinhAnh_Tmr;

        private bool _is_ImageAvailable = false;
        private bool _is_VideoAvailable = false;
        private bool _is_ThongBaoAvailable = false;
        private bool _is_VanBanAvailable = false;

        private int _IdleTime = 0;
        private int _LoopNum = MAXVALUE;
        private int _Duration = MAXVALUE;
        private int CountTimeLoop = 0;
        private int IdleTimeCount = 0;

        private const int MAXVALUE = 1000 * 1000 * 1000;

        private bool _EnableLoop = false;

        public DefaultForm()
        {
            InitializeComponent();

            Init_VLC_Library();

            txtThongBao.Text = "";
            txtVanBan.Text = "";

            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
        }

        private void Init_VLC_Library()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mp = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mp;
            _mp.AspectRatio = "320:277";
            _mp.EncounteredError += _mp_EncounteredError;

            tick = new Timer();
            tick.Interval = 500;
            tick.Tick += Tick_Tick;

            Duration_Media_Tmr = new System.Timers.Timer();
            Duration_ThongBao_Tmr = new System.Timers.Timer();
            Duration_VanBan_Tmr = new System.Timers.Timer();
            Duration_HinhAnh_Tmr = new System.Timers.Timer();
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
                if (_isVideo_DownloadDone == true)
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
        public bool GetMuteStatus()
        {
            if (_mp.IsPlaying != true) return true;
            return _mp.Mute;
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
            _is_VideoAvailable = true;

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
                        _is_VideoAvailable = false;
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
                DownloadAsync_Video(_VideoUrl);
                _EnableLoop = true;
            }

            try
            {
                _mp.Playing -= _mp_Playing;
            }
            catch { }
        }

        private int Duration_Caculate(int VideoLength, int IdleTime, int Duration)
        {
            return Duration / (VideoLength + IdleTime) + 1;
        }
        public void Close()
        {
            try
            {
                panelThongBao.Stop();
                panelVanBan.Stop();

                Duration_Media_Tmr.Stop();
                Duration_Media_Tmr.Dispose();
                Duration_ThongBao_Tmr.Stop();
                Duration_ThongBao_Tmr.Dispose();
                Duration_VanBan_Tmr.Stop();
                Duration_VanBan_Tmr.Dispose();
                Duration_HinhAnh_Tmr.Stop();
                Duration_HinhAnh_Tmr.Dispose();

                _EnableLoop = false;
                _isVideo_DownloadDone = false;
            }
            catch { }
            Task.Run(() =>
            {
                videoView1.Visible = false;
                _mp.Stop();
                videoView1.Visible = true;
            });

            _is_VideoAvailable = false;
            _is_ThongBaoAvailable = false;
            _is_VanBanAvailable = false;
            _is_ImageAvailable = false;
            tick.Stop();
            try
            {
                _mp.Playing -= _mp_Playing;
            }
            catch { }
        }
        public void Set_Infomation(DisplayScheduleType ScheduleType, string Content = "", string ColorValue = "", int Duration = MAXVALUE)
        {
            Log.Information("Set_Infomation: {A}, Content: {B}", ScheduleType, Content.Substring(0, Content.Length / 5));
            if (ScheduleType == DisplayScheduleType.BanTinThongBao)
            {
                try
                {
                    txtThongBao.Text = Content.Trim().ToUpper();
                    if (ColorValue != "") txtThongBao.ForeColor = ColorTranslator.FromHtml(ColorValue);
                    //txtThongBao.Text = JustifyParagraph(txtThongBao.Text, txtThongBao.Font, panelThongBao.Width - 10);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Set_Infomation_BanTinThongBao");
                }

                // Text Run
                panelThongBao.SetSpeed = 1;
                int Text_Height1 = txtThongBao.Height;
                panelThongBao.Start(Text_Height1, 10000);

                // Duration Handle
                Duration_Handle(Duration_ThongBao_Tmr, ref Duration_ThongBao_Tmr, Duration, () =>
                {
                    panelThongBao.Stop();
                    txtThongBao.Text = "";
                    Log.Information("{A} Stop!", ScheduleType);
                    _is_ThongBaoAvailable = false;
                });
                _is_ThongBaoAvailable = true;
            }
            else if (ScheduleType == DisplayScheduleType.BanTinVanBan)
            {
                try
                {
                    txtVanBan.Text = Content;
                    txtVanBan.Text = JustifyParagraph(txtVanBan.Text, txtVanBan.Font, panelVanBan.Width - 6);
                    if (ColorValue != "") txtVanBan.ForeColor = ColorTranslator.FromHtml(ColorValue);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Set_Infomation_BanTinThongBao");
                }

                // Text Run
                panelVanBan.SetSpeed = 1;
                int Text_Height2 = txtVanBan.Height;
                panelVanBan.Start(Text_Height2, 10000);

                // Duration Handle
                Duration_Handle(Duration_VanBan_Tmr, ref Duration_VanBan_Tmr, Duration, () =>
                {
                    panelVanBan.Stop();
                    txtVanBan.Text = "";
                    Log.Information("{A} Stop!", ScheduleType);
                    _is_VanBanAvailable = false;
                });

                _is_VanBanAvailable = true;
            }

            pictureBox1.Visible = false;
            pictureBox2.Visible = false;

            ////string _Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
            //Font font = new Font(txtThongBao.Font.Name, txtThongBao.Font.Size);
            //pictureBox1.Width = panel1.Width;
            //pictureBox1.Height = (int)(this.CreateGraphics().MeasureString(txtThongBao.Text, font, panel1.Width).Height * 1.3);
            //pictureBox1.Image = ConvertTextToImage(txtThongBao.Text, font, panel1.BackColor, txtThongBao.ForeColor, pictureBox1.Width, pictureBox1.Height);
            ////pictureBox1.Image = ConvertTextToImage(txtThongBao);
            //txtThongBao.Visible = false;

            ////string _Text2 = "Triển khai thực hiện nhiệm vụ “Xây dựng hệ thống thông tin nguồn và thu thập, tổng hợp, phân tích, quản lý dữ liệu, đánh giá hiệu quả hoạt động thông tin cơ sở” tại Quyết định số 135/QĐ-TTg ngày 20/01/2020 của Thủ tướng Chính phủ phê duyệt Đề án nâng cao hiệu quả hoạt động thông tin cơ sở dựa trên ứng dụng công nghệ thông tin; Bộ Thông tin và Truyền thông ban hành Hướng dẫn về chức năng, tính năng kỹ thuật của Hệ thống thông tin nguồn trung ương, Hệ thống thông tin nguồn cấp tỉnh và kết nối các hệ thống thông tin - Phiên bản 1.0 (gửi kèm theo văn bản này).";
            //Font font2 = new Font(txtVanBan.Font.Name, txtVanBan.Font.Size);
            //pictureBox2.Width = panel2.Width;
            //pictureBox2.Height = (int)(this.CreateGraphics().MeasureString(txtVanBan.Text, font2, panel2.Width).Height * 1.3);
            //pictureBox2.Image = ConvertTextToImage(txtVanBan.Text, font2, panel2.BackColor, txtVanBan.ForeColor, pictureBox2.Width, pictureBox2.Height);
            ////pictureBox2.Image = ConvertTextToImage(txtVanBan);
            //txtVanBan.Visible = false;
        }
        public async void ShowImage(string Url, int Duration = MAXVALUE)
        {
            Log.Information("ShowImage: {A}", Url);

            Task task = Task.Run(() =>
            {
                videoView1.Visible = false;
                _mp.Stop();
                videoView1.Visible = true;
            });
            await task;
            DownloadAsync_Image(Url);

            // Duration Handle
            Duration_Handle(Duration_HinhAnh_Tmr, ref Duration_HinhAnh_Tmr, Duration, () =>
            {
                // Stop Media
                Task.Run(() =>
                {
                    videoView1.Visible = false;
                    _mp.Stop();
                    videoView1.Visible = true;
                    _is_ImageAvailable = false;
                });
                Log.Information("Image Stop!");
            });

            _is_ImageAvailable = true;

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

        public Bitmap ConvertTextToImage(Control control)
        {
            var bitmap = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

            return bitmap;
        }

        public Bitmap ConvertTextToImage(string txt, Font font, Color bgcolor, Color fcolor, int width, int Height)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                Rectangle rectF1 = new Rectangle(0, 0, width, Height);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), rectF1);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }
            return bmp;
        }
        public string JustifyParagraph(string text, Font font, int ControlWidth)
        {
            string result = string.Empty;
            List<string> ParagraphsList = new List<string>();
            ParagraphsList.AddRange(text.Split(new[] { "\n" }, StringSplitOptions.None).ToList());

            foreach (string Paragraph in ParagraphsList)
            {
                string line = string.Empty;
                int ParagraphWidth = TextRenderer.MeasureText(Paragraph, font).Width;

                if (ParagraphWidth > ControlWidth)
                {
                    //Get all paragraph words, add a normal space and calculate when their sum exceeds the constraints
                    string[] Words = Paragraph.Split(' ');
                    line = Words[0] + (char)32;
                    for (int x = 1; x < Words.Length; x++)
                    {
                        string tmpLine = line + (Words[x] + (char)32);
                        if (TextRenderer.MeasureText(tmpLine, font).Width > ControlWidth)
                        {
                            //Max lenght reached. Justify the line and step back
                            result += Justify(line.TrimEnd(), font, ControlWidth) + "\n";
                            line = string.Empty;
                            --x;
                        }
                        else
                        {
                            //Some capacity still left
                            line += (Words[x] + (char)32);
                        }
                    }
                    //Adds the remainder if any
                    if (line.Length > 0)
                        result += line + "\n";
                }
                else
                {
                    result += Paragraph + "\n";
                }
            }
            return result.TrimEnd(new[] { '\n' });
        }
        private string Justify(string text, Font font, int width)
        {
            char SpaceChar = (char)0x200A;
            List<string> WordsList = text.Split((char)32).ToList();
            if (WordsList.Capacity < 2)
                return text;

            int NumberOfWords = WordsList.Capacity - 1;
            int WordsWidth = TextRenderer.MeasureText(text.Replace(" ", ""), font).Width;
            int SpaceCharWidth = TextRenderer.MeasureText(WordsList[0] + SpaceChar, font).Width
                               - TextRenderer.MeasureText(WordsList[0], font).Width;

            //Calculate the average spacing between each word minus the last one 
            int AverageSpace = ((width - WordsWidth) / NumberOfWords) / SpaceCharWidth;
            float AdjustSpace = (width - (WordsWidth + (AverageSpace * NumberOfWords * SpaceCharWidth)));

            //Add spaces to all words
            return ((Func<string>)(() =>
            {
                string Spaces = "";
                string AdjustedWords = "";

                for (int h = 0; h < AverageSpace; h++)
                    Spaces += SpaceChar;

                foreach (string Word in WordsList)
                {
                    AdjustedWords += Word + Spaces;
                    //Adjust the spacing if there's a reminder
                    if (AdjustSpace > 0)
                    {
                        AdjustedWords += SpaceChar;
                        AdjustSpace -= SpaceCharWidth;
                    }
                }
                return AdjustedWords.TrimEnd();
            }))();
        }
        private bool _isVideo_DownloadDone = false;
        private void DownloadAsync_Video(string Url)
        {
            _isVideo_DownloadDone = false;

            string fileExtension = "";
            Uri uri = new Uri(Url);
            try
            {
                fileExtension = Path.GetExtension(uri.LocalPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Video_GetExtension: {Url}", Url);
            }
            _FileName = Path.Combine(PathFile, "SaveVideo" + fileExtension);

            WebClient webClient = new WebClient();
            webClient.DownloadFileAsync(uri, _FileName);
            webClient.DownloadFileCompleted += (sender , e)=>
            {
                _isVideo_DownloadDone = true;
                Log.Information("DownloadVideoCompleted: {A}", Url);
                webClient.Dispose();
            };
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
                _mp.Play(new Media(_libVLC, new Uri(_ImageName)));
                webClient.Dispose();
            };
        }

        public void DefaultForm_FitToContainer(int Height, int Width)
        {
            Utility.FitUserControlToContainer(this, Height, Width);

            txtThongBao.MaximumSize = new Size(panelThongBao.Width, 0);
            txtVanBan.MaximumSize = new Size(panelVanBan.Width, 0);
            //txtVanBan.Text = JustifyParagraph(txtVanBan.Text, txtVanBan.Font, panelVanBan.Width - 10);

            label2.Width = 1;
            label1.Height = 1;
        }

        private void Timer_AutoHideScreen_Tick(object sender, EventArgs e)
        {
            if (_is_VideoAvailable == false && _is_ThongBaoAvailable == false && _is_VanBanAvailable == false && 
                                                                                 _is_ImageAvailable == false)
            {
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
            }
        }
    }
}
