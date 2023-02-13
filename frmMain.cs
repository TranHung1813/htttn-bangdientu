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

        private Page_VideoScreen page_VideoScreen = new Page_VideoScreen();
        public frmMain()
        {
            InitializeComponent();

            //axWindowsMediaPlayer1.fullScreen = true;

            //this.txtThongBao.SetSpeed = 1;
            //this.txtThongBao.Start();

            //this.txtVanBan.SetSpeed = 1;
            //this.txtVanBan.Start();

            InitParameters();

            ShowRTC(); // Hien thoi gian
            Add_UserControl(page_VideoScreen);
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

        private void Add_UserControl(UserControl uc)
        {
            ContainerPanel.Controls.Clear();
            ContainerPanel.Controls.Add(uc);
            uc.Dock = DockStyle.Fill;
            uc.BringToFront();
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

                    //txtThongBao.Text = payload.BanTinThongBao;
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
            Add_UserControl(page_VideoScreen);
            page_VideoScreen.ShowVideo(Url);
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
