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
    public partial class Transparent_BackGround : Form
    {
        public Transparent_BackGround()
        {
            InitializeComponent();
        }
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
    }
}
