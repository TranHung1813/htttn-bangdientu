using Microsoft.Win32;
using Newtonsoft.Json;
using Rijndael256;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Display
{
    public partial class frmMain : Form
    {
        private Message mqttMessage;
        string password = "bytech@2020";  // Khóa để mã hóa
        string GUID_Value = "";

        public const int MSG_ID_SET_CFG = 1;

        private const int DEFAULT_LENGTH_GUID = 15;
        private const int MQTT_PUBLISH_INTERVAL = 60000;
        private const int MQTT_SUBCRIBE_INTERVAL = 1000;

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

        System.Timers.Timer Timer_SendPing;
        Timer Timer_Publish;
        Timer Timer_Subcribe;

        private static int MAX_BUFFER_SIZE = 1024;
        ComPort Uart2Com = new ComPort(115200, MAX_BUFFER_SIZE, false);

        private CustomForm customForm = new CustomForm();
        private DefaultFormShow defaultForm = new DefaultFormShow();
        private LiveStreamForm streamForm = new LiveStreamForm();

        private const int DEFAULT_FORM = 1;
        private const int CUSTOM_FORM = 2;
        private const int STREAM_FORM = 3;
        private int CurrentForm = 0;

        private string NodeId = "";
        private string NodeName = "";

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
        //25. Lưu lại Time Schedule để sau khi app crash vẫn chạy bình thường (done)
        //26. Tính lại Time List theo ngày = số giây từ 0h00 T2 đến thời điểm phát(T2, T5, T7,...) (done)
        //27. Xóa bản tin theo ID (done)
        //28. Text run không mượt trên máy tính Mini

        //29. Kiểm tra: Không có gì để Show => Tắt màn hình (cả Default, Custom Form) (done)
        //30. Xử lý biến Toàn màn hình (done)
        //31. Xóa dữ liệu Show trên màn hình khi: hết Duration (done), hết ValidTime (done)
        //32. Tự động restart máy lúc 2-3h sáng

        //33. Xử lý Priority giữa các Groups (done)
        //34. Xử lý nốt Page Image cho đồng bộ (done)
        //35. Test Page Video (done)
        //36. Xử lý Các bản tin Load từ Database lên (First Time bằng true hay false?)
        //37. Xử lý bản tin bắn định kỳ (Ping Message) (done)
        //38. Xử lý lệnh Stream trực tiếp (StreamCommand) (done)
        //39. Xóa video đã Download sau khi hết Valid Time  (done)
        //40. Download Video, Image ngay khi đẩy bản tin xuống (Pending do chưa play thì chưa biết Video hay Stream để down)
        public frmMain()
        {
            InitializeComponent();
            InitParameters();

            //SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            //Init key press event
            this.KeyPreview = true;
            this.KeyUp += FrmMain_KeyUp;
            this.Shown += FrmMain_Shown;
            //this.TransparencyKey == Color.m;

            Uart2Com.NotifySendPacket += Notify_SendPacket;
            Uart2Com.NotifyRecvPacket += Notify_RecvPacket;
            //Uart2Com.StatusConnection += Notify_StatusConnection;

            scheduleHandle.NotifyTime2Play += ScheduleHandle_NotifyTime2Play;
            scheduleHandle.NotifyTime2Delete += ScheduleHandle_NotifyTime2Delete;

            defaultForm.NotifyEndProcess_TextRun += DefaultForm_NotifyEndProcess;
            defaultForm.NotifyStartProcess += DefaultForm_NotifyStartProcess;

            Uart2Com.Setup_InfoComport(_Baudrate, _Databit, _StopBit, _parity);
            //Uart2Com.FindComPort(PingPacket, PingPacket.Length, PongPacket, PongPacket.Length, 1000, true);

            Log.Information("App Start!");

            // Load Device Info
            DataUser_DeviceInfo Info = SqLiteDataAccess.Load_Device_Info();

            if (Info != null)
            {
                // Set Folder Name to Save File to ucPage1, ucPage2
                NodeId = Info.NodeId;
                NodeName = Info.NodeName;
            }
            else
            {
                // Handle when database = null
            }
        }

        private void DefaultForm_NotifyStartProcess(object sender, NotifyStartProcess e)
        {
            panelContainer.BackColor = Color.MistyRose;
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
                        //Add_UserControl(defaultForm);
                        defaultForm.Show();
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
                        customForm.ShowVideo("https://live.hungyentv.vn/hytvlive/tv1live.m3u8", "");
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
                        customForm.ShowImage("https://i2.wp.com/beebom.com/wp-content/uploads/2016/01/Reverse-Image-Search-Engines-Apps-And-Its-Uses-2016.jpg", "", 20 * 1000);
                    break;

                case Keys.F6:
                    //Show Text Overlay
                    string tex = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
                    if (CurrentForm == CUSTOM_FORM)

                        customForm.Show_TextOverlay(tex, "#5d0b83");
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
                        //Add_UserControl(defaultForm);
                        defaultForm.Show();
                        CurrentForm = DEFAULT_FORM;
                    }
                    defaultForm.Set_Infomation(DisplayScheduleType.BanTinThongBao, "", "“NGÀY HỘI ĐẠI ĐOÀN KẾT TOÀN DÂN TỘC”: TĂNG CƯỜNG KHỐI ĐẠI ĐOÀN KẾT TỪ MỖI CỘNG ĐỒNG DÂN CƯ", 0);
                    defaultForm.Set_Infomation(DisplayScheduleType.BanTinVanBan, "", "   Bộ Thông tin và Truyền thông vừa ban hành Công văn số 1273/BTTTT-TTCS về việc hướng dẫn về chức năng, tính năng kỹ thuật của Hệ thống thông tin (HTTT) nguồn trung ương, Hệ thống thông tin nguồn cấp tỉnh và kết nối các hệ thống thông tin - Phiên bản 1.0.\n\n   Theo đó, Bộ Thông tin và Truyền thông đề nghị Ủy ban nhân dân các tỉnh, thành phố trực thuộc Trung ương chỉ đạo, giao Sở Thông tin và Truyền thông chủ trì tham mưu, xây dựng Hệ thống thông tin nguồn cấp tỉnh để quản lý tập trung các đài truyền thanh cấp xã ứng dụng công nghệ thông tin - viễn thông, bảng tin điện tử công cộng và các phương tiện thông tin cơ sở khác trên địa bàn.\n\n    Hướng dẫn cụ thể về chức năng, tính năng kỹ thuật, HTTT nguồn trung ương và HTTT nguồn cấp tỉnh hoạt động gắn kết chặt chẽ và đồng bộ với nhau trong việc sử dụng, chia sẻ dữ liệu và quản lý hoạt động TTCS xuyên suốt từ Trung ương, cấp tỉnh, cấp huyện đến cơ sở. HTTT nguồn trung ương do Bộ Thông tin và Truyền thông quản lý bao gồm thành phần phục vụ công tác quản lý tại Trung ương và thành phần phục vụ kết nối, chia sẻ dữ liệu với HTTT nguồn cấp tỉnh. Mỗi tỉnh, thành phố trực thuộc Trung ương xây dựng một HTTT nguồn cấp tỉnh do Sở Thông tin và Truyền thông quản lý để tổ chức hoạt động thông tin cơ sở ở cả 3 cấp tỉnh, huyện và xã.\n\n    HTTT nguồn trung ương và HTTT nguồn cấp tỉnh kết nối và chia sẻ dữ liệu với nhau thông qua nền tảng tích hợp, chia sẻ dữ liệu quốc gia (NGSP) và nền tảng tích hợp, chia sẻ dữ liệu cấp bộ, cấp tỉnh (LGSP) của tỉnh, thành phố. Trong đó, HTTT nguồn trung ương được kết nối trực tiếp với hệ thống NGSP. Tùy theo nhu cầu của tỉnh, thành phố, HTTT nguồn cấp tỉnh có thể kết nối trực tiếp với hệ thống NGSP hoặc thông qua hệ thống LGSP của tỉnh, thành phố.\n\n    Yêu cầu chung đối với HTTT nguồn trung ương và HTTT nguồn cấp tỉnh là phải đảm bảo tuân thủ các quy định tại Quyết định số 135/QĐ-TTg ngày 20/01/2020 của Thủ tướng Chính phủ phê duyệt Đề án nâng cao hiệu quả hoạt động thông tin cơ sở dựa trên ứng dụng công nghệ thông tin; Thông tư số 39/2020/TT-BTTTT ngày 24/11/2020 của Bộ trưởng Bộ TTTT Quy định về quản lý đài truyền thanh cấp xã ứng dụng CNTT-VT.\n\n    Đối với HTTT nguồn trung ương phải xây dựng được ứng dụng trên thiết bị di động thông minh (điện thoại di động, máy tính bảng…). Thông qua ứng dụng người dân có thể tiếp nhận thông tin về đường lối, chủ trương của Đảng, chính sách, pháp luật của Nhà nước; thông tin chỉ đạo, điều hành của cấp ủy, chính quyền cơ sở; các thông tin khẩn cấp về thiên tai, hỏa hoạn, dịch bệnh… trên địa bàn; kiến thức về khoa học, kỹ thuật…; gửi ý kiến phản ánh, kiến nghị và đóng góp ý kiến về hiệu quả thực thi chính sách, pháp luật ở cơ sở.\n\n    Đối với HTTT nguồn cấp tỉnh được dùng chung cho cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã trên địa bàn tỉnh, thành phố để thực hiện các hoạt động TTCS. Thông qua HTTT nguồn cấp tỉnh, đội ngũ cán bộ làm công tác TTCS thực hiện tổ chức biên soạn bản tin phát thanh trên đài truyền thanh ứng dụng CNTT-VT, bản tin đăng tải trên bảng tin điện tử công cộng và các phương tiện TTCS khác. Ngoài ra HTTT nguồn cấp tỉnh còn có các chức năng quản lý các cụm loa truyền thanh, bảng tin điện tử công cộng và các phương tiện TTCS khác trên địa bàn tỉnh, thành phố; thực hiện tổng hợp, thống kê để đưa ra các báo cáo phục vụ công tác đánh giá hiệu quả hoạt động TTCS trên địa bàn, chia sẻ dữ liệu với HTTT nguồn trung ương. Cụm loa truyền thanh, bảng tin điện tử công cộng và các phương tiện TTCS khác được kết nối với HTTT nguồn cấp tỉnh thông qua Internet/Intranet, sim 3G/4G hoặc wifi.", 0);
                    defaultForm.ShowVideo(@"https://live.hungyentv.vn/hytvlive/tv1live.m3u8", "", 0);
                    //defaultForm.ShowVideo(@"http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4", Duration: 120 * 1000, loopNum: 0);
                    //defaultForm.ShowImage("https://images.unsplash.com/photo-1608229191360-7064b0afa639?crop=entropy&cs=tinysrgb&fit=crop&fm=jpg&h=800&ixid=MnwxfDB8MXxyYW5kb218MHx8fHx8fHx8MTY3NzIyMjk2Ng&ixlib=rb-4.0.3&q=80&utm_campaign=api-credit&utm_medium=referral&utm_source=unsplash_source&w=1900");
                    //defaultForm.Test();
                    break;

                case Keys.F:
                    Copy2ClipBoard(GUID_Value);
                    customForm.Test();
                    break;

                case Keys.P:
                    //ScheduleHandle abc = new ScheduleHandle();
                    //abc.NotifyTime2Play += ScheduleHandle_NotifyTime2Play;

                    //Schedule msg = new Schedule();
                    //msg.id = "001";
                    //msg.from = 1679892014;
                    //msg.to = 1680491799;
                    //msg.isActive = true;
                    //msg.isDaily = true;
                    //msg.days = new List<int> { 4, 5, 3, 1, 6 };
                    //msg.times = new List<int> { 42060, 42180, 42240 };
                    //msg.idleTime = 1;
                    //msg.loops = 0;
                    //msg.duration = 50 * 1000;
                    //msg.songs = new List<string> { "“NGÀY HỘI ĐẠI ĐOÀN KẾT TOÀN DÂN TỘC”: TĂNG CƯỜNG KHỐI ĐẠI ĐOÀN KẾT TỪ MỖI CỘNG ĐỒNG DÂN CƯ", "Triển khai thực hiện nhiệm vụ “Xây dựng hệ thống thông tin nguồn và thu thập, tổng hợp, phân tích, quản lý dữ liệu, đánh giá hiệu quả hoạt động thông tin cơ sở” tại Quyết định số 135/QĐ-TTg ngày 20/01/2020 của Thủ tướng Chính phủ phê duyệt Đề án nâng cao hiệu quả hoạt động thông tin cơ sở dựa trên ứng dụng công nghệ thông tin; Bộ Thông tin và Truyền thông ban hành Hướng dẫn về chức năng, tính năng kỹ thuật của Hệ thống thông tin nguồn trung ương, Hệ thống thông tin nguồn cấp tỉnh và kết nối các hệ thống thông tin - Phiên bản 1.0 (gửi kèm theo văn bản này).", @"https://live.hungyentv.vn/hytvlive/tv1live.m3u8" };
                    //abc.Schedule(msg);

                    //Schedule msg2 = new Schedule();
                    //msg2.id = "002";
                    //msg2.from = 1679892014;
                    //msg2.to = 1680491799;
                    //msg2.isActive = true;
                    //msg2.isDaily = false;
                    //msg2.times = new List<int> { 42120, 35940, 42210 };
                    //msg2.idleTime = 2;
                    //msg2.loops = 5;
                    //msg2.duration = 500 * 1000;
                    //msg2.songs = new List<string> { "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.", "“NGÀY HỘI ĐẠI ĐOÀN KẾT TOÀN DÂN TỘC”: TĂNG CƯỜNG KHỐI ĐẠI ĐOÀN KẾT TỪ MỖI CỘNG ĐỒNG DÂN CƯ", "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4" };
                    //abc.Schedule(msg2);

                    //Schedule msg3 = new Schedule();
                    //msg3.id = "002";
                    //msg3.isActive = false;
                    //abc.Schedule(msg3);
                    //break;
                    //TimerRestart_Callback(null);
                    DefaultFormShow defaultFormShow = new DefaultFormShow();
                    //defaultFormShow.Owner = form_Text;
                    Utility.fitFormToScreen(defaultFormShow, 768, 1366);

                    Point TB_Location = new Point();
                    Size TB_Size = new Size();
                    Point VB_Location = new Point();
                    Size VB_Size = new Size();
                    defaultFormShow.GetInfo_ThongBao(ref TB_Location, ref TB_Size);
                    defaultFormShow.GetInfo_VanBan(ref VB_Location, ref VB_Size);
                    defaultFormShow.ShowInTaskbar = false;

                    //Form background_TB = new Form();
                    //background_TB.FormBorderStyle = FormBorderStyle.None;
                    //background_TB.BackColor = Color.MistyRose;
                    //background_TB.StartPosition = FormStartPosition.Manual;
                    //background_TB.Size = new Size(TB_Size.Width + 2, TB_Size.Height + 2);
                    //background_TB.Location = TB_Location;
                    //background_TB.ShowInTaskbar = false;

                    //Form_ThongBao form_TB = new Form_ThongBao();
                    //form_TB.Owner = background_TB;
                    //form_TB.PageText_FitToContainer(TB_Size.Height, TB_Size.Width);
                    //form_TB.SetLocation_ThongBao(TB_Location);
                    //form_TB.StartPosition = FormStartPosition.Manual;
                    //form_TB.ShowText("“NGÀY HỘI ĐẠI ĐOÀN KẾT TOÀN DÂN TỘC”: TĂNG CƯỜNG KHỐI ĐẠI ĐOÀN KẾT TỪ MỖI CỘNG ĐỒNG DÂN CƯ" + "\n “NGÀY HỘI ĐẠI ĐOÀN KẾT TOÀN DÂN TỘC”: TĂNG CƯỜNG KHỐI ĐẠI ĐOÀN KẾT TỪ MỖI CỘNG ĐỒNG DÂN CƯ", "");

                    Form background_VB = new Form();
                    background_VB.FormBorderStyle = FormBorderStyle.None;
                    background_VB.BackColor = Color.MistyRose;
                    background_VB.StartPosition = FormStartPosition.Manual;
                    background_VB.Size = new Size(VB_Size.Width + 2, VB_Size.Height + 2);
                    background_VB.Location = VB_Location;
                    background_VB.ShowInTaskbar = false;

                    Form_VanBan form_VB = new Form_VanBan();
                    form_VB.Owner = background_VB;
                    form_VB.PageText_FitToContainer(VB_Size.Height, VB_Size.Width);
                    form_VB.SetLocation_VanBan(VB_Location);
                    form_VB.StartPosition = FormStartPosition.Manual;
                    form_VB.SetSpeed = 1;
                    form_VB.ShowText("   Bộ Thông tin và Truyền thông vừa ban hành Công văn số 1273/BTTTT-TTCS về việc hướng dẫn về chức năng, tính năng kỹ thuật của Hệ thống thông tin (HTTT) nguồn trung ương, Hệ thống thông tin nguồn cấp tỉnh và kết nối các hệ thống thông tin - Phiên bản 1.0.\n\n   Theo đó, Bộ Thông tin và Truyền thông đề nghị Ủy ban nhân dân các tỉnh, thành phố trực thuộc Trung ương chỉ đạo, giao Sở Thông tin và Truyền thông chủ trì tham mưu, xây dựng Hệ thống thông tin nguồn cấp tỉnh để quản lý tập trung các đài truyền thanh cấp xã ứng dụng công nghệ thông tin - viễn thông, bảng tin điện tử công cộng và các phương tiện thông tin cơ sở khác trên địa bàn.\n\n    Hướng dẫn cụ thể về chức năng, tính năng kỹ thuật, HTTT nguồn trung ương và HTTT nguồn cấp tỉnh hoạt động gắn kết chặt chẽ và đồng bộ với nhau trong việc sử dụng, chia sẻ dữ liệu và quản lý hoạt động TTCS xuyên suốt từ Trung ương, cấp tỉnh, cấp huyện đến cơ sở. HTTT nguồn trung ương do Bộ Thông tin và Truyền thông quản lý bao gồm thành phần phục vụ công tác quản lý tại Trung ương và thành phần phục vụ kết nối, chia sẻ dữ liệu với HTTT nguồn cấp tỉnh. Mỗi tỉnh, thành phố trực thuộc Trung ương xây dựng một HTTT nguồn cấp tỉnh do Sở Thông tin và Truyền thông quản lý để tổ chức hoạt động thông tin cơ sở ở cả 3 cấp tỉnh, huyện và xã.\n\n    HTTT nguồn trung ương và HTTT nguồn cấp tỉnh kết nối và chia sẻ dữ liệu với nhau thông qua nền tảng tích hợp, chia sẻ dữ liệu quốc gia (NGSP) và nền tảng tích hợp, chia sẻ dữ liệu cấp bộ, cấp tỉnh (LGSP) của tỉnh, thành phố. Trong đó, HTTT nguồn trung ương được kết nối trực tiếp với hệ thống NGSP. Tùy theo nhu cầu của tỉnh, thành phố, HTTT nguồn cấp tỉnh có thể kết nối trực tiếp với hệ thống NGSP hoặc thông qua hệ thống LGSP của tỉnh, thành phố.\n\n    Yêu cầu chung đối với HTTT nguồn trung ương và HTTT nguồn cấp tỉnh là phải đảm bảo tuân thủ các quy định tại Quyết định số 135/QĐ-TTg ngày 20/01/2020 của Thủ tướng Chính phủ phê duyệt Đề án nâng cao hiệu quả hoạt động thông tin cơ sở dựa trên ứng dụng công nghệ thông tin; Thông tư số 39/2020/TT-BTTTT ngày 24/11/2020 của Bộ trưởng Bộ TTTT Quy định về quản lý đài truyền thanh cấp xã ứng dụng CNTT-VT.\n\n    Đối với HTTT nguồn trung ương phải xây dựng được ứng dụng trên thiết bị di động thông minh (điện thoại di động, máy tính bảng…). Thông qua ứng dụng người dân có thể tiếp nhận thông tin về đường lối, chủ trương của Đảng, chính sách, pháp luật của Nhà nước; thông tin chỉ đạo, điều hành của cấp ủy, chính quyền cơ sở; các thông tin khẩn cấp về thiên tai, hỏa hoạn, dịch bệnh… trên địa bàn; kiến thức về khoa học, kỹ thuật…; gửi ý kiến phản ánh, kiến nghị và đóng góp ý kiến về hiệu quả thực thi chính sách, pháp luật ở cơ sở.\n\n    Đối với HTTT nguồn cấp tỉnh được dùng chung cho cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã trên địa bàn tỉnh, thành phố để thực hiện các hoạt động TTCS. Thông qua HTTT nguồn cấp tỉnh, đội ngũ cán bộ làm công tác TTCS thực hiện tổ chức biên soạn bản tin phát thanh trên đài truyền thanh ứng dụng CNTT-VT, bản tin đăng tải trên bảng tin điện tử công cộng và các phương tiện TTCS khác. Ngoài ra HTTT nguồn cấp tỉnh còn có các chức năng quản lý các cụm loa truyền thanh, bảng tin điện tử công cộng và các phương tiện TTCS khác trên địa bàn tỉnh, thành phố; thực hiện tổng hợp, thống kê để đưa ra các báo cáo phục vụ công tác đánh giá hiệu quả hoạt động TTCS trên địa bàn, chia sẻ dữ liệu với HTTT nguồn trung ương. Cụm loa truyền thanh, bảng tin điện tử công cộng và các phương tiện TTCS khác được kết nối với HTTT nguồn cấp tỉnh thông qua Internet/Intranet, sim 3G/4G hoặc wifi.", "");

                    background_VB.Show();
                    //background_TB.Show();
                    defaultFormShow.Show();
                    break;
                case Keys.C:
                    //Test Crash
                    Uri uri = new Uri(null);
                    break;
            }
        }
        private bool PrioritySchedule_Check(int Priority)
        {
            if (CurrentForm == STREAM_FORM) return false;
            else if (CurrentForm == CUSTOM_FORM)
            {
                return customForm.CheckPriority(Priority);
            }
            else if (CurrentForm == DEFAULT_FORM)
            {
                return defaultForm.CheckPriority(Priority);
            }
            return true;
        }
        private void ScheduleHandle_NotifyTime2Play(object sender, NotifyTime2Play e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                NotifyPlay_Handle(sender, e);
            });
        }

        private void NotifyPlay_Handle(object sender, NotifyTime2Play e)
        {
            if (CurrentForm == STREAM_FORM) return;
            if (PrioritySchedule_Check(e.Priority) == false)
            {
                Log.Information("Message reject by Priority: {A}, Id:{B}, Priority:{C}", e.ScheduleType, e.ScheduleId, e.Priority);
                return;
            }
            if (e.FullScreen == false)
            {
                // Chuyển sang Form Default
                if (CurrentForm != DEFAULT_FORM)
                {
                    customForm.Close();
                    //Add_UserControl(defaultForm);
                    panelContainer.Controls.Clear();
                    //defaultForm.Show();
                    panelContainer.BackColor = Color.Black;
                    CurrentForm = DEFAULT_FORM;
                }
                if (e.ScheduleType == DisplayScheduleType.BanTinThongBao || e.ScheduleType == DisplayScheduleType.BanTinVanBan)
                {
                    Log.Information("NotifyTime2Play: {A}, Id : {id}, Content: {B}, Color: {C}, Duration: {D}, FullScreen: {E}", e.ScheduleType, e.ScheduleId, e.Text.Substring(0, e.Text.Length / 5), e.ColorValue, e.Duration, e.FullScreen);
                    defaultForm.Set_Infomation(e.ScheduleType, e.ScheduleId, e.Text, e.Priority, Duration: e.Duration * 1000);

                    if (e.ScheduleType == DisplayScheduleType.BanTinVanBan)
                    {
                        defaultForm.NotifyEndProcess_TextRun += DefaultForm_NotifyEndProcess;
                    }
                }
                else if (e.ScheduleType == DisplayScheduleType.BanTinVideo)
                {
                    Log.Information("NotifyTime2Play: Video: {A}, Id : {id}, Idle: {B}, Loops: {C}, Duration: {D}, FullScreen: {E}", e.MediaUrl, e.ScheduleId, e.IdleTime, e.LoopNum, e.Duration, e.FullScreen);
                    defaultForm.ShowVideo(e.MediaUrl[0], e.ScheduleId, e.Priority, e.StartPosition);
                }
                else if (e.ScheduleType == DisplayScheduleType.BanTinHinhAnh)
                {
                    Log.Information("NotifyTime2Play: Hinh anh: {A}, Id : {id}, Duration: {B}, FullScreen: {C}", e.MediaUrl, e.ScheduleId, e.Duration, e.FullScreen);
                    defaultForm.ShowImage(e.MediaUrl[0], e.ScheduleId, e.Priority, e.Duration * 1000);
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
                    Log.Information("NotifyTime2Play: Video: {A}, Id : {id}, Idle: {B}, Loops: {C}, Duration: {D}, FullScreen: {E}, Text: {F}", e.MediaUrl, e.ScheduleId, e.IdleTime, e.LoopNum, e.Duration, e.FullScreen, e.TextContent.Substring(0, e.TextContent.Length / 5));
                    customForm.ShowVideo(e.MediaUrl[0], e.ScheduleId, e.Priority, e.StartPosition);

                    customForm.Show_TextOverlay(e.TextContent, e.ColorValue, e.Duration * 1000);
                }
                else if (e.ScheduleType == DisplayScheduleType.BanTinHinhAnh)
                {
                    Log.Information("NotifyTime2Play: Hinh anh: {A}, Id : {id}, Duration: {B}, FullScreen: {C}, Text: {D}", e.MediaUrl, e.ScheduleId, e.Duration, e.FullScreen, e.TextContent.Substring(0, e.TextContent.Length / 5));
                    customForm.ShowImage(e.MediaUrl[0], e.ScheduleId, e.Priority, e.Duration * 1000);
                    customForm.Show_TextOverlay(e.TextContent, e.ColorValue, e.Duration * 1000);
                }
                else if (e.ScheduleType == DisplayScheduleType.BanTinVanBan)
                {
                    Log.Information("NotifyTime2Play: {A}, Id : {id}, Content: {B}, Color: {C}, Duration: {D}, FullScreen: {E}", e.ScheduleType, e.ScheduleId, e.Text.Substring(0, e.Text.Length / 5), e.ColorValue, e.Duration, e.FullScreen);
                    customForm.ShowText(e.Title, e.Text, e.ScheduleId, e.Priority, Duration: e.Duration * 1000);
                }
            }
        }

        private void DefaultForm_NotifyEndProcess(object sender, NotifyTextEndProcess e)
        {
            panelContainer.BackColor = Color.Black;
        }

        private void ScheduleHandle_NotifyTime2Delete(object sender, NotifyTime2Delete e)
        {
            if (CurrentForm == STREAM_FORM) return;
            else if (CurrentForm == DEFAULT_FORM)
            {
                defaultForm.Close_by_Id(e.ScheduleId);
            }
            else if (CurrentForm == CUSTOM_FORM)
            {
                customForm.Close_by_Id(e.ScheduleId);
            }

            if (e.DeleteSavedFile == true) DeleteFile_in_Database(e.ScheduleId);
        }
        private void DeleteFile_in_Database(string ScheduleId)
        {
            List<DataUser_SavedFiles> SavedFiles = SqLiteDataAccess.Load_SavedFiles_Info();

            if (SavedFiles != null)
            {
                // Kiem tra xem File da download chua, neu roi thi Xoa
                int index = SavedFiles.FindIndex(s => (s.ScheduleId == ScheduleId));
                if (index != -1)
                {
                    try
                    {
                        if (File.Exists(SavedFiles[index].PathLocation))
                        {
                            File.Delete(SavedFiles[index].PathLocation);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "ScheduleHandle_NotifyTime2Delete");
                    }

                    // Save lai
                    SqLiteDataAccess.DeleteAll_SavedFiles();
                    if (SavedFiles.Count > 1)
                    {
                        SavedFiles.RemoveAt(index);

                        int Id = 1;
                        foreach (var file in SavedFiles)
                        {
                            DataUser_SavedFiles info_Save = new DataUser_SavedFiles();

                            info_Save.Id = Id++;
                            info_Save.ScheduleId = file.ScheduleId;
                            info_Save.PathLocation = file.PathLocation;
                            info_Save.Link = file.Link;

                            SqLiteDataAccess.SaveInfo_SavedFiles(info_Save);
                        }
                    }
                }
                else
                {
                    // Do nothing
                }
            }
            else
            {
                // Do nothing
            }
        }
        private void Close_Relay()
        {
            // Close Relay
            Log.Information("Close_Relay");
            //Uart2Com.SendPacket(Uart2Com.GetChanelFree(), ClosePacket, ClosePacket.Length);
            Uart2Com.Send(ClosePacket, ClosePacket.Length);
            _isRelayOpened = false;
        }
        private void OpenRelay()
        {
            // Open Relay
            Log.Information("Open_Relay");
            //Uart2Com.SendPacket(Uart2Com.GetChanelFree(), OpenPacket, OpenPacket.Length);
            Uart2Com.Send(OpenPacket, OpenPacket.Length);
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
            Load_Groups_Info();
            mqttMessage = new Message(Properties.Settings.Default.MqttAddress, Properties.Settings.Default.MqttPort,
                    Properties.Settings.Default.MqttUserName, Properties.Settings.Default.MqttPassword, GUID_Value);

            Timer_Publish = new Timer();
            Timer_Publish.Interval = MQTT_PUBLISH_INTERVAL;
            Timer_Publish.Tick += Timer_Publish_Elapsed;
            Timer_Publish.Start();

            Timer_Subcribe = new Timer();
            Timer_Subcribe.Interval = MQTT_SUBCRIBE_INTERVAL;
            Timer_Subcribe.Tick += Timer_Subcribe_Elapsed;
            Timer_Subcribe.Start();
        }

        private void Load_Groups_Info()
        {
            //Get Groups info
            List<Group> GroupsList = new List<Group>();

            List<DataUser_Groups_Info> GroupsInfo = SqLiteDataAccess.Load_Groups_Info();
            if (GroupsInfo != null)
            {
                foreach (var groupInfo in GroupsInfo)
                {
                    Group gr = new Group();
                    gr.Id = groupInfo.GroupId;
                    gr.MasterType = groupInfo.MasterType;
                    gr.Name = groupInfo.Name;
                    gr.Priority = groupInfo.Priority;
                    GroupsList.Add(gr);
                }
            }
            else
            {
                // Null Handle
            }
            Message.Load_Groups_Info(GroupsList);
        }

        private void LogInit()
        {
            string LocalPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                        .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.File(LocalPath + @"\Log\Info-.txt", rollingInterval: RollingInterval.Day))
                        .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.File(LocalPath + @"\Log\Debug-.txt", rollingInterval: RollingInterval.Day))
                        .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.File(LocalPath + @"\Log\Warning-.txt", rollingInterval: RollingInterval.Day))
                        .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File(LocalPath + @"\Log\Error-.txt", rollingInterval: RollingInterval.Day))
                    .CreateLogger();
        }

        private void Timer_Subcribe_Elapsed(object sender, EventArgs e)
        {
            ProcessNewMessage();
        }
        private void Timer_Publish_Elapsed(object sender, EventArgs e)
        {
            SendHeartBeatTick();
        }

        private void SendHeartBeatTick()
        {
            PingMessage pingMessage = new PingMessage();
            pingMessage.Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            pingMessage.Type = 2;
            pingMessage.Ip = GetLocalIPAddress();
            pingMessage.Sn = GUID_Value;
            pingMessage.Id = NodeId;
            pingMessage.Name = NodeName;
            pingMessage.Stmt = "";
            pingMessage.Stlk = "";
            pingMessage.Stst = 0;
            pingMessage.Scid = "";
            pingMessage.Scf = "";
            pingMessage.Scst = 0;
            pingMessage.Mic = false;
            pingMessage.Spk = true;
            pingMessage.Cam = false;
            pingMessage.Vl1 = 0;
            pingMessage.Vl2 = 0;

            if (CurrentForm == DEFAULT_FORM)
            {
                defaultForm.GetScheduleInfo(ref pingMessage.Scid, ref pingMessage.Scf,
                                            ref pingMessage.Scst, ref pingMessage.Spk,
                                            ref pingMessage.Vl2);
            }
            else if (CurrentForm == CUSTOM_FORM)
            {
                customForm.GetScheduleInfo(ref pingMessage.Scid, ref pingMessage.Scf,
                                            ref pingMessage.Scst, ref pingMessage.Spk,
                                            ref pingMessage.Vl2);
            }
            else if (CurrentForm == STREAM_FORM)
            {
                streamForm.GetStreamInfo(ref pingMessage.Stmt, ref pingMessage.Stlk,
                                         ref pingMessage.Stst, ref pingMessage.Spk, ref pingMessage.Vl2);
            }

            Ping_TxMessage txMsg = new Ping_TxMessage();
            txMsg.Id = 10;
            txMsg.Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            txMsg.Sender = GUID_Value;
            txMsg.Message = pingMessage;

            var json = new JavaScriptSerializer().Serialize(txMsg);
            mqttMessage.SendMessage(json);
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void ProcessNewMessage()
        {
            try
            {
                var newMessage = mqttMessage.GetMessage;
                if (newMessage != null)
                {
                    var topic = newMessage.Topic;
                    if (newMessage.Topic == mqttMessage.subcribeTopic_Default)
                    {
                        string message = Encoding.UTF8.GetString(newMessage.Payload);
                        //Log.Information("ProcessNewMessage: {A}", message);
                        dynamic payload = JsonConvert.DeserializeObject<object>(message);

                        if (payload.BanTinThongBao != null)
                        {
                            Log.Information("Get_NewMessage: DefaultTopic");
                            _TxtThongBao = payload.BanTinThongBao;
                            //ShowText(_TxtThongBao);
                            _TxtVanBan = payload.BanTinVanBan;
                            _VideoUrl = payload.VideoUrl;

                            //customForm.ShowVideo(_VideoUrl);
                            if (CurrentForm == STREAM_FORM) return;
                            // Chuyển sang Form Default
                            if (CurrentForm != DEFAULT_FORM)
                            {
                                customForm.Close();
                                //Add_UserControl(defaultForm);
                                defaultForm.Show();
                                CurrentForm = DEFAULT_FORM;
                            }
                            defaultForm.Set_Infomation(DisplayScheduleType.BanTinThongBao, "", _TxtThongBao);
                            defaultForm.Set_Infomation(DisplayScheduleType.BanTinVanBan, "", _TxtVanBan);
                            defaultForm.ShowVideo(_VideoUrl, "");
                            //customForm.ShowVideo("https://live.hungyentv.vn/hytvlive/tv1live.m3u8");
                        }
                    }
                    else if (newMessage.Topic == mqttMessage.subcribeTopic_Groups)
                    {
                        string message = Encoding.UTF8.GetString(newMessage.Payload);
                        //Log.Information("ProcessNewMessage: {A}", message);
                        dynamic payload = JsonConvert.DeserializeObject<object>(message);

                        if (payload.Message.Groups != null)
                        {
                            Log.Information("Get_NewMessage: DeviceConfigMessage");
                            string s = JsonConvert.SerializeObject(payload.Message);
                            DeviceConfigMessage newGroups_msg = JsonConvert.DeserializeObject<DeviceConfigMessage>(s);
                            mqttMessage.Subcribe2Groups(newGroups_msg.Groups);

                            DataUser_Groups_Info info_Save = new DataUser_Groups_Info();
                            info_Save.Id = 0;
                            foreach (var group in newGroups_msg.Groups)
                            {
                                info_Save.Id += 1;
                                info_Save.GroupId = group.Id;
                                info_Save.Priority = group.Priority;
                                info_Save.Name = group.Name;
                                info_Save.MasterType = group.MasterType;

                                SqLiteDataAccess.SaveInfo_Groups(info_Save);
                            }
                            Load_Groups_Info();

                            NodeId = payload.Message.Id;
                            NodeName = payload.Message.Name;

                            DataUser_DeviceInfo info = new DataUser_DeviceInfo();
                            info.Id = 1;
                            info.NodeId = payload.Message.Id;
                            info.NodeName = payload.Message.Name;
                            SqLiteDataAccess.SaveInfo_Device(info);
                        }
                    }
                    else
                    {
                        int Priority = -1;
                        int index = Message.GroupsList.FindIndex(s => newMessage.Topic.Contains(s.Id) == true);
                        if (index != -1)
                        {
                            Priority = Message.GroupsList[index].Priority;
                        }
                        string message = Encoding.UTF8.GetString(newMessage.Payload);
                        //Log.Information("ProcessNewMessage: {A}", message);
                        dynamic payload = JsonConvert.DeserializeObject<object>(message);

                        // Schedule Message
                        if (payload.Message.Schedule != null)
                        {
                            string s = JsonConvert.SerializeObject(payload.Message.Schedule);
                            Schedule newSchedule_msg = JsonConvert.DeserializeObject<Schedule>(s);
                            Log.Information("Get_NewMessage: ScheduleMessage, Id: {A}, isActive: {B}, Type: {C}",
                                                                                        newSchedule_msg.Id,
                                                                                        newSchedule_msg.IsActive.ToString(),
                                                                                        newSchedule_msg.ScheduleType);
                            System.Threading.Tasks.Task.Run(() =>
                            {
                                scheduleHandle.Schedule(newSchedule_msg, Priority);
                            });
                        }
                        // Stream Command
                        else if (payload.Message.StreamInfo != null)
                        {
                            string s = JsonConvert.SerializeObject(payload.Message);
                            StreamCommandMessage StreamCmd = JsonConvert.DeserializeObject<StreamCommandMessage>(s);
                            Log.Information("Get_NewMessage: StreamCommandMessage, Command: {A}, Url: {B}",
                                                                                        StreamCmd.CmdCode,
                                                                                        StreamCmd.StreamInfo.Uri);

                            if (StreamCmd.IsAll == true || StreamCmd.Serial.Contains(GUID_Value))
                            {
                                if (StreamCmd.CmdCode == commandCode.CMD_STREAM_PREPAIR)
                                {
                                    if (_isRelayOpened == false)
                                    {
                                        // Bat man hinh
                                        OpenRelay();
                                    }
                                    CloseCurrentForm();
                                    Add_UserControl(streamForm);
                                    CurrentForm = STREAM_FORM;
                                }
                                else if (StreamCmd.CmdCode == commandCode.CMD_STREAM_START)
                                {
                                    CloseCurrentForm();
                                    Add_UserControl(streamForm);
                                    CurrentForm = STREAM_FORM;

                                    streamForm.ShowLiveStream(StreamCmd.StreamInfo.Uri, StreamCmd.Volume, StreamCmd.MasterImei);
                                }
                                else if (StreamCmd.CmdCode == commandCode.CMD_STREAM_STOP)
                                {
                                    if (CurrentForm == STREAM_FORM)
                                    {
                                        streamForm.Close();
                                        panelContainer.Controls.Clear();

                                        CurrentForm = 0;
                                    }
                                }
                            }
                        }
                        else if(payload.Id != null && payload.Id == MSG_ID_SET_CFG)
                        {
                            // MSG_SET_CFG
                            string s = JsonConvert.SerializeObject(payload.Message);
                            SetConfigMessage SetCfg_Msg = JsonConvert.DeserializeObject<SetConfigMessage>(s);
                            Log.Information("Get_NewMessage: SetConfigMessage");
                            int VolumeValue = -1;
                            string VolumeCmd = "";
                            if (SetCfg_Msg.Cmd != null)
                            {
                                List<string> List_Cmd = SetCfg_Msg.Cmd.Split(' ').OfType<string>().ToList();
                                int index_VolumeCmd = List_Cmd.FindIndex(i => (i.Split(',').OfType<string>().ToList().Contains("96") ||
                                                                               i.Split(',').OfType<string>().ToList().Contains("97")));
                                if(index_VolumeCmd != -1)
                                {
                                    VolumeCmd = List_Cmd[index_VolumeCmd].Split(',')[2].Replace("(", "").Replace(")", "");
                                }
                            }
                            else if (SetCfg_Msg.Obj != null)
                            {
                                int index_VolumeCmd = SetCfg_Msg.Obj.FindIndex(i => (i.C == 96 || i.C == 97));
                                if (index_VolumeCmd != -1)
                                {
                                    VolumeCmd = SetCfg_Msg.Obj[index_VolumeCmd].V.ToString();
                                }
                            }
                            if (Int32.TryParse(VolumeCmd, out VolumeValue))
                            {
                                if (VolumeValue < 0 && VolumeValue > 100) return;
                                if (CurrentForm == DEFAULT_FORM)
                                {
                                    defaultForm.SetVolume(VolumeValue);
                                }
                                else if (CurrentForm == CUSTOM_FORM)
                                {
                                    customForm.SetVolume(VolumeValue);
                                }
                                else if (CurrentForm == STREAM_FORM)
                                {
                                    streamForm.SetVolume(VolumeValue);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ProcessNewMessage");
            }
        }

        private void CloseCurrentForm()
        {
            if (CurrentForm == DEFAULT_FORM)
            {
                defaultForm.Close();
            }
            else if (CurrentForm == CUSTOM_FORM)
            {
                customForm.Close();
            }
            else if (CurrentForm == STREAM_FORM)
            {
                streamForm.Close();
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
            streamForm.LiveStreamForm_FitToContainer(panelContainer.Height, panelContainer.Width);

            ComputerRestart_Handle(2.0);
            ComputerRestart_Handle(3.0);
            ComputerRestart_Handle(4.0);
        }
        private void FrmMain_Shown(object sender, EventArgs e)
        {
            scheduleHandle.Load_ScheduleMessageInfo();
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
                if (Timer_SendPing != null)
                {
                    Timer_SendPing.Stop();
                    Timer_SendPing.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Timer_SendPing.Stop");
            }
            Timer_SendPing = new System.Timers.Timer();
            Timer_SendPing.Interval = Interval;
            Timer_SendPing.Elapsed += Timer_SendPing_Elapsed;
            Timer_SendPing.Start();
        }

        private void Timer_SendPing_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Uart2Com.SendPacket(Uart2Com.GetChanelFree(), PingPacket, PingPacket.Length);
            Uart2Com.Send(PingPacket, PingPacket.Length);
        }

        //*****************************************************************************************************************
        //************************************************** Restart Computer **************************************************//

        private void ComputerRestart_Handle(double Hour)
        {
            if (Hour < 0 || Hour > 24) return;
            try
            {
                var t = new System.Threading.Timer(TimerRestart_Callback);

                // Figure how much time until 2:00 AM
                DateTime now = DateTime.Now;
                DateTime fourOClock = DateTime.Today.AddHours(Hour);

                // If it's already past 2:00, wait until 2:00 tomorrow    
                if (now > fourOClock)
                {
                    fourOClock = fourOClock.AddDays(1.0);
                }

                int msUntilFour = (int)((fourOClock - now).TotalMilliseconds);

                // Set the timer to elapse only once, at 2:00.
                t.Change(msUntilFour, System.Threading.Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ComputerRestart_Handle");
            }
        }
        private void TimerRestart_Callback(object state)
        {
            if (CheckMessage_Available() == true)
            {
                try
                {
                    System.Threading.Timer t = (System.Threading.Timer)state;
                    t.Dispose();
                }
                catch (Exception ex) { Log.Error(ex, "TimerRestart_Callback"); }
                return;
            }
            else
            {
                // Computer Restart
                Log.Information("Computer Restart !!!");
                System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            }
        }

        private bool CheckMessage_Available()
        {
            if (CurrentForm == DEFAULT_FORM)
            {
                return defaultForm.CheckMessage_Available();
            }
            else if (CurrentForm == CUSTOM_FORM)
            {
                return customForm.CheckMessage_Available();
            }
            else if (CurrentForm == STREAM_FORM)
            {
                return true;
            }
            return false;
        }
    }

    public class DisplayMessage
    {
        public string BanTinThongBao { get; set; }
        public string BanTinVanBan { get; set; }
        public string VideoUrl { get; set; }
    }
}
