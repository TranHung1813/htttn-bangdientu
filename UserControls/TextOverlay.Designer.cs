
namespace Display
{
    partial class TextOverlay
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextOverlay));
            this.panel1 = new System.Windows.Forms.Panel();
            this.timer_DelayTextRun = new System.Windows.Forms.Timer(this.components);
            this.txtOverlay = new Display.TextEx2();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.LavenderBlush;
            this.panel1.Controls.Add(this.txtOverlay);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(818, 53);
            this.panel1.TabIndex = 0;
            // 
            // timer_DelayTextRun
            // 
            this.timer_DelayTextRun.Interval = 3000;
            this.timer_DelayTextRun.Tick += new System.EventHandler(this.timer_DelayTextRun_Tick);
            // 
            // txtOverlay
            // 
            this.txtOverlay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOverlay.AutoEllipsis = true;
            this.txtOverlay.AutoSize = true;
            this.txtOverlay.BackColor = System.Drawing.Color.Transparent;
            this.txtOverlay.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOverlay.ForeColor = System.Drawing.Color.White;
            this.txtOverlay.Location = new System.Drawing.Point(3, 3);
            this.txtOverlay.Name = "txtOverlay";
            this.txtOverlay.OutlineForeColor = System.Drawing.Color.Lime;
            this.txtOverlay.OutlineWidth = 3F;
            this.txtOverlay.SetSpeed = 0;
            this.txtOverlay.Size = new System.Drawing.Size(4851, 54);
            this.txtOverlay.TabIndex = 3;
            this.txtOverlay.Text = resources.GetString("txtOverlay.Text");
            this.txtOverlay.UseCompatibleTextRendering = true;
            // 
            // TextOverlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LavenderBlush;
            this.Controls.Add(this.panel1);
            this.Name = "TextOverlay";
            this.Size = new System.Drawing.Size(818, 53);
            this.Load += new System.EventHandler(this.TextOverlay_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private TextEx2 txtOverlay;
        private System.Windows.Forms.Timer timer_DelayTextRun;
    }
}
