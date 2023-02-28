
namespace Display
{
    partial class Page_Text
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
            this.timerDelayTextRun = new System.Windows.Forms.Timer(this.components);
            this.panelContainer = new System.Windows.Forms.Panel();
            this.panel_TextRun = new Display.PanelEx();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lb_Content = new Display.GrowLabel();
            this.lb_Title = new Display.GrowLabel();
            this.panelContainer.SuspendLayout();
            this.panel_TextRun.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // timerDelayTextRun
            // 
            this.timerDelayTextRun.Interval = 10000;
            this.timerDelayTextRun.Tick += new System.EventHandler(this.timerDelayTextRun_Tick);
            // 
            // panelContainer
            // 
            this.panelContainer.BackColor = System.Drawing.Color.MistyRose;
            this.panelContainer.Controls.Add(this.panel_TextRun);
            this.panelContainer.Location = new System.Drawing.Point(10, 10);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Size = new System.Drawing.Size(900, 400);
            this.panelContainer.TabIndex = 1;
            // 
            // panel_TextRun
            // 
            this.panel_TextRun.Controls.Add(this.pictureBox1);
            this.panel_TextRun.Controls.Add(this.lb_Content);
            this.panel_TextRun.Controls.Add(this.lb_Title);
            this.panel_TextRun.Location = new System.Drawing.Point(0, 0);
            this.panel_TextRun.Name = "panel_TextRun";
            this.panel_TextRun.SetSpeed = 0;
            this.panel_TextRun.Size = new System.Drawing.Size(900, 400);
            this.panel_TextRun.State = 0;
            this.panel_TextRun.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 50);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // lb_Content
            // 
            this.lb_Content.BackColor = System.Drawing.Color.Transparent;
            this.lb_Content.Dock = System.Windows.Forms.DockStyle.Top;
            this.lb_Content.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.lb_Content.ForeColor = System.Drawing.Color.Navy;
            this.lb_Content.Location = new System.Drawing.Point(0, 93);
            this.lb_Content.Name = "lb_Content";
            this.lb_Content.Size = new System.Drawing.Size(900, 78);
            this.lb_Content.TabIndex = 2;
            this.lb_Content.Text = "    UBND thị trấn Bến Lức thông báo đến nhân dân \"Về việc đeo khẩu trang, hạn chế" +
    " tập trung đông người trên địa bàn thị trấn Bến Lức\" Để chủ động kiểm soát, ngăn" +
    " chặn dịch bệnh Covid-19 gây ra.";
            // 
            // lb_Title
            // 
            this.lb_Title.BackColor = System.Drawing.Color.Transparent;
            this.lb_Title.Dock = System.Windows.Forms.DockStyle.Top;
            this.lb_Title.Font = new System.Drawing.Font("Times New Roman", 20F, System.Drawing.FontStyle.Bold);
            this.lb_Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lb_Title.Location = new System.Drawing.Point(0, 0);
            this.lb_Title.Name = "lb_Title";
            this.lb_Title.Size = new System.Drawing.Size(900, 93);
            this.lb_Title.TabIndex = 1;
            this.lb_Title.Text = "UBND thị trấn Bến Lức thông báo đến nhân dân \"Về việc đeo khẩu trang, hạn chế tập" +
    " trung đông người trên địa bàn thị trấn Bến Lức\" Để chủ động kiểm soát, ngăn chặ" +
    "n dịch bệnh Covid-19 gây ra.";
            this.lb_Title.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Page_Text
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.Controls.Add(this.panelContainer);
            this.Name = "Page_Text";
            this.Size = new System.Drawing.Size(920, 420);
            this.panelContainer.ResumeLayout(false);
            this.panel_TextRun.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timerDelayTextRun;
        private System.Windows.Forms.Panel panelContainer;
        private GrowLabel lb_Title;
        private GrowLabel lb_Content;
        private PanelEx panel_TextRun;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
