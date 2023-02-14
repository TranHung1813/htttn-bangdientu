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
    public partial class Page_Image : UserControl
    {
        private string _ImageURL = "";
        public Page_Image()
        {
            InitializeComponent();
            _ImageURL = "http://www.gravatar.com/avatar/6810d91caff032b202c50701dd3af745?d=identicon&r=PG";
        }

        public void ShowImage(string Url)
        {
            pictureBox1.Load(_ImageURL);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}
