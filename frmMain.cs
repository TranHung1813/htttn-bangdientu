using MQTTnet.Client;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
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
    public partial class frmMain : Form
    {
        private Message mqttMessage;
        private string _VideoUrl = "";
        private string _TxtThongBao = "";

        private const int PAGE_VIDEO = 1;
        private const int PAGE_TEXT = 2;
        private const int PAGE_IMAGE = 3;
        private const int PAGE_MULTI_IMAGE = 4;
        private int TabPageID = 0;

        public static int parentX, parentY;
        public static int Form_Height, Form_Width;

        private Page_VideoScreen page_VideoScreen = new Page_VideoScreen();
        private Page_Text page_Text = new Page_Text();
        private Page_Image page_Image = new Page_Image();
        private TextOverlay text_Overlay = new TextOverlay();

        private PanelContainer panel_Video;
        private PanelContainer panel_Text;
        private PanelContainer panel_Image;
        private PanelContainer panel_Multi_Image;

        //1. Thread cho Text Overlay chay doc lap
        //2. Debug voi man hinh lon
        //3. Them Page hien multi Image
        //4. Test voi ban tin Server (can modifi lai ban tin giao tiep)
        //5. Code phan dieu khien voi Relay
        //6. Xu ly phan Load anh (Load cham?, load xong bi nháy đen 1 phát)
        //7. Thiet ke lai giao dien Text Overlay
        //8. Thiet ke lai giao dien Text: Phan chia Text thành 2 phần: Title, Content
        public frmMain()
        {
            InitializeComponent();

            InitParameters();

            ShowRTC(); // Hien thoi gian

            //Init key press event
            this.KeyPreview = true;
            this.KeyUp += FrmMain_KeyUp;

            panel_Video = new PanelContainer(this);
            panel_Text = new PanelContainer(this);
            panel_Image = new PanelContainer(this);
            panel_Multi_Image = new PanelContainer(this);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }

            base.OnKeyUp(e);
        }
        private void FrmMain_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F3:
                    // Chuyen sang tab xem Video
                    ShowVideo(_VideoUrl);
                    break;

                case Keys.F4:
                    // Chuyen sang tab Text
                    ShowText(_TxtThongBao);
                    break;

                case Keys.F5:
                    // Chuyen sang tab Image
                    ShowImage("");
                    break;

                case Keys.F6:
                    //Show Text Overlay
                    Show_TextOverlay("Thông báo", "");
                    break;

                case Keys.F7:
                    //Show Multi Image
                    Page_Multi_Image page_Multi_Image = new Page_Multi_Image();
                    ShowPanel(panel_Multi_Image, page_Multi_Image);
                    TabPageID = PAGE_MULTI_IMAGE;

                    string[] ImageURLs = new string[4];
                    ImageURLs[0] = "http://placehold.it/120x120&text=image1";
                    ImageURLs[1] = "http://placehold.it/120x120&text=image2";
                    ImageURLs[2] = "http://placehold.it/120x120&text=image3";
                    ImageURLs[3] = "http://placehold.it/120x120&text=image4";

                    page_Multi_Image.Show_Multi_Image(ImageURLs, 4);
                    break;
            }
        }

        private void InitParameters()
        {
            mqttMessage = new Message(Properties.Settings.Default.MqttAddress, Properties.Settings.Default.MqttPort,
                Properties.Settings.Default.MqttUserName, Properties.Settings.Default.MqttPassword, Properties.Settings.Default.ClientId);

            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                        .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.File(@"Log\Info-.txt", rollingInterval: RollingInterval.Day))
                        .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.File(@"Log\Debug-.txt", rollingInterval: RollingInterval.Day))
                        .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.File(@"Log\Warning-.txt", rollingInterval: RollingInterval.Day))
                        .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File(@"Log\Error-.txt", rollingInterval: RollingInterval.Day))
                    .CreateLogger();
        }

        private void ShowPanel(PanelContainer panel, UserControl uc)
        {
            panel.Controls.Clear();
            panel.Controls.Add(uc);
            uc.Dock = DockStyle.Fill;
            uc.BringToFront();

            switch (TabPageID)
            {
                case PAGE_IMAGE:
                    guna2Transition1.HideSync(panel_Image);
                    panel_Image.Visible = false;
                    break;

                case PAGE_TEXT:
                    guna2Transition1.HideSync(panel_Text);
                    panel_Text.Visible = false;
                    break;

                case PAGE_VIDEO:
                    page_VideoScreen.StopVideo();
                    guna2Transition1.HideSync(panel_Video);
                    panel_Video.Visible = false;
                    break;
            }
            try
            {
                guna2Transition1.ShowSync(panel, true);
            }
            catch
            { }
        }

        private void tick_Tick(object sender, EventArgs e)
        {
            ProcessNewMessage();
            SendHeartBeatTick();
        }

        private void SendHeartBeatTick()
        {

        }

        private void ProcessNewMessage()
        {
            try
            {
                var newMessage = mqttMessage.GetMessage;
                if(newMessage != null)
                {
                    var topic = newMessage.Topic;
                    var payload = JsonConvert.DeserializeObject<DisplayMessage>(Encoding.UTF8.GetString(newMessage.Payload));

                    _TxtThongBao = payload.BanTinVanBan;
                    //ShowText(_TxtThongBao);
                    //txtVanBan.Text = payload.BanTinVanBan;
                    //_VideoUrl = payload.VideoUrl;


                    ShowVideo("https://live.hungyentv.vn/hytvlive/tv1live.m3u8");
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "ProcessNewMessage");
            }
        }

    //-----------------------------------API Show Page Text, Video, Image, Text Overlay-----------------------------//
        private void ShowVideo(string Url)
        {
            ShowPanel(panel_Video, page_VideoScreen);
            page_VideoScreen.ShowVideo(Url);
            TabPageID = PAGE_VIDEO;
        }
        private void ShowText(string Text)
        {
            page_Text = new Page_Text();
            page_Text.ShowText(Text);
            ShowPanel(panel_Text, page_Text);
            TabPageID = PAGE_TEXT;
        }
        private void ShowImage(string ImageURL)
        {
            page_Image.ShowImage(ImageURL);
            ShowPanel(panel_Image, page_Image);
            TabPageID = PAGE_IMAGE;
        }
        private void Show_TextOverlay(string Title, string Content)
        {
            //panel_TextOverlay.Visible = true;

            panel_TextOverlay.Controls.Clear();
            panel_TextOverlay.Controls.Add(text_Overlay);
            text_Overlay.Dock = DockStyle.Fill;
            text_Overlay.BringToFront();

            panel_TextOverlay.BringToFront();

            try
            {
                guna2Transition1.ShowSync(panel_TextOverlay, true);
            }
            catch { }

            text_Overlay.ShowTextOverlay(Content);
        }

    //-----------------------------------------------------------------------------------------------------//
        private void timer_GetRTC_Tick(object sender, EventArgs e)
        {
            ShowRTC(); // Hien thoi gian
        }

        private void ShowRTC()
        {
            string DayNumber = "";
            int day1 = (int)DateTime.Now.DayOfWeek;
            if (day1 == 0) DayNumber = "Chủ nhật";
            else DayNumber = "Thứ " + (day1 + 1).ToString();
            //lbTime.Text = DayNumber + ", " + DateTime.Now.Date.ToString("d/M/yyyy") + "  |  " + DateTime.Now.ToShortTimeString();
        }

    }

    public class DisplayMessage
    {
        public string BanTinThongBao { get; set; }
        public string BanTinVanBan { get; set; }
        public string VideoUrl { get; set; }
    }
}
