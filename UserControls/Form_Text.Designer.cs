
namespace Display
{
    partial class Form_Text
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
            this.panel_TextRun = new Display.PanelEx();
            this.lb_Content = new Display.GrowLabel();
            this.lb_Title = new Display.GrowLabel();
            this.panel_TextRun.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_TextRun
            // 
            this.panel_TextRun.Controls.Add(this.lb_Content);
            this.panel_TextRun.Controls.Add(this.lb_Title);
            this.panel_TextRun.Location = new System.Drawing.Point(0, 0);
            this.panel_TextRun.Name = "panel_TextRun";
            this.panel_TextRun.SetSpeed = 0;
            this.panel_TextRun.Size = new System.Drawing.Size(920, 420);
            this.panel_TextRun.TabIndex = 4;
            // 
            // lb_Content
            // 
            this.lb_Content.BackColor = System.Drawing.Color.MistyRose;
            this.lb_Content.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.lb_Content.ForeColor = System.Drawing.Color.Navy;
            this.lb_Content.Location = new System.Drawing.Point(3, 93);
            this.lb_Content.Name = "lb_Content";
            this.lb_Content.Size = new System.Drawing.Size(917, 78);
            this.lb_Content.TabIndex = 2;
            this.lb_Content.Text = "    UBND thị trấn Bến Lức thông báo đến nhân dân \"Về việc đeo khẩu trang, hạn chế" +
    " tập trung đông người trên địa bàn thị trấn Bến Lức\" Để chủ động kiểm soát, ngăn" +
    " chặn dịch bệnh Covid-19 gây ra.";
            // 
            // lb_Title
            // 
            this.lb_Title.BackColor = System.Drawing.Color.MistyRose;
            this.lb_Title.Dock = System.Windows.Forms.DockStyle.Top;
            this.lb_Title.Font = new System.Drawing.Font("Times New Roman", 20F, System.Drawing.FontStyle.Bold);
            this.lb_Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lb_Title.Location = new System.Drawing.Point(0, 0);
            this.lb_Title.Name = "lb_Title";
            this.lb_Title.Size = new System.Drawing.Size(920, 93);
            this.lb_Title.TabIndex = 1;
            this.lb_Title.Text = "UBND thị trấn Bến Lức thông báo đến nhân dân \"Về việc đeo khẩu trang, hạn chế tập" +
    " trung đông người trên địa bàn thị trấn Bến Lức\" Để chủ động kiểm soát, ngăn chặ" +
    "n dịch bệnh Covid-19 gây ra.";
            this.lb_Title.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Form_Text
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.MistyRose;
            this.ClientSize = new System.Drawing.Size(920, 420);
            this.Controls.Add(this.panel_TextRun);
            this.Name = "Form_Text";
            this.Text = "Form_Text";
            this.panel_TextRun.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PanelEx panel_TextRun;
        private GrowLabel lb_Content;
        private GrowLabel lb_Title;
    }
}