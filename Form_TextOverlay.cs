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
    public partial class Form_TextOverlay : Form
    {
        public Form_TextOverlay()
        {
            InitializeComponent();

            this.BackColor = Color.LimeGreen;
            this.TransparencyKey = Color.LimeGreen;

            textEx21.SetSpeed = 1;
            textEx21.Start();
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
    }
}
