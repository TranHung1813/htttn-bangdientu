
namespace Display
{
    partial class CustomForm
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Guna.UI2.AnimatorNS.Animation animation5 = new Guna.UI2.AnimatorNS.Animation();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomForm));
            this.panel_TextOverlay = new System.Windows.Forms.Panel();
            this.guna2Transition1 = new Guna.UI2.WinForms.Guna2Transition();
            this.tick = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // panel_TextOverlay
            // 
            this.panel_TextOverlay.BackColor = System.Drawing.Color.Gray;
            this.guna2Transition1.SetDecoration(this.panel_TextOverlay, Guna.UI2.AnimatorNS.DecorationType.None);
            this.panel_TextOverlay.Location = new System.Drawing.Point(0, 433);
            this.panel_TextOverlay.Name = "panel_TextOverlay";
            this.panel_TextOverlay.Size = new System.Drawing.Size(915, 42);
            this.panel_TextOverlay.TabIndex = 1;
            this.panel_TextOverlay.Visible = false;
            // 
            // guna2Transition1
            // 
            this.guna2Transition1.AnimationType = Guna.UI2.AnimatorNS.AnimationType.Transparent;
            this.guna2Transition1.Cursor = null;
            animation5.AnimateOnlyDifferences = true;
            animation5.BlindCoeff = ((System.Drawing.PointF)(resources.GetObject("animation5.BlindCoeff")));
            animation5.LeafCoeff = 0F;
            animation5.MaxTime = 1F;
            animation5.MinTime = 0F;
            animation5.MosaicCoeff = ((System.Drawing.PointF)(resources.GetObject("animation5.MosaicCoeff")));
            animation5.MosaicShift = ((System.Drawing.PointF)(resources.GetObject("animation5.MosaicShift")));
            animation5.MosaicSize = 0;
            animation5.Padding = new System.Windows.Forms.Padding(0);
            animation5.RotateCoeff = 0F;
            animation5.RotateLimit = 0F;
            animation5.ScaleCoeff = ((System.Drawing.PointF)(resources.GetObject("animation5.ScaleCoeff")));
            animation5.SlideCoeff = ((System.Drawing.PointF)(resources.GetObject("animation5.SlideCoeff")));
            animation5.TimeCoeff = 0F;
            animation5.TransparencyCoeff = 1F;
            this.guna2Transition1.DefaultAnimation = animation5;
            // 
            // tick
            // 
            this.tick.Enabled = true;
            this.tick.Interval = 1000;
            // 
            // CustomForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.panel_TextOverlay);
            this.guna2Transition1.SetDecoration(this, Guna.UI2.AnimatorNS.DecorationType.None);
            this.Name = "CustomForm";
            this.Size = new System.Drawing.Size(915, 475);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_TextOverlay;
        private Guna.UI2.WinForms.Guna2Transition guna2Transition1;
        private System.Windows.Forms.Timer tick;
    }
}
