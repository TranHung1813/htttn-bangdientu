using LibVLCSharp.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private string PathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Files");
        private string _FileName = "";
        private string _ImageName = "";

        System.Timers.Timer Duration_HinhAnh_Tmr;
        System.Timers.Timer Duration_Video_Tmr;

        public bool _is_ImageAvailable = false;
        public bool _is_VideoAvailable = false;
        public bool _is_ThongBaoAvailable = false;
        public bool _is_VanBanAvailable = false;

        private string _ScheduleID_ThongBao = "";
        private string _ScheduleID_VanBan = "";
        private string _ScheduleID_Video = "";
        private string _ScheduleID_Image = "";

        private int _Priority_ThongBao = 1000;
        private int _Priority_VanBan = 1000;
        private int _Priority_Video = 1000;
        private int _Priority_Image = 1000;

        private const int MAXVALUE = 1000 * 1000 * 1000;

        public DefaultForm()
        {
            InitializeComponent();

            //Init_VLC_Library();

            txtThongBao.Text = "";
            txtVanBan.Text = "";

            pictureBox1.Visible = false;
            pictureBox2.Visible = false;

            Duration_HinhAnh_Tmr = new System.Timers.Timer();
            Duration_Video_Tmr = new System.Timers.Timer();
            AutoHideScreen_Check();
        }

        private void Init_VLC_Library()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            if (_mp != null)
            {
                try
                {
                    videoView1.Visible = false;
                    videoView1.MediaPlayer = null;
                    _mp.Stop();
                    _mp.EncounteredError -= _mp_EncounteredError;
                    _mp.EndReached -= _mp_EndReached;
                    _mp.Playing -= _mp_Playing;
                    // _mp = null;
                    //_mp.Dispose();
                }
                catch { }
            }
            _mp = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mp;
            _mp.AspectRatio = "320:277";
            _mp.EncounteredError += _mp_EncounteredError;
            _mp.EndReached += _mp_EndReached;
            videoView1.Visible = true;
        }

        private void _mp_EndReached(object sender, EventArgs e)
        {
            if (_is_VideoAvailable == true)
            {
                videoView1.Visible = false;
                _is_VideoAvailable = false;
                AutoHideScreen_Check();
            }
        }

        private void _mp_EncounteredError(object sender, EventArgs e)
        {
            Log.Error("_mp_EncounteredError : {A}", e.ToString());
        }

        public void GetScheduleInfo(ref string ScheduleID, ref string PlayingFile, ref int PlayState, ref bool IsSpkOn, ref int Volume)
        {
            ScheduleID = _ScheduleID_Video;
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

        public void ShowVideo(string url, string ScheduleID, int Priority = 0, int StartPos = 0, int Duration = MAXVALUE)
        {
            if (_Priority_Video < Priority) return;
            Log.Information("ShowVideo: {A}", url);
            _VideoUrl = url;
            _is_VideoAvailable = true;
            _ScheduleID_Video = ScheduleID;
            _Priority_Video = Priority;
            this.Visible = true;

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

            // Duration Handle
            //Duration_Handle(Duration_Video_Tmr, ref Duration_Video_Tmr, Duration, () =>
            //{
            //    // Stop Media
            //    _is_VideoAvailable = false;
            //    Log.Information("Het Video!");
            //});
        }

        private async void PlayVideo(string url, int StartPos = 0)
        {
            string[] @params = new string[] { "input-repeat=0", "start-time=" + StartPos.ToString() };//, "run-time=5" };
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
                    Log.Error("PlayMedia_Fail: {A}, TimeWait: {B}ms", url, 5000);
                    //Log.Information("Retry Play Video 1 time!");
                    //await PlayVideo;
                }
            }
        }

        private void _mp_Playing(object sender, EventArgs e)
        {
            long VideoLength = _mp.Length;
            Log.Information("PlayMedia_Succeeded: {A}, length: {B}", _VideoUrl, VideoLength);
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
                    int index = SavedFiles.FindIndex(s => (s.ScheduleId == _ScheduleID_Video) && (s.Link == _VideoUrl));
                    if (index != -1)
                    {
                        // Da Download
                        if (File.Exists(SavedFiles[index].PathLocation))
                        {

                        }
                        else
                        {
                            // Neu chua download thi Download
                            DownloadAsync_Video(_VideoUrl, _ScheduleID_Video);
                        }
                    }
                    else
                    {
                        // Neu chua download thi Download
                        DownloadAsync_Video(_VideoUrl, _ScheduleID_Video);
                    }
                }
                else
                {
                    // Neu chua download thi Download
                    DownloadAsync_Video(_VideoUrl, _ScheduleID_Video);
                }
            }

            try
            {
                _mp.Playing -= _mp_Playing;
            }
            catch { }
        }
        public bool CheckPriority(int Priority)
        {
            if(_is_ImageAvailable == true)
            {
                if (_Priority_Image < Priority) return false;
            }
            else if(_is_VideoAvailable == true)
            {
                if (_Priority_Video < Priority) return false;
            }
            else if(_is_ThongBaoAvailable == true)
            {
                if (_Priority_ThongBao < Priority) return false;
            }
            else if(_is_VanBanAvailable == true)
            {
                if (_Priority_VanBan < Priority) return false;
            }
            return true;
        }
        public void Close()
        {
            try
            {
                txtThongBao.Text = "";
                txtVanBan.Text = "";
                panelThongBao.Stop();
                panelVanBan.Stop();

                if (Duration_HinhAnh_Tmr != null)
                {
                    Duration_HinhAnh_Tmr.Stop();
                    Duration_HinhAnh_Tmr.Dispose();
                }

                if(Duration_Video_Tmr != null)
                {
                    Duration_Video_Tmr.Stop();
                    Duration_Video_Tmr.Dispose();
                }
            }
            catch { }
            Task.Run(() =>
            {
                try
                {
                    //videoView1.Visible = false;
                    videoView1.MediaPlayer = null;
                    if (_mp != null)
                    {
                        _mp.Stop();
                        _mp.EncounteredError -= _mp_EncounteredError;
                        _mp.EndReached -= _mp_EndReached;
                        _mp.Playing -= _mp_Playing;
                    }
                }
                catch { }
            });

            _is_VideoAvailable = false;
            _is_ThongBaoAvailable = false;
            _is_VanBanAvailable = false;
            _is_ImageAvailable = false;
            AutoHideScreen_Check();

            _Priority_ThongBao = 1000;
            _Priority_VanBan = 1000;
            _Priority_Video = 1000;
            _Priority_Image = 1000;
        }

        public async void Close_by_Id(string ScheduleId)
        {
            if (_ScheduleID_Video == ScheduleId)
            {
                Log.Information("Ban tin Video het thoi gian Valid!");
                await Task.Run(() =>
                {
                    videoView1.Visible = false;
                    _mp.Stop();
                    videoView1.Visible = true;
                });

                if (Duration_Video_Tmr != null)
                {
                    Duration_Video_Tmr.Stop();
                    Duration_Video_Tmr.Dispose();
                }

                _is_VideoAvailable = false;
                AutoHideScreen_Check();
                _Priority_Video = 1000;

                try
                {
                    _mp.Playing -= _mp_Playing;
                }
                catch { }
            }
            else if (_ScheduleID_Image == ScheduleId)
            {
                Log.Information("Ban tin Hinh Anh het thoi gian Valid!");
                await Task.Run(() =>
                {
                    videoView1.Visible = false;
                    _mp.Stop();
                    videoView1.Visible = true;
                });
                if (Duration_HinhAnh_Tmr != null)
                {
                    Duration_HinhAnh_Tmr.Stop();
                    Duration_HinhAnh_Tmr.Dispose();
                }
                _is_ImageAvailable = false;
                AutoHideScreen_Check();
                _Priority_Image = 1000;
            }
            else if (_ScheduleID_ThongBao == ScheduleId)
            {
                Log.Information("Ban tin Thong Bao het thoi gian Valid!");
                try
                {
                    panelThongBao.Stop();
                    txtThongBao.Text = "";
                    _is_ThongBaoAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_ThongBao = 1000;
                }
                catch { }
            }
            else if (_ScheduleID_VanBan == ScheduleId)
            {
                Log.Information("Ban tin Van Ban het thoi gian Valid!");
                try
                {
                    panelVanBan.Stop();
                    txtVanBan.Text = "";
                    _is_VanBanAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_VanBan = 1000;
                }
                catch { }
            }
        }
        public void Set_Infomation(DisplayScheduleType ScheduleType, string ScheduleID, string Content = "", int Priority = 0, string ColorValue = "", int Duration = MAXVALUE)
        {
            Log.Information("Set_Infomation: {A}, Content: {B}", ScheduleType, Content.Substring(0, Content.Length / 5));
            if (ScheduleType == DisplayScheduleType.BanTinThongBao)
            {
                if (_Priority_ThongBao < Priority) return;
                if (txtThongBao.Text == Content) return;
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

                _is_ThongBaoAvailable = true;
                _Priority_ThongBao = Priority;
                _ScheduleID_ThongBao = ScheduleID;
                this.Visible = true;
            }
            else if (ScheduleType == DisplayScheduleType.BanTinVanBan)
            {
                if (_Priority_VanBan < Priority) return;
                if (txtVanBan.Text == Content) return;
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

                _is_VanBanAvailable = true;
                _Priority_VanBan = Priority;
                _ScheduleID_VanBan = ScheduleID;
                this.Visible = true;
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
        public async void ShowImage(string Url, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            if (_Priority_Image < Priority) return;
            Log.Information("ShowImage: {A}", Url);
            _ScheduleID_Image = ScheduleId;
            _Priority_Image = Priority;

            Task task = Task.Run(() =>
            {
                Init_VLC_Library();
            });
            await task;
            DownloadAsync_Image(Url, ScheduleId);

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
                    AutoHideScreen_Check();
                    _Priority_Image = 1000;
                });
                Log.Information("Image Stop!");
            });

            _is_ImageAvailable = true;
            this.Visible = true;

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
        private void DownloadAsync_Video(string Url, string ScheduleId)
        {
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
            _FileName = Path.Combine(PathFile, "SaveVideo-" + ScheduleId + fileExtension);

            WebClient webClient = new WebClient();
            webClient.DownloadFileAsync(uri, _FileName);
            webClient.DownloadFileCompleted += (sender, e) =>
            {
                Log.Information("DownloadVideoCompleted: {A}", Url);

                // Save to Database
                SavedFile_Type videoFile = new SavedFile_Type();
                videoFile.PathLocation = _FileName;
                videoFile.ScheduleId = ScheduleId;
                videoFile.Link = Url;
                SaveFileDownloaded(videoFile);

                webClient.Dispose();
            };
        }
        private void SaveFileDownloaded(SavedFile_Type file)
        {
            List<DataUser_SavedFiles> SavedFiles = SqLiteDataAccess.Load_SavedFiles_Info();
            DataUser_SavedFiles info_Save = new DataUser_SavedFiles();
            if (SavedFiles != null)
            {
                int index = SavedFiles.FindIndex(s => s.ScheduleId == file.ScheduleId);
                if (index == -1)
                {
                    info_Save.Id = SavedFiles.Count + 1;
                }
                else
                {
                    info_Save.Id = index + 1;
                }
            }
            else
            {
                info_Save.Id = 1;
            }
            info_Save.ScheduleId = file.ScheduleId;
            info_Save.PathLocation = file.PathLocation;
            info_Save.Link = file.Link;

            SqLiteDataAccess.SaveInfo_SavedFiles(info_Save);
        }
        private void DownloadAsync_Image(string Url, string ScheduleId)
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
            _ImageName = Path.Combine(PathFile, "SaveImage-" + ScheduleId + fileExtension);

            WebClient webClient = new WebClient();
            webClient.DownloadFileAsync(uri, _ImageName);
            webClient.DownloadFileCompleted += (sender, e) =>
            {
                Log.Information("DownloadImageCompleted: {A}, PathLocation: {B}", Url, _ImageName);
                string[] @params = new string[] { "input-repeat=65536" };//, "run-time=5" };
                _mp.Play(new Media(_libVLC, new Uri(_ImageName), @params));

                // Save to Database
                SavedFile_Type videoFile = new SavedFile_Type();
                videoFile.PathLocation = _ImageName;
                videoFile.ScheduleId = ScheduleId;
                videoFile.Link = Url;
                SaveFileDownloaded(videoFile);

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

        private void AutoHideScreen_Check()
        {
            if (_is_VideoAvailable == false && _is_ThongBaoAvailable == false && _is_VanBanAvailable == false &&
                                                                                 _is_ImageAvailable == false)
            {
                this.Visible = false;
            }
        }
    }
    public class SavedFile_Type
    {
        public string ScheduleId;
        public string PathLocation; /* Path File Downloaded */
        public string Link;
    }
}
