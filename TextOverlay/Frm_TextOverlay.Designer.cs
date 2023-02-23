
namespace Display
{
    partial class Frm_TextOverlay
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer_DelayText = new System.Windows.Forms.Timer(this.components);
            this.panel_TxtOverlay = new Display.PanelEx2();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel_TxtOverlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // timer_DelayText
            // 
            this.timer_DelayText.Interval = 1000;
            this.timer_DelayText.Tick += new System.EventHandler(this.timer_DelayText_Tick);
            // 
            // panel_TxtOverlay
            // 
            this.panel_TxtOverlay.BackColor = System.Drawing.Color.Transparent;
            this.panel_TxtOverlay.Controls.Add(this.pictureBox1);
            this.panel_TxtOverlay.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel_TxtOverlay.Location = new System.Drawing.Point(0, 0);
            this.panel_TxtOverlay.Margin = new System.Windows.Forms.Padding(0);
            this.panel_TxtOverlay.Max_Repeat_Time = 0;
            this.panel_TxtOverlay.Name = "panel_TxtOverlay";
            this.panel_TxtOverlay.SetSpeed = 0;
            this.panel_TxtOverlay.Size = new System.Drawing.Size(1023, 72);
            this.panel_TxtOverlay.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(5225, 55);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // Frm_TextOverlay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(1023, 78);
            this.Controls.Add(this.panel_TxtOverlay);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(18);
            this.Name = "Frm_TextOverlay";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Frm_TextOverlay";
            this.panel_TxtOverlay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PanelEx2 panel_TxtOverlay;
        private System.Windows.Forms.Timer timer_DelayText;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}