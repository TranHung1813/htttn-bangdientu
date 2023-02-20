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
    public partial class Page_Text : UserControl
    {
        public Page_Text()
        {
            InitializeComponent();
            txtThongBao.Size = panelContainer.Size;
        }

        public void ShowText(string txt)
        {
            //txtThongBao.Text = txt;
            timerDelayTextRun.Interval = 5000;
            timerDelayTextRun.Start();
        }

        private void timerDelayTextRun_Tick(object sender, EventArgs e)
        {
            txtThongBao.SetSpeed = 1;
            txtThongBao.Start();

            timerDelayTextRun.Stop();
        }
    }
}
