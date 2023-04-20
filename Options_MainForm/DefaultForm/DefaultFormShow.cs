using LibVLCSharp.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class DefaultFormShow : Form
    {
        private LibVLC _libVLC;
        private MediaPlayer _mp;
        private string _VideoUrl = "";
        private string PathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Files");
        private string _FileName = "";
        private string _ImageName = "";

        private int panelVanBan_OldHeight = 0;

        System.Timers.Timer Duration_ThongBao_Tmr;
        //System.Timers.Timer Duration_VanBan_Tmr;
        System.Timers.Timer Duration_HinhAnh_Tmr;
        System.Timers.Timer Duration_Video_Tmr;

        public bool _is_ImageAvailable = false;
        public bool _is_VideoAvailable = false;
        public bool _is_ThongBaoAvailable = false;
        //public bool _is_VanBanAvailable = false;

        private string _ScheduleID_ThongBao = "";
        //private string _ScheduleID_VanBan = "";
        private string _ScheduleID_Video = "";
        private string _ScheduleID_Image = "";

        private int _Priority_ThongBao = 1000;
        //private int _Priority_VanBan = 1000;
        private int _Priority_Video = 1000;
        private int _Priority_Image = 1000;

        Form_VanBan form_VB = new Form_VanBan();

        private const int MAXVALUE = 1000 * 1000 * 1000;
        public DefaultFormShow()
        {
            InitializeComponent();
            this.TransparencyKey = Color.Khaki;

            txtThongBao.Text = "";
            pictureBox1.Visible = false;

            Duration_ThongBao_Tmr = new System.Timers.Timer();
            //Duration_VanBan_Tmr = new System.Timers.Timer();
            Duration_HinhAnh_Tmr = new System.Timers.Timer();
            Duration_Video_Tmr = new System.Timers.Timer();
            //AutoHideScreen_Check();

            picBox_Image.LoadCompleted += PicBox_Image_LoadCompleted;
        }

        private void PicBox_Image_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.Visible = true;
            this.Activate();
            this.BringToFront();
            this.Focus();
            picBox_Image.Visible = true;
            OnNotifyStartProcess();
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
            videoView1.Visible = false;
            _is_VideoAvailable = false;
            AutoHideScreen_Check();
        }

        private void _mp_EncounteredError(object sender, EventArgs e)
        {
            Log.Error("_mp_EncounteredError : {A}", e.ToString());
        }

        public void GetScheduleInfo(ref string ScheduleID, ref string PlayingFile, ref int PlayState, ref bool IsSpkOn, ref int Volume)
        {
            if (_is_VideoAvailable == false) return;
            ScheduleID = _ScheduleID_Video;
            PlayingFile = _VideoUrl;
            if (_mp == null) return;
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
        public void SetVolume(int value)
        {
            if (_is_VideoAvailable == false) return;
            try
            {
                _mp.Volume = value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SetVolume");
            }
        }

        public void ShowVideo(string url, string ScheduleID, int Priority = 0, int StartPos = 0, int Duration = MAXVALUE)
        {
            if (_Priority_Video < Priority) return;
            Log.Information("ShowVideo: {A}", url);
            _VideoUrl = url;
            _is_VideoAvailable = true;
            _ScheduleID_Video = ScheduleID;
            _Priority_Video = Priority;

            picBox_Image.Visible = false;
            if (picBox_Image.Image != null)
            {
                try
                {
                    picBox_Image.Image.Dispose();
                }
                catch { }
            }
            videoView1.Visible = true;

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

            this.Activate();
            this.BringToFront();
            this.Focus();
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
            this.Visible = true;
            OnNotifyStartProcess();
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
            if (_is_ImageAvailable == true)
            {
                if (_Priority_Image < Priority) return false;
            }
            else if (_is_VideoAvailable == true)
            {
                if (_Priority_Video < Priority) return false;
            }
            else if (_is_ThongBaoAvailable == true)
            {
                if (_Priority_ThongBao < Priority) return false;
            }
            else if (form_VB != null)
            {
                if (form_VB._is_VanBanAvailable == true)
                {
                    if (form_VB._Priority_VanBan < Priority) return false;
                }
            }
            return true;
        }
        public void Close()
        {
            try
            {
                txtThongBao.Text = "";
                pictureBox1.Visible = false;
                if (pictureBox1.Image != null)
                {
                    try
                    {
                        pictureBox1.Image.Dispose();
                    }
                    catch { }
                }
                panelThongBao.Stop();

                if (Duration_ThongBao_Tmr != null)
                {
                    Duration_ThongBao_Tmr.Stop();
                    Duration_ThongBao_Tmr.Dispose();
                }

                //if (Duration_VanBan_Tmr != null)
                //{
                //    Duration_VanBan_Tmr.Stop();
                //    Duration_VanBan_Tmr.Dispose();
                //}

                if (Duration_HinhAnh_Tmr != null)
                {
                    Duration_HinhAnh_Tmr.Stop();
                    Duration_HinhAnh_Tmr.Dispose();
                }

                if (Duration_Video_Tmr != null)
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
            if (picBox_Image.Image != null)
            {
                try
                {
                    picBox_Image.Image.Dispose();
                }
                catch { }
            }
            picBox_Image.Visible = false;

            _is_VideoAvailable = false;
            _is_ThongBaoAvailable = false;
            //_is_VanBanAvailable = false;
            _is_ImageAvailable = false;
            AutoHideScreen_Check();

            _Priority_ThongBao = 1000;
            //_Priority_VanBan = 1000;
            _Priority_Video = 1000;
            _Priority_Image = 1000;

            if (form_VB != null)
            {
                try
                {
                    form_VB.CloseForm();
                    form_VB.Dispose();
                }
                catch { }
            }
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

                if (picBox_Image.Image != null)
                {
                    try
                    {
                        picBox_Image.Image.Dispose();
                    }
                    catch { }
                }
                picBox_Image.Visible = false;

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
                    pictureBox1.Visible = false;
                    if (pictureBox1.Image != null)
                    {
                        try
                        {
                            pictureBox1.Image.Dispose();
                        }
                        catch { }
                    }

                    if (Duration_ThongBao_Tmr != null)
                    {
                        Duration_ThongBao_Tmr.Stop();
                        Duration_ThongBao_Tmr.Dispose();
                    }

                    _is_ThongBaoAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_ThongBao = 1000;
                }
                catch { }
            }
            else if (form_VB != null)
            {
                if (form_VB.ScheduleID_VanBan == ScheduleId)
                {
                    Log.Information("Ban tin Van Ban het thoi gian Valid!");
                    try
                    {
                        if (form_VB != null)
                        {
                            try
                            {
                                form_VB.CloseForm();
                                form_VB.Dispose();
                            }
                            catch { }
                        }

                        //if (Duration_VanBan_Tmr != null)
                        //{
                        //    Duration_VanBan_Tmr.Stop();
                        //    Duration_VanBan_Tmr.Dispose();
                        //}

                        AutoHideScreen_Check();
                    }
                    catch { }
                }
            }
        }
        public bool CheckMessage_Available()
        {
            if (_is_ImageAvailable || _is_VideoAvailable || form_VB._is_VanBanAvailable || _is_ThongBaoAvailable) return true;
            else return false;
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

                    //string _Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
                    Font font = new Font(txtThongBao.Font.Name, txtThongBao.Font.Size);
                    pictureBox1.Width = panel1.Width;
                    pictureBox1.Height = (int)(this.CreateGraphics().MeasureString(txtThongBao.Text, font, panel1.Width).Height * 1.3);
                    //pictureBox1.Image = ConvertTextToImage(txtThongBao.Text, font, panel1.BackColor, txtThongBao.ForeColor, pictureBox1.Width, pictureBox1.Height);
                    if(pictureBox1.Image != null)
                    {
                        try
                        {
                            pictureBox1.Image.Dispose();
                        }
                        catch { }
                    }
                    pictureBox1.Image = ConvertTextToImage(txtThongBao);
                    pictureBox1.Visible = true;
                    txtThongBao.Visible = false;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Set_Infomation_BanTinThongBao");
                }


                // Duration Handle
                Duration_Handle(Duration_ThongBao_Tmr, ref Duration_ThongBao_Tmr, Duration, () =>
                {
                    // Stop Media
                    panelThongBao.Stop();
                    txtThongBao.Text = "";
                    pictureBox1.Visible = false;
                    if (pictureBox1.Image != null)
                    {
                        try
                        {
                            pictureBox1.Image.Dispose();
                        }
                        catch { }
                    }
                    _is_ThongBaoAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_ThongBao = 1000;
                    Log.Information("BanTinThongBao Stop!");
                });

                // Text Run
                panelThongBao.SetSpeed = 1;
                int Text_Height1 = txtThongBao.Height;
                panelThongBao.Start(Text_Height1, 10000);

                _is_ThongBaoAvailable = true;
                _Priority_ThongBao = Priority;
                _ScheduleID_ThongBao = ScheduleID;
                this.Visible = true;
                OnNotifyStartProcess();

                this.Activate();
                this.BringToFront();
                this.Focus();
            }
            else if (ScheduleType == DisplayScheduleType.BanTinVanBan)
            {
                Show_VanBan(ScheduleID, Content, Priority, ColorValue, Duration);
                this.Visible = true;
                OnNotifyStartProcess();

                this.Activate();
                this.BringToFront();
                this.Focus();
            }
        }

        public Bitmap ConvertTextToImage(Control control)
        {
            var bitmap = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

            return bitmap;
        }

        private void Show_VanBan(string ScheduleID, string Content = "", int Priority = 0, string ColorValue = "", int Duration = MAXVALUE)
        {
            if (form_VB != null)
            {
                try
                {
                    form_VB.CloseForm();
                    form_VB.Dispose();
                }
                catch { }
            }

            form_VB = new Form_VanBan();

            form_VB.ContainerHeight_OldValue = panelVanBan_OldHeight;
            form_VB.PageText_FitToContainer(panelVanBan.Height, panelVanBan.Width);
            form_VB.SetLocation_VanBan(panelVanBan.Location);
            form_VB.StartPosition = FormStartPosition.Manual;

            form_VB.NotifyEndProcess_TextRun += Form_VB_NotifyEndProcess_TextRun;
            form_VB.SetSpeed = 1;
            form_VB.ShowText(Content, ScheduleID, Priority, Duration);
        }

        private void Form_VB_NotifyEndProcess_TextRun(object sender, NotifyTextEndProcess e)
        {
            AutoHideScreen_Check();
            form_VB.NotifyEndProcess_TextRun -= Form_VB_NotifyEndProcess_TextRun;
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
        public void ShowImage(string Url, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            if (_Priority_Image < Priority) return;
            Log.Information("ShowImage: {A}", Url);
            _ScheduleID_Image = ScheduleId;
            _Priority_Image = Priority;
            if (picBox_Image.Image != null)
            {
                try
                {
                    picBox_Image.Image.Dispose();
                }
                catch { }
            }

            videoView1.Visible = false;

            // Duration Handle
            Duration_Handle(Duration_HinhAnh_Tmr, ref Duration_HinhAnh_Tmr, Duration, () =>
            {
                // Stop Media
                if (picBox_Image.Image != null)
                {
                    try
                    {
                        picBox_Image.Image.Dispose();
                    }
                    catch { }
                }
                picBox_Image.Visible = false;
                _is_ImageAvailable = false;
                AutoHideScreen_Check();
                _Priority_Image = 1000;
                Log.Information("Image Stop!");
            });

            _is_ImageAvailable = true;

            List<DataUser_SavedFiles> SavedFiles = SqLiteDataAccess.Load_SavedFiles_Info();

            if (SavedFiles != null)
            {
                // Kiem tra xem File da download chua, neu roi thi Play
                int index = SavedFiles.FindIndex(s => (s.ScheduleId == ScheduleId) && (s.Link == Url));
                if (index != -1)
                {
                    if (File.Exists(SavedFiles[index].PathLocation))
                    {
                        picBox_Image.LoadAsync(SavedFiles[index].PathLocation);
                    }
                    else
                    {
                        DownloadAsync_Image(Url, ScheduleId);
                    }
                }
                else
                {
                    // Neu chua download thi play link nhu binh thuong
                    DownloadAsync_Image(Url, ScheduleId);
                }
            }
            else
            {
                // Neu chua download thi play link nhu binh thuong
                DownloadAsync_Image(Url, ScheduleId);
            }

            Task.Run(() =>
            {
                //    // Tắt Video
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
            });
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
                try
                {
                    picBox_Image.LoadAsync(_ImageName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "DownloadAsync_Image_Completed");
                }

                // Save to Database
                SavedFile_Type videoFile = new SavedFile_Type();
                videoFile.PathLocation = _ImageName;
                videoFile.ScheduleId = ScheduleId;
                videoFile.Link = Url;
                SaveFileDownloaded(videoFile);

                webClient.Dispose();
            };
        }

        public Point GetLocation_ThongBao()
        {
            return panelThongBao.Location;
        }

        public void GetInfo_ThongBao(ref Point Location, ref Size size)
        {
            Location = panelThongBao.Location;
            size = panelThongBao.Size;
        }
        public Point GetLocation_VanBan()
        {
            return panelVanBan.Location;
        }
        public void GetInfo_VanBan(ref Point Location, ref Size size)
        {
            Location = panelVanBan.Location;
            size = panelVanBan.Size;
        }

        public void DefaultForm_FitToContainer(int Height, int Width)
        {
            panelVanBan_OldHeight = panelVanBan.Size.Height;
            Utility.fitFormToScreen(this, 768, 1366);

            txtThongBao.MaximumSize = new Size(panelThongBao.Width, 0);

            label2.Width = 1;
            label1.Height = 1;
        }
        private void AutoHideScreen_Check()
        {
            try
            {
                if (_is_VideoAvailable == false && _is_ThongBaoAvailable == false && form_VB._is_VanBanAvailable == false &&
                                                                                     _is_ImageAvailable == false)
                {
                    OnNotifyEndProcess();
                    this.Visible = false;
                }
            }
            catch { }
        }

        private event EventHandler<NotifyTextEndProcess> _NotifyEndProcess_TextRun;
        public event EventHandler<NotifyTextEndProcess> NotifyEndProcess_TextRun
        {
            add
            {
                _NotifyEndProcess_TextRun += value;
            }
            remove
            {
                _NotifyEndProcess_TextRun -= value;
            }
        }
        protected virtual void OnNotifyEndProcess()
        {
            if (_NotifyEndProcess_TextRun != null)
            {
                _NotifyEndProcess_TextRun(this, new NotifyTextEndProcess());
            }
        }
        private event EventHandler<NotifyStartProcess> _NotifyStartProcess;
        public event EventHandler<NotifyStartProcess> NotifyStartProcess
        {
            add
            {
                _NotifyStartProcess += value;
            }
            remove
            {
                _NotifyStartProcess -= value;
            }
        }
        protected virtual void OnNotifyStartProcess()
        {
            if (_NotifyStartProcess != null)
            {
                _NotifyStartProcess(this, new NotifyStartProcess());
            }
        }
    }
    public class NotifyStartProcess : EventArgs
    {
        public NotifyStartProcess()
        {

        }
    }
}
