namespace Display
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtThongBao = new Display.TextEx();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtVanBan = new Display.TextEx();
            this.tick = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // axWindowsMediaPlayer1
            // 
            this.axWindowsMediaPlayer1.Enabled = true;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(451, 142);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(421, 407);
            this.axWindowsMediaPlayer1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtThongBao);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(859, 115);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông báo";
            // 
            // txtThongBao
            // 
            this.txtThongBao.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtThongBao.Location = new System.Drawing.Point(3, 16);
            this.txtThongBao.Name = "txtThongBao";
            this.txtThongBao.SetSpeed = 1;
            this.txtThongBao.Size = new System.Drawing.Size(853, 96);
            this.txtThongBao.TabIndex = 1;
            this.txtThongBao.Text = resources.GetString("txtThongBao.Text");
            this.txtThongBao.UseCompatibleTextRendering = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtVanBan);
            this.groupBox2.Location = new System.Drawing.Point(13, 134);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(432, 415);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Văn bản";
            // 
            // txtVanBan
            // 
            this.txtVanBan.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVanBan.Location = new System.Drawing.Point(7, 20);
            this.txtVanBan.Name = "txtVanBan";
            this.txtVanBan.SetSpeed = 1;
            this.txtVanBan.Size = new System.Drawing.Size(419, 392);
            this.txtVanBan.TabIndex = 0;
            this.txtVanBan.Text = resources.GetString("txtVanBan.Text");
            this.txtVanBan.UseCompatibleTextRendering = true;
            // 
            // tick
            // 
            this.tick.Enabled = true;
            this.tick.Interval = 1000;
            this.tick.Tick += new System.EventHandler(this.tick_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.axWindowsMediaPlayer1);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bảng hiển thị";
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private TextEx txtThongBao;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private TextEx txtVanBan;
        private System.Windows.Forms.Timer tick;
    }
}

