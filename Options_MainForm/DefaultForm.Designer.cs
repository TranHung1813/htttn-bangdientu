
namespace Display
{
    partial class DefaultForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefaultForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelThongBao = new Display.PanelEx();
            this.txtThongBao = new Display.GrowLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panelVanBan = new Display.PanelEx();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.txtVanBan = new System.Windows.Forms.Label();
            this.videoView1 = new LibVLCSharp.WinForms.VideoView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panelThongBao.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.panelVanBan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.MistyRose;
            this.panel1.Controls.Add(this.panelThongBao);
            this.panel1.Location = new System.Drawing.Point(5, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1030, 121);
            this.panel1.TabIndex = 0;
            // 
            // panelThongBao
            // 
            this.panelThongBao.BackColor = System.Drawing.Color.MistyRose;
            this.panelThongBao.Controls.Add(this.txtThongBao);
            this.panelThongBao.Controls.Add(this.pictureBox1);
            this.panelThongBao.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelThongBao.Location = new System.Drawing.Point(0, 0);
            this.panelThongBao.Name = "panelThongBao";
            this.panelThongBao.SetSpeed = 0;
            this.panelThongBao.Size = new System.Drawing.Size(1030, 121);
            this.panelThongBao.TabIndex = 2;
            // 
            // txtThongBao
            // 
            this.txtThongBao.Font = new System.Drawing.Font("Times New Roman", 28.5F, System.Drawing.FontStyle.Bold);
            this.txtThongBao.ForeColor = System.Drawing.Color.DarkRed;
            this.txtThongBao.Location = new System.Drawing.Point(0, 3);
            this.txtThongBao.Name = "txtThongBao";
            this.txtThongBao.Size = new System.Drawing.Size(1030, 215);
            this.txtThongBao.TabIndex = 4;
            this.txtThongBao.Text = resources.GetString("txtThongBao.Text");
            this.txtThongBao.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(125, 71);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.MistyRose;
            this.panel2.Controls.Add(this.panelVanBan);
            this.panel2.Location = new System.Drawing.Point(5, 139);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(488, 529);
            this.panel2.TabIndex = 2;
            // 
            // panelVanBan
            // 
            this.panelVanBan.BackColor = System.Drawing.Color.MistyRose;
            this.panelVanBan.Controls.Add(this.pictureBox2);
            this.panelVanBan.Controls.Add(this.txtVanBan);
            this.panelVanBan.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelVanBan.Location = new System.Drawing.Point(0, 0);
            this.panelVanBan.Name = "panelVanBan";
            this.panelVanBan.SetSpeed = 0;
            this.panelVanBan.Size = new System.Drawing.Size(488, 529);
            this.panelVanBan.TabIndex = 1;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(158, 88);
            this.pictureBox2.TabIndex = 3;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Visible = false;
            // 
            // txtVanBan
            // 
            this.txtVanBan.AutoSize = true;
            this.txtVanBan.Font = new System.Drawing.Font("Microsoft Sans Serif", 29F);
            this.txtVanBan.ForeColor = System.Drawing.Color.Black;
            this.txtVanBan.Location = new System.Drawing.Point(2, 0);
            this.txtVanBan.MaximumSize = new System.Drawing.Size(477, 0);
            this.txtVanBan.Name = "txtVanBan";
            this.txtVanBan.Size = new System.Drawing.Size(476, 836);
            this.txtVanBan.TabIndex = 4;
            this.txtVanBan.Text = resources.GetString("txtVanBan.Text");
            // 
            // videoView1
            // 
            this.videoView1.BackColor = System.Drawing.Color.Black;
            this.videoView1.Location = new System.Drawing.Point(505, 139);
            this.videoView1.MediaPlayer = null;
            this.videoView1.Name = "videoView1";
            this.videoView1.Size = new System.Drawing.Size(531, 529);
            this.videoView1.TabIndex = 6;
            this.videoView1.Text = "videoView1";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(-1, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1042, 1);
            this.label1.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(499, 132);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1, 540);
            this.label2.TabIndex = 8;
            // 
            // DefaultForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(15)))), ((int)(((byte)(25)))));
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.videoView1);
            this.Name = "DefaultForm";
            this.Size = new System.Drawing.Size(1040, 673);
            this.panel1.ResumeLayout(false);
            this.panelThongBao.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panelVanBan.ResumeLayout(false);
            this.panelVanBan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private PanelEx panelThongBao;
        private PanelEx panelVanBan;
        private LibVLCSharp.WinForms.VideoView videoView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label txtVanBan;
        private GrowLabel txtThongBao;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
