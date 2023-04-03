using Microsoft.Win32;
using Newtonsoft.Json;
using Rijndael256;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Display
{
    public partial class frmMain : Form
    {
        private Message mqttMessage;
        string password = "bytech@2020";  // Khóa để mã hóa
        string GUID_Value = "";

        private const int DEFAULT_LENGTH_GUID = 15;

        private byte[] PingPacket = { 0x03, 0x01 };
        private byte[] PongPacket = { 0x03, 0x01, 0x02, 0x03 };
        //private byte[] ConnectedPacket = { 0x03, 0x01, 0x00 };
        private byte[] OpenPacket = { 0x03, 0x02, 0x00 };
        private byte[] ClosePacket = { 0x03, 0x02, 0x01 };

        private const int _Baudrate = 115200;
        private const int _Databit = 8;
        private const StopBits _StopBit = StopBits.One;
        private const Parity _parity = Parity.None;

        private bool _isRelayOpened = true;
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

        ScheduleHandle scheduleHandle = new ScheduleHandle();

        private string PathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"ScreenResolution.");

        public int WM_SYSCOMMAND = 0x0112;
        public int SC_MONITORPOWER = 0xF170;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // bytech@2020
        //1. Thread cho Text Overlay chay doc lap (tạm ổn)
        //2. Debug voi man hinh lon (done)
        //3. Them Page hien multi Image (done)
        //4. Test voi ban tin Server (can modifi lai ban tin giao tiep)
        //5. Code phan dieu khien voi Relay(done)
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

        //17. Xem lai Growing label (done)
        //18. Ve lai Text (done)
        //19. Code phan Porting voi VDK (done)

        //20. Xu ly loi khi 2 moc thoi gian trung nhau (done)
        //21. Thử với url video lối => xem có bị lỗi k (done, k loi)
        //22. Duration với video stream (done)
        //23. Schedule theo ngay trong tuan (done)
        //24. Chuyen thoi gian Schedule ve thoi gian trong ngay (số giây từ 0h) (done)
        //25. Lưu lại Time Schedule để sau khi app crash vẫn chạy bình thường
        //26. Tính lại Time List theo ngày = số giây từ 0h00 T2 đến thời điểm phát(T2, T5, T7,...) (done)
        //27. Xóa bản tin theo ID
        //28. Text run không mượt trên máy tính Mini

        //29. Kiểm tra: Không có gì để Show => Tắt màn hình (cả Default, Custom Form) (done)
        //30. Xử lý biến Toàn màn hình (done)
        //31. Xóa dữ liệu Show trên màn hình khi: hết Duration (done), hết ValidTime
        //32. Tự động restart máy lúc 2-3h sáng
        public frmMain()
        {
            InitializeComponent();
            InitParameters();

            //SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            //Init key press event
            this.KeyPreview = true;
            this.KeyUp += FrmMain_KeyUp;

            Uart2Com.NotifySendPacket += Notify_SendPacket;
            Uart2Com.NotifyRecvPacket += Notify_RecvPacket;
            //Uart2Com.StatusConnection += Notify_StatusConnection;

            scheduleHandle.NotifyTime2Play += ScheduleHandle_NotifyTime2Play;

            Uart2Com.Setup_InfoComport(_Baudrate, _Databit, _StopBit, _parity);
            //Uart2Com.FindComPort(PingPacket, PingPacket.Length, PongPacket, PongPacket.Length, 1000, true);

            Log.Information("App Start!");
        }

        private void GUID_Handle()
        {
            //opening the subkey  
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BDT_Settings\GUID");

            //if it does exist, retrieve the stored values  
            if (key != null)
            {
                try
                {
                    GUID_Value = Rijndael.Decrypt(key.GetValue("GUID").ToString(), password, KeySize.Aes256);
                    //string Date = Rijndael.Decrypt(key.GetValue("Date Generate").ToString(), password, KeySize.Aes256);
                    //string MAC = Rijndael.Decrypt(key.GetValue("MAC Address").ToString(), password, KeySize.Aes256);
                    Log.Information("Decode Key: {A}", GUID_Value);

                    if (GUID_Value.Length != DEFAULT_LENGTH_GUID)
                    {
                        Log.Error("GUIDLength_not_Fit: {A}", GUID_Value);
                        Generate_NewKey();
                    }

                    Copy2ClipBoard(GUID_Value);
                }
                catch
                {
                    Log.Error("Read_GUID_Fail");
                    Generate_NewKey();
                }

                key.Close();
            }
            else
            {
                Log.Error("Key_not_Exist");
                Generate_NewKey();
            }
        }
        private void Copy2ClipBoard(string value)
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetDataObject(value, false, 5, 200);
                //Clipboard.SetText(value);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CopyGUID_to_ClipBoard");
            }
        }
        private void Generate_NewKey()
        {
            try
            {
                RegistryKey new_key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BDT_Settings\GUID");

                GUID_Value = GetNewGUID();
                new_key.SetValue("GUID", Rijndael.Encrypt(GUID_Value, password, KeySize.Aes256));
                new_key.SetValue("Date Generate", Rijndael.Encrypt(DateTime.UtcNow.Date.ToString("dd/MM/yyyy"), password, KeySize.Aes256));
                new_key.SetValue("MAC Address", Rijndael.Encrypt(GetDefaultMacAddress().ToString(), password, KeySize.Aes256));

                Copy2ClipBoard(GUID_Value);

                Log.Information("Generate NewKey: {A}", GUID_Value);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Generate_NewKey");
            }
        }
        public string GetNewGUID()
        {
            string guid_str = "";
            // Get the GUID
            byte[] guid_ByteArray = Guid.NewGuid().ToByteArray();
            int[] guid_int = new int[guid_ByteArray.Length];
            for (int CountByte = 0; CountByte < guid_ByteArray.Length; CountByte++)
            {
                guid_int[CountByte] = guid_ByteArray[CountByte] % 9;
            }
            for (int CountInt = 0; CountInt < guid_int.Length - 1; CountInt++)
            {
                int value = (guid_int[CountInt] + guid_int[CountInt]) % 9;
                guid_str += value.ToString();
            }
            // Return  the GUID
            return guid_str;
        }
        public string GetDefaultMacAddress()
        {
            Dictionary<string, long> macAddresses = new Dictionary<string, long>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    macAddresses[nic.GetPhysicalAddress().ToString()] = nic.GetIPStatistics().BytesSent + nic.GetIPStatistics().BytesReceived;
            }
            long maxValue = 0;
            string mac = "";
            foreach (KeyValuePair<string, long> pair in macAddresses)
            {
                if (pair.Value > maxValue)
                {
                    mac = pair.Key;
                    maxValue = pair.Value;
                }
            }
            return mac;
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
                    }
                    break;

                case Keys.F2:
                    // Chuyển sang Form Custom
                    if (CurrentForm != CUSTOM_FORM)
                    {
                        defaultForm.Close();
                        Add_UserControl(customForm);
                        CurrentForm = CUSTOM_FORM;
                        //customForm.ShowText(_TxtThongBao, _TxtVanBan);
                    }
                    break;
                case Keys.F3:
                    // Chuyen sang tab xem Video
                    if (CurrentForm == CUSTOM_FORM)
                        customForm.ShowVideo("https://live.hungyentv.vn/hytvlive/tv1live.m3u8", Duration: 25 * 1000, loopNum: 0);
                    break;

                case Keys.F4:
                    // Chuyen sang tab Text
                    if (CurrentForm == CUSTOM_FORM)
                    {
                        string txt = "RadioTech là một ứng dụng trên smartphone dùng để giám sát, điều khiển, cài đặt nội dung phát thanh của hệ thống truyền thanh ứng dụng công nghệ thông tin do Công ty Cổ phần BY TECH Việt Nam nghiên cứu và phát triển. \nĐể biết thêm thông tin chi tiết xin vui lòng liên hệ:\n Mr Biền - 0987 888 411\nHoặc các đơn vị phân phối sản phẩm Bytech trên toàn quốc\n";
                        customForm.ShowText(DisplayScheduleType.BanTinThongBao, "Hướng dẫn sử dụng phần mềm RadioTech");
                        customForm.ShowText(DisplayScheduleType.BanTinVanBan, txt);
                        //customForm.ShowText(_TxtThongBao, _TxtVanBan);
                    }
                    break;

                case Keys.F5:
                    // Chuyen sang tab Image
                    if (CurrentForm == CUSTOM_FORM)
                        customForm.ShowImage("https://i2.wp.com/beebom.com/wp-content/uploads/2016/01/Reverse-Image-Search-Engines-Apps-And-Its-Uses-2016.jpg", 20 * 1000);
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
                        ImageURLs[0] = "https://images.unsplash.com/photo-1608229191360-7064b0afa639?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=800&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIyMjk2Ng&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1900";
                        ImageURLs[1] = "https://images.unsplash.com/photo-1597429287872-86c80ff53f38?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=800&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIyMzAyMg&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1900";
                        ImageURLs[2] = "https://images.unsplash.com/photo-1597428963794-aba7182f3abf?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=768&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIxMTE5MQ&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1366";
                        ImageURLs[3] = "https://images.unsplash.com/photo-1631193079266-4af74b218c86?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=800&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIyMjkwOQ&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1900";

                        //ImageURLs[0] = "http://via.placeholder.com/1366x761&text=image1";
                        //ImageURLs[1] = "http://via.placeholder.com/1366x762&text=image2";
                        //ImageURLs[2] = "http://via.placeholder.com/1366x763&text=image3";
                        //ImageURLs[3] = "http://via.placeholder.com/1366x764&text=image4";

                        customForm.Show_Multi_Image(ImageURLs, 4);
                    }
                    break;
                case Keys.F8:
                    // Tat man hinh
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

                case Keys.S:
                    if (CurrentForm != DEFAULT_FORM)
                    {
                        Add_UserControl(defaultForm);
                        CurrentForm = DEFAULT_FORM;
                    }
                    defaultForm.Set_Infomation(DisplayScheduleType.BanTinThongBao, "“NGÀY HỘI ĐẠI ĐOÀN KẾT TOÀN DÂN TỘC”: TĂNG CƯỜNG KHỐI ĐẠI ĐOÀN KẾT TỪ MỖI CỘNG ĐỒNG DÂN CƯ");
                    defaultForm.Set_Infomation(DisplayScheduleType.BanTinVanBan, "Triển khai thực hiện nhiệm vụ “Xây dựng hệ thống thông tin nguồn và thu thập, tổng hợp, phân tích, quản lý dữ liệu, đánh giá hiệu quả hoạt động thông tin cơ sở” tại Quyết định số 135/QĐ-TTg ngày 20/01/2020 của Thủ tướng Chính phủ phê duyệt Đề án nâng cao hiệu quả hoạt động thông tin cơ sở dựa trên ứng dụng công nghệ thông tin; Bộ Thông tin và Truyền thông ban hành Hướng dẫn về chức năng, tính năng kỹ thuật của Hệ thống thông tin nguồn trung ương, Hệ thống thông tin nguồn cấp tỉnh và kết nối các hệ thống thông tin - Phiên bản 1.0 (gửi kèm theo văn bản này).");
                    defaultForm.ShowVideo(@"https://live.hungyentv.vn/hytvlive/tv1live.m3u8");
                    //defaultForm.ShowVideo(@"http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4", Duration: 120 * 1000, loopNum: 0);
                    //defaultForm.ShowImage("https://images.unsplash.com/photo-1608229191360-7064b0afa639?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=800&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIyMjk2Ng&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1900");
                    //defaultForm.Test();
                    break;

                case Keys.F:
                    Copy2ClipBoard(GUID_Value);
                    //customForm.Test();
                    break;

                case Keys.P:
                    ScheduleHandle abc = new ScheduleHandle();
                    abc.NotifyTime2Play += ScheduleHandle_NotifyTime2Play;

                    Schedule msg = new Schedule();
                    msg.id = "001";
                    msg.from = 1679892014;
                    msg.to = 1680491799;
                    msg.isActive = true;
                    msg.isDaily = true;
                    msg.days = new List<int> { 4, 5, 3, 1, 6 };
                    msg.times = new List<int> { 42060, 42180, 42240 };
                    msg.idleTime = 1;
                    msg.loops = 0;
                    msg.duration = 50 * 1000;
                    msg.songs = new List<string> { "“NGÀY HỘI ĐẠI ĐOÀN KẾT TOÀN DÂN TỘC”: TĂNG CƯỜNG KHỐI ĐẠI ĐOÀN KẾT TỪ MỖI CỘNG ĐỒNG DÂN CƯ", "Triển khai thực hiện nhiệm vụ “Xây dựng hệ thống thông tin nguồn và thu thập, tổng hợp, phân tích, quản lý dữ liệu, đánh giá hiệu quả hoạt động thông tin cơ sở” tại Quyết định số 135/QĐ-TTg ngày 20/01/2020 của Thủ tướng Chính phủ phê duyệt Đề án nâng cao hiệu quả hoạt động thông tin cơ sở dựa trên ứng dụng công nghệ thông tin; Bộ Thông tin và Truyền thông ban hành Hướng dẫn về chức năng, tính năng kỹ thuật của Hệ thống thông tin nguồn trung ương, Hệ thống thông tin nguồn cấp tỉnh và kết nối các hệ thống thông tin - Phiên bản 1.0 (gửi kèm theo văn bản này).", @"https://live.hungyentv.vn/hytvlive/tv1live.m3u8" };
                    abc.Schedule(msg);

                    Schedule msg2 = new Schedule();
                    msg2.id = "002";
                    msg2.from = 1679892014;
                    msg2.to = 1680491799;
                    msg2.isActive = true;
                    msg2.isDaily = false;
                    msg2.times = new List<int> { 42120, 35940, 42210 };
                    msg2.idleTime = 2;
                    msg2.loops = 5;
                    msg2.duration = 500 * 1000;
                    msg2.songs = new List<string> { "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.", "“NGÀY HỘI ĐẠI ĐOÀN KẾT TOÀN DÂN TỘC”: TĂNG CƯỜNG KHỐI ĐẠI ĐOÀN KẾT TỪ MỖI CỘNG ĐỒNG DÂN CƯ", "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4" };
                    abc.Schedule(msg2);

                    Schedule msg3 = new Schedule();
                    msg3.id = "002";
                    msg3.isActive = false;
                    abc.Schedule(msg3);
                    break;
                case Keys.C:
                    //Test Crash
                    Uri uri = new Uri(null);
                    break;
            }
        }
        private void ScheduleHandle_NotifyTime2Play(object sender, NotifyTime2Play e)
        {
            if (e.FullScreen == false)
            {
                // Chuyển sang Form Default
                if (CurrentForm != DEFAULT_FORM)
                {
                    Add_UserControl(defaultForm);
                    CurrentForm = DEFAULT_FORM;
                }
                if (e.ScheduleType == DisplayScheduleType.BanTinThongBao || e.ScheduleType == DisplayScheduleType.BanTinVanBan)
                {
                    Log.Information("NotifyTime2Play: {A}, Content: {B}, Color: {C}, Duration: {D}, FullScreen: {E}", e.ScheduleType, e.Text, e.ColorValue, e.Duration * 1000, e.FullScreen);
                    defaultForm.Set_Infomation(e.ScheduleType, e.Text, e.ColorValue, e.Duration * 1000);
                }
                else if (e.ScheduleType == DisplayScheduleType.BanTinVideo)
                {
                    Log.Information("NotifyTime2Play: Video: {A}, Idle: {B}, Loops: {C}, Duration: {D}, FullScreen: {E}", e.MediaUrl, e.IdleTime, e.LoopNum, e.Duration * 1000, e.FullScreen);
                    defaultForm.ShowVideo(e.MediaUrl, e.IdleTime, e.LoopNum, e.Duration * 1000);
                }
                else if (e.ScheduleType == DisplayScheduleType.BanTinHinhAnh)
                {
                    Log.Information("NotifyTime2Play: Hinh anh: {A}, Duration: {B}, FullScreen: {C}", e.MediaUrl, e.Duration * 1000, e.FullScreen);
                    defaultForm.ShowImage(e.MediaUrl, e.Duration * 1000);
                }
            }
            else
            {
                // Chuyển sang Form Custom
                if (CurrentForm != CUSTOM_FORM)
                {
                    defaultForm.Close();
                    Add_UserControl(customForm);
                    CurrentForm = CUSTOM_FORM;
                }
                if (e.ScheduleType == DisplayScheduleType.BanTinVideo)
                {
                    Log.Information("NotifyTime2Play: Video: {A}, Idle: {B}, Loops: {C}, Duration: {D}, FullScreen: {E}", e.MediaUrl, e.IdleTime, e.LoopNum, e.Duration * 1000, e.FullScreen);
                    customForm.ShowVideo(e.MediaUrl, e.IdleTime, e.LoopNum, e.Duration * 1000);
                }
                else if (e.ScheduleType == DisplayScheduleType.BanTinHinhAnh)
                {
                    Log.Information("NotifyTime2Play: Hinh anh: {A}, Duration: {B}, FullScreen: {C}", e.MediaUrl, e.Duration * 1000, e.FullScreen);
                    customForm.ShowImage(e.MediaUrl, e.Duration * 1000);
                }
                else if (e.ScheduleType == DisplayScheduleType.BanTinThongBao || e.ScheduleType == DisplayScheduleType.BanTinVanBan)
                {
                    Log.Information("NotifyTime2Play: {A}, Content: {B}, Color: {C}, Duration: {D}, FullScreen: {E}", e.ScheduleType, e.Text, e.ColorValue, e.Duration * 1000, e.FullScreen);
                    customForm.ShowText(e.ScheduleType, e.Text, e.ColorValue, e.Duration * 1000);
                }
            }
        }
        private void Close_Relay()
        {
            // Close Relay
            Log.Information("Close_Relay");
            Uart2Com.SendPacket(Uart2Com.GetChanelFree(), ClosePacket, ClosePacket.Length);
            _isRelayOpened = false;
        }
        private void OpenRelay()
        {
            // Open Relay
            Log.Information("Open_Relay");
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
            LogInit();
            GUID_Handle();
            if (GUID_Value == null || GUID_Value.Length != DEFAULT_LENGTH_GUID)
            {
                GUID_Value = Properties.Settings.Default.ClientId;
                //GUID_Value = "180116373248482";
            }
            mqttMessage = new Message(Properties.Settings.Default.MqttAddress, Properties.Settings.Default.MqttPort,
                    Properties.Settings.Default.MqttUserName, Properties.Settings.Default.MqttPassword, GUID_Value);
        }

        private void LogInit()
        {
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
                if (newMessage != null)
                {
                    var topic = newMessage.Topic;
                    string message = Encoding.UTF8.GetString(newMessage.Payload);
                    //Log.Information("ProcessNewMessage: {A}", message);
                    Log.Information("Get_NewMessage");
                    dynamic payload = JsonConvert.DeserializeObject<object>(message);

                    if (payload.BanTinThongBao != null)
                    {
                        _TxtThongBao = payload.BanTinThongBao;
                        //ShowText(_TxtThongBao);
                        _TxtVanBan = payload.BanTinVanBan;
                        _VideoUrl = payload.VideoUrl;

                        //customForm.ShowVideo(_VideoUrl);
                        // Chuyển sang Form Default
                        if (CurrentForm != DEFAULT_FORM)
                        {
                            customForm.Close();
                            Add_UserControl(defaultForm);
                            CurrentForm = DEFAULT_FORM;
                        }
                        defaultForm.Set_Infomation(DisplayScheduleType.BanTinThongBao, _TxtThongBao);
                        defaultForm.Set_Infomation(DisplayScheduleType.BanTinVanBan, _TxtVanBan);
                        //defaultForm.ShowVideo(_VideoUrl);
                        //customForm.ShowVideo("https://live.hungyentv.vn/hytvlive/tv1live.m3u8");
                    }
                    else if (payload.message != null)
                    {
                        if (payload.message.schedule != null)
                        {
                            string s = JsonConvert.SerializeObject(payload.message.schedule);
                            Schedule newSchedule_msg = JsonConvert.DeserializeObject<Schedule>(s);

                            scheduleHandle.Schedule(newSchedule_msg);
                        }
                    }
                }
            }
            catch (Exception ex)
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
            if (ComPort.E_OK == Uart2Com.CompareByteArray(e.DataRecv, e.Length, PongPacket, PongPacket.Length))
            {
                if (e.DataRecv[4] == 0x00)
                {
                    if (_isRelayOpened == false)
                    {
                        Close_Relay();
                    }
                }
                else if (e.DataRecv[4] == 0x01)
                {
                    if (_isRelayOpened == true)
                    {
                        OpenRelay();
                    }
                }
            }
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
            //Utility.fitFormToContainer(this, this.Height, this.Width, Screen.PrimaryScreen.Bounds.Size.Height, Screen.PrimaryScreen.Bounds.Size.Width);
            Utility.fitFormToScreen(this, 768, 1366);
            this.CenterToScreen();
            defaultForm.DefaultForm_FitToContainer(panelContainer.Height, panelContainer.Width);
            customForm.CustomForm_FitToContainer(panelContainer.Height, panelContainer.Width);

            Timer_MQTT.Start();
        }

        private void Timer_FindComPort_Tick(object sender, EventArgs e)
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort");
            bool isDriver_Availabel = false;
            string ComName = "";
            foreach (ManagementObject result in searcher.Get())
            {
                //Look at result["Caption"].ToString() and result["DeviceID"].ToString()
                if (result["Caption"].ToString().Contains("USB Serial Device") | result["Caption"].ToString().Contains("CP210x"))
                {
                    ComName = result["DeviceID"].ToString();
                    isDriver_Availabel = true;
                    break;
                }
            }

            if (isDriver_Availabel == true)
            {
                if (_isModule_Connected == false)
                {
                    int ret = ComPort.E_NOT_OK;
                    try
                    {
                        ret = Uart2Com.SetupComPort(ComName, _Baudrate, _Databit, _StopBit, _parity);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Uart2Com.SetupComPort");
                    }
                    if (ret == ComPort.E_OK)
                    {
                        Log.Information("Module Relay Connected: {A}", ComName);
                        _isModule_Connected = true;
                        //Uart2Com.SendPacketPing(PingPacket, PingPacket.Length, true, 1500);
                        SendPing(2000);
                    }
                }
            }
            else
            {
                _isModule_Connected = false;
                Uart2Com.Close();
            }
        }
        private void SendPing(int Interval)
        {
            try
            {
                Timer_SendPing.Stop();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Timer_SendPing.Stop");
            }
            Timer_SendPing.Interval = Interval;
            Timer_SendPing.Start();
        }
        private void Timer_SendPing_Tick(object sender, EventArgs e)
        {
            Uart2Com.SendPacket(Uart2Com.GetChanelFree(), PingPacket, PingPacket.Length);
        }
    }

    public class DisplayMessage
    {
        public string BanTinThongBao { get; set; }
        public string BanTinVanBan { get; set; }
        public string VideoUrl { get; set; }
    }

    public class Schedule
    {
        public string id { get; set; }     //Schedule ID, định danh các schedule khác nhau
        public long createdTime { get; set; }        //Thời điểm tạo lịch (UTC second)
        public int duration { get; set; }       //Thời lượng phát (s) -> bỏ qua nếu có set loopNum
        public int loops { get; set; }        //Số lần phát
        public int idleTime { get; set; }       //Thời gian nghỉ giữa 2 lần phát (s)
        public bool isDaily { get; set; }    //Phát lặp lại hàng ngày
        public bool isBroadcast { get; set; }    //Phát quảng bá cho cả room cùng nghe
        public long from { get; set; }          //Có hiệu lực từ thời điểm (UTC second)
        public long to { get; set; }            //Có hiệu lực đến thời điểm (UTC second)
        public bool isActive { get; set; }       //Còn hiệu lực hay không
        public List<int> times { get; set; } //Thời điểm phát trong ngày (số giây trôi qua từ 0h)
        public List<int> days { get; set; }  //Ngày phát trong tuần (T2 -> CN) nếu isDaily = true
        public List<string> songs { get; set; }  //Danh sách nội dung
        public string path { get; set; }          //Remote folder on MinIO server

        public DisplayScheduleType ScheduleType { get; set; } // Loại bản tin
        public string TextContent { get; set; }
        public string MediaContent { get; set; }
        public bool FullScreen { get; set; }
        public string ColorValue { get; set; }
    }

    public class Schedule_TxMessage
    {
        public int id;          /* Id bản tin - xem danh sách defined */

        public long time;       /* Thời gian bản tin in millisecond UTC */

        public string sender;      /* Imei/ID của đối tượng gửi tin */

        public ScheduleMessage message;    /* Bản tin chi tiết */
    }

    public class ScheduleMessage
    {
        public bool isLeaderOnly;
        public Schedule schedule;
    }
    public enum DisplayScheduleType
    {
        BanTinThongBao = 1,

        BanTinVanBan = 2,

        BanTinVideo = 3,

        BanTinHinhAnh = 4,
    }
}
