using System.Drawing;
using System.Windows.Forms;

namespace Display
{
    public partial class Frm_TextOverlay : Form
    {
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;

                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);

                return baseParams;
            }
        }

        private string _Text = "";
        public Frm_TextOverlay()
        {
            InitializeComponent();

            BackColor = Color.Gray;
            TransparencyKey = Color.Gray;
        }

        public void ShowTextOverlay(string Txt)
        {
            _Text = Txt;
            _Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
            //_Text = "Hello world!!";
            //timer_DelayTextRun.Interval = 1000;
            //timer_DelayTextRun.Start();

            txtOverlay.SetSpeed = 2;
            txtOverlay.Start(panel1.Width);
        }
    }
}
