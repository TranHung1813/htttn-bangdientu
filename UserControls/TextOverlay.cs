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
    public partial class TextOverlay : UserControl
    {
        private string _Text = "";
        public TextOverlay()
        {
            InitializeComponent();
            txtOverlay.Text = "";
        }

        public void ShowTextOverlay(string txt)
        {
            //_Text = txt;
            _Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
            //_Text = "Hello world!!";
            timer_DelayTextRun.Interval = 2000;
            timer_DelayTextRun.Start();
        }

        private void timer_DelayTextRun_Tick(object sender, EventArgs e)
        {
            txtOverlay.Text = _Text;
            txtOverlay.SetSpeed = 3;
            txtOverlay.Start(panel1.Width);

            timer_DelayTextRun.Stop();
        }
    }
}
