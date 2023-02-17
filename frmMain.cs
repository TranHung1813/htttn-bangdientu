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
        private string _TxtVanBan = "";

        private CustomForm customForm = new CustomForm();

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

            //Init key press event
            this.KeyPreview = true;
            this.KeyUp += FrmMain_KeyUp;

            Add_UserControl(customForm);

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
                    customForm.ShowVideo(_VideoUrl);
                    break;

                case Keys.F4:
                    // Chuyen sang tab Text
                    customForm.ShowText(_TxtThongBao);
                    break;

                case Keys.F5:
                    // Chuyen sang tab Image
                    customForm.ShowImage("");
                    break;

                case Keys.F6:
                    //Show Text Overlay
                    customForm.Show_TextOverlay("");
                    break;

                case Keys.F7:
                    //Show Multi Image
                    string[] ImageURLs = new string[4];
                    ImageURLs[0] = "http://placehold.it/120x120&text=image1";
                    ImageURLs[1] = "http://placehold.it/120x120&text=image2";
                    ImageURLs[2] = "http://placehold.it/120x120&text=image3";
                    ImageURLs[3] = "http://placehold.it/120x120&text=image4";

                    customForm.Show_Multi_Image(ImageURLs, 4);
                    break;
            }
        }

        private void Add_UserControl(Control uc)
        {
            panelContainer.Controls.Clear();
            panelContainer.Controls.Add(uc);
            uc.Dock = DockStyle.Fill;
            uc.BringToFront();
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

                    _TxtThongBao = payload.BanTinVanBan;
                    //ShowText(_TxtThongBao);
                    _TxtVanBan = payload.BanTinVanBan;
                    _VideoUrl = payload.VideoUrl;

                    customForm.ShowVideo(_VideoUrl);
                    //customForm.ShowVideo("https://live.hungyentv.vn/hytvlive/tv1live.m3u8");
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "ProcessNewMessage");
            }
        }
    }

    public class DisplayMessage
    {
        public string BanTinThongBao { get; set; }
        public string BanTinVanBan { get; set; }
        public string VideoUrl { get; set; }
    }
}
