
namespace Display
{
    partial class Form_ThongBao
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_ThongBao));
            this.panel_TextRun = new Display.PanelEx();
            this.txtThongBao = new Display.GrowLabel();
            this.panel_TextRun.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_TextRun
            // 
            this.panel_TextRun.BackColor = System.Drawing.Color.MistyRose;
            this.panel_TextRun.Controls.Add(this.txtThongBao);
            this.panel_TextRun.Location = new System.Drawing.Point(0, 0);
            this.panel_TextRun.Name = "panel_TextRun";
            this.panel_TextRun.SetSpeed = 0;
            this.panel_TextRun.Size = new System.Drawing.Size(1030, 215);
            this.panel_TextRun.TabIndex = 5;
            // 
            // txtThongBao
            // 
            this.txtThongBao.Font = new System.Drawing.Font("Times New Roman", 28.5F, System.Drawing.FontStyle.Bold);
            this.txtThongBao.ForeColor = System.Drawing.Color.DarkRed;
            this.txtThongBao.Location = new System.Drawing.Point(0, 3);
            this.txtThongBao.Name = "txtThongBao";
            this.txtThongBao.Size = new System.Drawing.Size(1030, 215);
            this.txtThongBao.TabIndex = 5;
            this.txtThongBao.Text = resources.GetString("txtThongBao.Text");
            this.txtThongBao.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Form_ThongBao
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1030, 215);
            this.Controls.Add(this.panel_TextRun);
            this.Name = "Form_ThongBao";
            this.Text = "Form_ThongBao";
            this.panel_TextRun.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PanelEx panel_TextRun;
        private GrowLabel txtThongBao;
    }
}