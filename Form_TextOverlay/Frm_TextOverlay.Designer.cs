
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Frm_TextOverlay));
            this.timer_DelayText = new System.Windows.Forms.Timer(this.components);
            this.panel_TxtOverlay = new Display.PanelEx2();
            this.txtOverlay = new Display.TextEx2();
            this.panel_TxtOverlay.SuspendLayout();
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
            this.panel_TxtOverlay.Controls.Add(this.txtOverlay);
            this.panel_TxtOverlay.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel_TxtOverlay.Location = new System.Drawing.Point(0, 0);
            this.panel_TxtOverlay.Margin = new System.Windows.Forms.Padding(0);
            this.panel_TxtOverlay.Max_Repeat_Time = 0;
            this.panel_TxtOverlay.Name = "panel_TxtOverlay";
            this.panel_TxtOverlay.SetSpeed = 0;
            this.panel_TxtOverlay.Size = new System.Drawing.Size(1023, 72);
            this.panel_TxtOverlay.TabIndex = 0;
            // 
            // txtOverlay
            // 
            this.txtOverlay.AutoEllipsis = true;
            this.txtOverlay.AutoSize = true;
            this.txtOverlay.BackColor = System.Drawing.Color.Transparent;
            this.txtOverlay.Font = new System.Drawing.Font("Arial", 30F, System.Drawing.FontStyle.Bold);
            this.txtOverlay.ForeColor = System.Drawing.Color.Honeydew;
            this.txtOverlay.Location = new System.Drawing.Point(0, 5);
            this.txtOverlay.Margin = new System.Windows.Forms.Padding(0);
            this.txtOverlay.Max_Repeat_Time = 0;
            this.txtOverlay.Name = "txtOverlay";
            this.txtOverlay.OutlineForeColor = System.Drawing.Color.Red;
            this.txtOverlay.OutlineWidth = 3.5F;
            this.txtOverlay.SetSpeed = 0;
            this.txtOverlay.Size = new System.Drawing.Size(5056, 53);
            this.txtOverlay.TabIndex = 4;
            this.txtOverlay.Text = resources.GetString("txtOverlay.Text");
            this.txtOverlay.UseCompatibleTextRendering = true;
            // 
            // Frm_TextOverlay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(1023, 72);
            this.Controls.Add(this.panel_TxtOverlay);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(18, 18, 18, 18);
            this.Name = "Frm_TextOverlay";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Frm_TextOverlay";
            this.panel_TxtOverlay.ResumeLayout(false);
            this.panel_TxtOverlay.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private PanelEx2 panel_TxtOverlay;
        private System.Windows.Forms.Timer timer_DelayText;
        private TextEx2 txtOverlay;
    }
}