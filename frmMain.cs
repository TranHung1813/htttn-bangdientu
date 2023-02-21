using Microsoft.Win32;
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
        private DefaultForm defaultForm = new DefaultForm();

        private const int DEFAULT_FORM = 1;
        private const int CUSTOM_FORM = 2;
        private int CurrentForm = 0;

        //1. Thread cho Text Overlay chay doc lap (tạm ổn)
        //2. Debug voi man hinh lon
        //3. Them Page hien multi Image
        //4. Test voi ban tin Server (can modifi lai ban tin giao tiep)
        //5. Code phan dieu khien voi Relay
        //6. Xu ly phan Load anh (Load cham?, load xong bi nháy đen 1 phát) (done)
        //7. Thiet ke lai giao dien Text Overlay (done)
        //8. Thiet ke lai giao dien Text: Phan chia Text thành 2 phần: Title, Content (done)
        //9. Check lai phan enable Scroll cua TextEx va TextEx2 khi thay doi do dai Text (done)
        //10. Stop tat ca cac Timer sau khi da su dung xong (tạm ổn)
        //11. Khởi động cungd WIndow 
        //12. tự khỏi động lại khi crash hay khi có lệnh từ server.
        public frmMain()
        {
            InitializeComponent();

            InitParameters();

            //Init key press event
            this.KeyPreview = true;
            this.KeyUp += FrmMain_KeyUp;

            RegisterInStartup(true);
            //Add_UserControl(customForm);
            //Add_UserControl(defaultForm);
            //CurrentForm = DEFAULT_FORM;
        }
        private void RegisterInStartup(bool isChecked)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (isChecked)
            {
                registryKey.SetValue("ApplicationName", Application.ExecutablePath);
            }
            else
            {
                registryKey.DeleteValue("ApplicationName");
            }
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
                case Keys.F1:
                    // Chuyển sang Form Default
                    if (CurrentForm != DEFAULT_FORM)
                    {
                        Add_UserControl(defaultForm);
                        CurrentForm = DEFAULT_FORM;
                    }
                    break;

                case Keys.F2:
                    // Chuyển sang Form Custom
                    if (CurrentForm != CUSTOM_FORM)
                    {
                        Add_UserControl(customForm);
                        CurrentForm = CUSTOM_FORM;
                        customForm.ShowText(_TxtThongBao, _TxtVanBan);
                    }
                    break;
                case Keys.F3:
                    // Chuyen sang tab xem Video
                    if(CurrentForm == CUSTOM_FORM)
                        customForm.ShowVideo(_VideoUrl);
                    break;

                case Keys.F4:
                    // Chuyen sang tab Text
                    if (CurrentForm == CUSTOM_FORM)
                        customForm.ShowText(_TxtThongBao, _TxtVanBan);
                    break;

                case Keys.F5:
                    // Chuyen sang tab Image
                    if (CurrentForm == CUSTOM_FORM)
                        customForm.ShowImage("");
                    break;

                case Keys.F6:
                    //Show Text Overlay
                    if (CurrentForm == CUSTOM_FORM)
                        customForm.Show_TextOverlay("");
                    break;

                case Keys.F7:
                    //Show Multi Image
                    if (CurrentForm == CUSTOM_FORM)
                    {
                        string[] ImageURLs = new string[4];
                        ImageURLs[0] = "http://placehold.it/120x120&text=image1";
                        ImageURLs[1] = "http://placehold.it/120x120&text=image2";
                        ImageURLs[2] = "http://placehold.it/120x120&text=image3";
                        ImageURLs[3] = "http://placehold.it/120x120&text=image4";

                        customForm.Show_Multi_Image(ImageURLs, 4);
                    }
                    break;
            }
        }

        private void Add_UserControl(Control uc)
        {
            panelContainer.Controls.Clear();
            panelContainer.Controls.Add(uc);
            uc.Dock = DockStyle.Fill;
            uc.BringToFront();
            uc.Focus();
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

                    _TxtThongBao = payload.BanTinThongBao;
                    //ShowText(_TxtThongBao);
                    _TxtVanBan = payload.BanTinVanBan;
                    _VideoUrl = payload.VideoUrl;

                    //customForm.ShowVideo(_VideoUrl);
                    // Chuyển sang Form Default
                    if (CurrentForm != DEFAULT_FORM)
                    {
                        Add_UserControl(defaultForm);
                        CurrentForm = DEFAULT_FORM;
                    }
                    defaultForm.Set_Infomation(_TxtThongBao, _TxtVanBan, _VideoUrl);
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
