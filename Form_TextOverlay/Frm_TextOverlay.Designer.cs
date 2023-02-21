
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.timer_DelayText = new System.Windows.Forms.Timer(this.components);
            this.txtOverlay = new Display.TextEx2();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.txtOverlay);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1023, 87);
            this.panel1.TabIndex = 0;
            // 
            // timer_DelayText
            // 
            this.timer_DelayText.Interval = 1000;
            this.timer_DelayText.Tick += new System.EventHandler(this.timer_DelayText_Tick);
            // 
            // txtOverlay
            // 
            this.txtOverlay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOverlay.AutoEllipsis = true;
            this.txtOverlay.AutoSize = true;
            this.txtOverlay.BackColor = System.Drawing.Color.Transparent;
            this.txtOverlay.Font = new System.Drawing.Font("Arial", 30F, System.Drawing.FontStyle.Bold);
            this.txtOverlay.ForeColor = System.Drawing.Color.White;
            this.txtOverlay.Location = new System.Drawing.Point(0, 5);
            this.txtOverlay.Margin = new System.Windows.Forms.Padding(0);
            this.txtOverlay.Name = "txtOverlay";
            this.txtOverlay.OutlineForeColor = System.Drawing.Color.Lime;
            this.txtOverlay.OutlineWidth = 4F;
            this.txtOverlay.SetSpeed = 0;
            this.txtOverlay.Size = new System.Drawing.Size(5056, 53);
            this.txtOverlay.TabIndex = 4;
            this.txtOverlay.Text = resources.GetString("txtOverlay.Text");
            this.txtOverlay.UseCompatibleTextRendering = true;
            // 
            // Frm_TextOverlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(37F, 73F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(1023, 87);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(18, 17, 18, 17);
            this.Name = "Frm_TextOverlay";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Frm_TextOverlay";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Timer timer_DelayText;
        private TextEx2 txtOverlay;
    }
}