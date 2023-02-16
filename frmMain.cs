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
        private int TabPageID = 0;

        public static int parentX, parentY;
        public static int Form_Height, Form_Width;

        private Page_VideoScreen page_VideoScreen = new Page_VideoScreen();
        private Page_Text page_Text = new Page_Text();
        private Page_Image page_Image = new Page_Image();

        private PanelContainer panel_Video;
        private PanelContainer panel_Text;
        private PanelContainer panel_Image;
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
                    if (TabPageID != PAGE_VIDEO)
                    {
                        // Chuyen sang tab xem Video
                        ShowVideo(_VideoUrl);
                    }
                    break;
                case Keys.F4:
                    if (TabPageID != PAGE_TEXT)
                    {
                        // Chuyen sang tab Text
                        ShowText(_TxtThongBao);
                    }
                    break;
                case Keys.F5:
                    if (TabPageID != PAGE_IMAGE)
                    {
                        // Chuyen sang tab Image
                        ShowImage("");
                    }
                    break;

                case Keys.F6:
                    //FormLoad_Image modal = new FormLoad_Image();
                    //modal.ShowDialog();
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
            guna2Transition1.ShowSync(panel, true);
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
                    _VideoUrl = payload.VideoUrl;
                    ShowVideo(_VideoUrl);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "ProcessNewMessage");
            }
        }
        private void ShowVideo(string Url)
        {
            page_VideoScreen.ShowVideo(Url);
            ShowPanel(panel_Video, page_VideoScreen);
            TabPageID = PAGE_VIDEO;
        }
        private void ShowText(string Text)
        {
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
