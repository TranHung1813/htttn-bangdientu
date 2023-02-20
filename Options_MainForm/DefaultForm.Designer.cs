﻿
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefaultForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panelThongBao = new Display.PanelEx();
            this.txtThongBao = new Display.GrowLabel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panelVanBan = new Display.PanelEx();
            this.txtVanBan = new Display.GrowLabel();
            this.videoView1 = new LibVLCSharp.WinForms.VideoView();
            this.Timer_DelayTextRun = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.panelThongBao.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panelVanBan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.videoView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(13, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1017, 127);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông báo";
            // 
            // panelThongBao
            // 
            this.panelThongBao.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelThongBao.BackColor = System.Drawing.Color.MistyRose;
            this.panelThongBao.Controls.Add(this.txtThongBao);
            this.panelThongBao.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelThongBao.Location = new System.Drawing.Point(0, 0);
            this.panelThongBao.Name = "panelThongBao";
            this.panelThongBao.SetSpeed = 0;
            this.panelThongBao.Size = new System.Drawing.Size(1005, 98);
            this.panelThongBao.TabIndex = 2;
            // 
            // txtThongBao
            // 
            this.txtThongBao.BackColor = System.Drawing.Color.Transparent;
            this.txtThongBao.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtThongBao.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtThongBao.ForeColor = System.Drawing.Color.DarkRed;
            this.txtThongBao.Location = new System.Drawing.Point(0, 0);
            this.txtThongBao.Name = "txtThongBao";
            this.txtThongBao.Size = new System.Drawing.Size(1005, 230);
            this.txtThongBao.TabIndex = 1;
            this.txtThongBao.Text = resources.GetString("txtThongBao.Text");
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.panel2);
            this.groupBox2.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(13, 143);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(442, 518);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Văn bản";
            // 
            // panelVanBan
            // 
            this.panelVanBan.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelVanBan.BackColor = System.Drawing.Color.MistyRose;
            this.panelVanBan.Controls.Add(this.txtVanBan);
            this.panelVanBan.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelVanBan.Location = new System.Drawing.Point(0, 0);
            this.panelVanBan.Name = "panelVanBan";
            this.panelVanBan.SetSpeed = 0;
            this.panelVanBan.Size = new System.Drawing.Size(430, 493);
            this.panelVanBan.TabIndex = 1;
            // 
            // txtVanBan
            // 
            this.txtVanBan.BackColor = System.Drawing.Color.Transparent;
            this.txtVanBan.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtVanBan.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVanBan.ForeColor = System.Drawing.Color.Black;
            this.txtVanBan.Location = new System.Drawing.Point(0, 0);
            this.txtVanBan.Name = "txtVanBan";
            this.txtVanBan.Size = new System.Drawing.Size(430, 1242);
            this.txtVanBan.TabIndex = 0;
            this.txtVanBan.Text = resources.GetString("txtVanBan.Text");
            // 
            // videoView1
            // 
            this.videoView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoView1.BackColor = System.Drawing.Color.Black;
            this.videoView1.Location = new System.Drawing.Point(461, 153);
            this.videoView1.MediaPlayer = null;
            this.videoView1.Name = "videoView1";
            this.videoView1.Size = new System.Drawing.Size(569, 508);
            this.videoView1.TabIndex = 6;
            this.videoView1.Text = "videoView1";
            // 
            // Timer_DelayTextRun
            // 
            this.Timer_DelayTextRun.Interval = 5000;
            this.Timer_DelayTextRun.Tick += new System.EventHandler(this.Timer_DelayTextRun_Tick);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.MistyRose;
            this.panel1.Controls.Add(this.panelThongBao);
            this.panel1.Location = new System.Drawing.Point(6, 20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1005, 98);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.Color.MistyRose;
            this.panel2.Controls.Add(this.panelVanBan);
            this.panel2.Location = new System.Drawing.Point(6, 20);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(430, 493);
            this.panel2.TabIndex = 2;
            // 
            // DefaultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.Controls.Add(this.videoView1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "DefaultForm";
            this.Size = new System.Drawing.Size(1040, 673);
            this.groupBox1.ResumeLayout(false);
            this.panelThongBao.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.panelVanBan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.videoView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private PanelEx panelThongBao;
        private GrowLabel txtThongBao;
        private System.Windows.Forms.GroupBox groupBox2;
        private PanelEx panelVanBan;
        private GrowLabel txtVanBan;
        private LibVLCSharp.WinForms.VideoView videoView1;
        private System.Windows.Forms.Timer Timer_DelayTextRun;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}
