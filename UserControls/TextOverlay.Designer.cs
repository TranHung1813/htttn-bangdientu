
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
            this.txtOverlay = new Display.TextEx2();
            this.timer_DelayTextRun = new System.Windows.Forms.Timer(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.lb_Title = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.txtOverlay);
            this.panel1.Location = new System.Drawing.Point(93, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(725, 53);
            this.panel1.TabIndex = 0;
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
            this.txtOverlay.ForeColor = System.Drawing.Color.Brown;
            this.txtOverlay.Location = new System.Drawing.Point(3, 3);
            this.txtOverlay.Name = "txtOverlay";
            this.txtOverlay.SetSpeed = 0;
            this.txtOverlay.Size = new System.Drawing.Size(4851, 54);
            this.txtOverlay.TabIndex = 3;
            this.txtOverlay.Text = resources.GetString("txtOverlay.Text");
            this.txtOverlay.UseCompatibleTextRendering = true;
            // 
            // timer_DelayTextRun
            // 
            this.timer_DelayTextRun.Interval = 3000;
            this.timer_DelayTextRun.Tick += new System.EventHandler(this.timer_DelayTextRun_Tick);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lb_Title);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(92, 53);
            this.panel2.TabIndex = 1;
            // 
            // lb_Title
            // 
            this.lb_Title.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_Title.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_Title.ForeColor = System.Drawing.Color.Red;
            this.lb_Title.Location = new System.Drawing.Point(3, 3);
            this.lb_Title.Name = "lb_Title";
            this.lb_Title.Size = new System.Drawing.Size(92, 53);
            this.lb_Title.TabIndex = 0;
            this.lb_Title.Text = "Thông báo";
            this.lb_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lb_Title.UseCompatibleTextRendering = true;
            // 
            // TextOverlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "TextOverlay";
            this.Size = new System.Drawing.Size(818, 53);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private TextEx2 txtOverlay;
        private System.Windows.Forms.Timer timer_DelayTextRun;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lb_Title;
    }
}
