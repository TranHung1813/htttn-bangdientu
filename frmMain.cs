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
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Management;

namespace Display
{
    public partial class frmMain : Form
    {
        private Message mqttMessage;

        private byte[] PingPacket = { 0x03, 0x00 };
        private byte[] PongPacket = { 0x03, 0x01, 0x02, 0x03 };
        //private byte[] ConnectedPacket = { 0x03, 0x01, 0x00 };
        private byte[] OpenPacket = { 0x03, 0x02, 0x00 };
        private byte[] ClosePacket = { 0x03, 0x02, 0x01 };

        private const int _Baudrate = 115200;
        private const int _Databit = 8;
        private const StopBits _StopBit = StopBits.One;
        private const Parity _parity = Parity.None;

        private bool _isRelayOpened = false;
        private bool _isModule_Connected = false;

        private string _VideoUrl = "";
        private string _TxtThongBao = "";
        private string _TxtVanBan = "";

        private static int MAX_BUFFER_SIZE = 1024;
        ComPort Uart2Com = new ComPort(115200, MAX_BUFFER_SIZE, false);

        private CustomForm customForm = new CustomForm();
        private DefaultForm defaultForm = new DefaultForm();

        private const int DEFAULT_FORM = 1;
        private const int CUSTOM_FORM = 2;
        private int CurrentForm = 0;

        public int WM_SYSCOMMAND = 0x0112;
        public int SC_MONITORPOWER = 0xF170;

        // bytech@2020
        //1. Thread cho Text Overlay chay doc lap (tạm ổn)
        //2. Debug voi man hinh lon (done)
        //3. Them Page hien multi Image (done)
        //4. Test voi ban tin Server (can modifi lai ban tin giao tiep)
        //5. Code phan dieu khien voi Relay
        //6. Xu ly phan Load anh (Load cham?, load xong bi nháy đen 1 phát) (done)
        //7. Thiet ke lai giao dien Text Overlay (done)
        //8. Thiet ke lai giao dien Text: Phan chia Text thành 2 phần: Title, Content (done)
        //9. Check lai phan enable Scroll cua TextEx va TextEx2 khi thay doi do dai Text (done)
        //10. Stop tat ca cac Timer sau khi da su dung xong (tạm ổn)
        //11. Khởi động cungd WIndow (done)
        //12. tự khỏi động lại khi crash hay khi có lệnh từ server. (1/2)
        //13. Bug lâu lâu bấm lại F1 bị lỗi (done)
        //14. Bug PanelEx không quay lại khi chạy hết (done)
        //15. Bug Page_Multi_Image chạy chưa Smooth (done)
        //16. Bug am thanh quay ve F1 khong tat (done)

        //17. Xem lai Growing label
        //18. Ve lai Text
        //19. Code phan Porting voi VDK
        public frmMain()
        {
            InitializeComponent();

            InitParameters();

            //Init key press event
            this.KeyPreview = true;
            this.KeyUp += FrmMain_KeyUp;

            Uart2Com.NotifySendPacket += Notify_SendPacket;
            Uart2Com.NotifyRecvPacket += Notify_RecvPacket;
            Uart2Com.StatusConnection += Notify_StatusConnection;

            Uart2Com.Setup_InfoComport(_Baudrate, _Databit, _StopBit, _parity);
            //Uart2Com.FindComPort(PingPacket, PingPacket.Length, PongPacket, PongPacket.Length, 1000, true);

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
                        customForm.Close();
                        Add_UserControl(defaultForm);
                        CurrentForm = DEFAULT_FORM;
                        defaultForm.Set_Infomation(_TxtThongBao, _TxtVanBan, _VideoUrl);
                    }
                    break;

                case Keys.F2:
                    // Chuyển sang Form Custom
                    if (CurrentForm != CUSTOM_FORM)
                    {
                        defaultForm.Close();
                        Add_UserControl(customForm);
                        CurrentForm = CUSTOM_FORM;
                        customForm.ShowText(_TxtThongBao, _TxtVanBan);
                    }
                    break;
                case Keys.F3:
                    // Chuyen sang tab xem Video
                    if (CurrentForm == CUSTOM_FORM)
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
                        //pictureBox1.Load("https://fastly.picsum.photos/id/1026/1200/600.jpg?hmac=JwvbmRinwixVccKkAI-YCSQMCEFZOVWnGE6iReEqEAc");
                        //pictureBox2.Load("https://i2.wp.com/beebom.com/wp-content/uploads/2016/01/Reverse-Image-Search-Engines-Apps-And-Its-Uses-2016.jpg");
                        string[] ImageURLs = new string[4];
                        //ImageURLs[0] = "https://images.unsplash.com/photo-1608229191360-7064b0afa639?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=800&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIyMjk2Ng&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1900";
                        //ImageURLs[1] = "https://images.unsplash.com/photo-1597429287872-86c80ff53f38?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=800&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIyMzAyMg&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1900";
                        //ImageURLs[2] = "https://images.unsplash.com/photo-1597428963794-aba7182f3abf?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=768&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIxMTE5MQ&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1366";
                        //ImageURLs[3] = "https://images.unsplash.com/photo-1631193079266-4af74b218c86?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=800&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIyMjkwOQ&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1900";

                        ImageURLs[0] = "http://via.placeholder.com/1366x761&text=image1";
                        ImageURLs[1] = "http://via.placeholder.com/1366x762&text=image2";
                        ImageURLs[2] = "http://via.placeholder.com/1366x763&text=image3";
                        ImageURLs[3] = "http://via.placeholder.com/1366x764&text=image4";

                        customForm.Show_Multi_Image(ImageURLs, 4);
                    }
                    break;
                case Keys.F8:
                    SendMessage(this.Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, 2);
                    break;

                case Keys.F12:
                    // Open Relay
                    OpenRelay();
                    break;

                case Keys.F11:
                    // Close Relay
                    Close_Relay();
                    break;
            }

        }
        private void Close_Relay()
        {
            // Close Relay
            Uart2Com.SendPacket(Uart2Com.GetChanelFree(), ClosePacket, ClosePacket.Length);
            _isRelayOpened = false;
        }
        private void OpenRelay()
        {
            // Open Relay
            Uart2Com.SendPacket(Uart2Com.GetChanelFree(), OpenPacket, OpenPacket.Length);
            _isRelayOpened = true;
        }
 

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        private void Add_UserControl(Control uc)
        {
            panelContainer.Controls.Clear();
            panelContainer.Controls.Add(uc);
            //uc.Dock = DockStyle.Fill;
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

            //MessageBox.Show(panelContainer.Width.ToString() + panelContainer.Height.ToString()+ "," + this.Width.ToString() + this.Height.ToString());
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

        private void Notify_SendPacket(object sender, StatusSendPacket e)
        { 
            if (e.Status == ComPort.E_OK)
            {
                //MessageBox.Show("Send" + Encoding.Default.GetString(e.DataSend, 0, e.Length));
            }
        }

        private void Notify_RecvPacket(object sender, RecvPacket e)
        {
            if(ComPort.E_OK == Uart2Com.CompareByteArray(e.DataRecv, e.Length, PongPacket, PongPacket.Length))
            {
                if(e.DataRecv[4] == 0x00)
                {
                    if(_isRelayOpened == false)
                    {
                        Close_Relay();
                    }
                }
                else if(e.DataRecv[4] == 0x01)
                {
                    if (_isRelayOpened == true)
                    {
                        OpenRelay();
                    }
                }
            }
            //MessageBox.Show("Send" + Encoding.Default.GetString(e.DataRecv, 0, e.Length));
        }
        private void Notify_StatusConnection(object sender, NotifyStatusConnection e)
        {
            if (e.Status == ComPort.E_OK)
            {
                //Uart2Com.SendPacketPing(PingPacket, PingPacket.Length, true, 2000);
            }
            else
            {

            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Utility.fitFormToScreen(this, 768, 1366);
            defaultForm.DefaultForm_FitToContainer(panelContainer.Height, panelContainer.Width);
            customForm.CustomForm_FitToContainer(panelContainer.Height, panelContainer.Width);
        }

        private void Timer_FindComPort_Tick(object sender, EventArgs e)
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort");
            bool isDriver_Availabel = false;
            string ComName = "";
            foreach (ManagementObject result in searcher.Get())
            {
                // Look at result["Caption"].ToString() and result["DeviceID"].ToString()
                if(result["Caption"].ToString().Contains("CP210x"))
                {
                    ComName = result["DeviceID"].ToString();
                    isDriver_Availabel = true;
                    break;
                }
            }
            if(isDriver_Availabel == true)
            {
                if(_isModule_Connected == false)
                {
                    int ret = ComPort.E_NOT_OK;
                    try
                    {
                        ret = Uart2Com.SetupComPort(ComName, _Baudrate, _Databit, _StopBit, _parity);
                    }
                    catch { }
                    if (ret == ComPort.E_OK)
                    {
                        _isModule_Connected = true;
                        Uart2Com.SendPacketPing(PingPacket, PingPacket.Length, true, 2000);
                    }
                }
            }
            else
            {
                _isModule_Connected = false;
                Uart2Com.Close();
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
