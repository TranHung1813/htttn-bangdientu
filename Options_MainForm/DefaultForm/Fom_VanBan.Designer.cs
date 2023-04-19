
namespace Display
{
    partial class Form_VanBan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_VanBan));
            this.panel_TextRun = new Display.PanelEx();
            this.txtVanBan = new System.Windows.Forms.Label();
            this.panel_TextRun.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_TextRun
            // 
            this.panel_TextRun.BackColor = System.Drawing.Color.MistyRose;
            this.panel_TextRun.Controls.Add(this.txtVanBan);
            this.panel_TextRun.Location = new System.Drawing.Point(0, 0);
            this.panel_TextRun.Name = "panel_TextRun";
            this.panel_TextRun.SetSpeed = 0;
            this.panel_TextRun.Size = new System.Drawing.Size(476, 836);
            this.panel_TextRun.TabIndex = 5;
            // 
            // txtVanBan
            // 
            this.txtVanBan.AutoSize = true;
            this.txtVanBan.Font = new System.Drawing.Font("Microsoft Sans Serif", 29F);
            this.txtVanBan.ForeColor = System.Drawing.Color.Black;
            this.txtVanBan.Location = new System.Drawing.Point(0, 0);
            this.txtVanBan.MaximumSize = new System.Drawing.Size(477, 0);
            this.txtVanBan.Name = "txtVanBan";
            this.txtVanBan.Size = new System.Drawing.Size(476, 836);
            this.txtVanBan.TabIndex = 5;
            this.txtVanBan.Text = resources.GetString("txtVanBan.Text");
            // 
            // Form_VanBan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MistyRose;
            this.ClientSize = new System.Drawing.Size(476, 788);
            this.Controls.Add(this.panel_TextRun);
            this.Name = "Form_VanBan";
            this.Text = "Form_ThongBao";
            this.panel_TextRun.ResumeLayout(false);
            this.panel_TextRun.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private PanelEx panel_TextRun;
        private System.Windows.Forms.Label txtVanBan;
    }
}