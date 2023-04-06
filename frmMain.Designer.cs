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
            this.Timer_MQTT = new System.Windows.Forms.Timer(this.components);
            this.panelContainer = new System.Windows.Forms.Panel();
            this.Timer_FindComPort = new System.Windows.Forms.Timer(this.components);
            this.MQTTPing_Timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // Timer_MQTT
            // 
            this.Timer_MQTT.Interval = 1000;
            this.Timer_MQTT.Tick += new System.EventHandler(this.tick_Tick);
            // 
            // panelContainer
            // 
            this.panelContainer.Location = new System.Drawing.Point(0, 0);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Size = new System.Drawing.Size(1040, 700);
            this.panelContainer.TabIndex = 0;
            // 
            // Timer_FindComPort
            // 
            this.Timer_FindComPort.Enabled = true;
            this.Timer_FindComPort.Interval = 2000;
            this.Timer_FindComPort.Tick += new System.EventHandler(this.Timer_FindComPort_Tick);
            // 
            // MQTTPing_Timer
            // 
            this.MQTTPing_Timer.Enabled = true;
            this.MQTTPing_Timer.Interval = 60000;
            this.MQTTPing_Timer.Tick += new System.EventHandler(this.MQTTPing_Timer_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(1040, 700);
            this.Controls.Add(this.panelContainer);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bảng hiển thị";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer Timer_MQTT;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Timer Timer_FindComPort;
        private System.Windows.Forms.Timer MQTTPing_Timer;
    }
}

