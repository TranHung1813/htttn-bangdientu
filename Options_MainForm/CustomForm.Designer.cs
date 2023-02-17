
namespace Display.Options_MainForm
{
    partial class CustomForm
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
            this.panel_TextOverlay = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel_TextOverlay
            // 
            this.panel_TextOverlay.BackColor = System.Drawing.Color.LavenderBlush;
            this.panel_TextOverlay.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_TextOverlay.Location = new System.Drawing.Point(0, 422);
            this.panel_TextOverlay.Name = "panel_TextOverlay";
            this.panel_TextOverlay.Size = new System.Drawing.Size(915, 53);
            this.panel_TextOverlay.TabIndex = 1;
            this.panel_TextOverlay.Visible = false;
            // 
            // CustomForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Controls.Add(this.panel_TextOverlay);
            this.Name = "CustomForm";
            this.Size = new System.Drawing.Size(915, 475);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_TextOverlay;
    }
}
