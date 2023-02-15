using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class PanelContainer : Panel
    {
        public PanelContainer(Control parent)
        {
            if (parent != null)
            {
                this.Parent = parent;
            }
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Black;
            this.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Size = parent.Size;
            this.Visible = false;
        }

        [System.ComponentModel.DefaultValue(typeof(DockStyle), "Fill")]
        public override DockStyle Dock
        {
            get { return base.Dock; }
            set { base.Dock = value; }
        }
    }
}
