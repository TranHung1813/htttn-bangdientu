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

        public frmMain()
        {
            InitializeComponent();

            axWindowsMediaPlayer1.URL = "https://dev-data.radiotech.vn/media/Yêu dấu theo gió bay.mp4";
            //axWindowsMediaPlayer1.URL = "https://dev-data.radiotech.vn/media/041d3e6b-29af-4dbc-9ee1-94655ddec328.mp4";
            axWindowsMediaPlayer1.Ctlcontrols.play();
            axWindowsMediaPlayer1.settings.setMode("loop", true);
            axWindowsMediaPlayer1.uiMode = "none";

            this.txtThongBao.SetSpeed = 1;
            this.txtThongBao.Start();

            this.txtVanBan.SetSpeed = 1;
            this.txtVanBan.Start();

            InitParameters();

            ShowRTC(); // Hien thoi gian
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

                    txtThongBao.Text = payload.BanTinThongBao;
                    txtVanBan.Text = payload.BanTinVanBan;
                    axWindowsMediaPlayer1.URL = payload.VideoUrl;
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "ProcessNewMessage");
            }
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
            lbTime.Text = DayNumber + ", " + DateTime.Now.Date.ToString("d/M/yyyy") + "  |  " + DateTime.Now.ToShortTimeString();
        }
    }

    public class DisplayMessage
    {
        public string BanTinThongBao { get; set; }
        public string BanTinVanBan { get; set; }
        public string VideoUrl { get; set; }
    }
}
