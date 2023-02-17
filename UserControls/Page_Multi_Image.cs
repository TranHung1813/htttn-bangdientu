using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class Page_Multi_Image : UserControl
    {
        public Page_Multi_Image()
        {
            InitializeComponent();
        }

        public void Show_Multi_Image(string[] ImageURLs, int Number_Image)
        {
            if (Number_Image <= 0) return;

            for (int CountImage = 0; CountImage < Number_Image; CountImage ++)
            {
                try
                {
                    pictureBox1.Load(ImageURLs[CountImage]);
                }
                catch
                {

                }

                Thread.Sleep(2000);
            }
        }
    }
}
