using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace Display
{
    public partial class CustomForm : UserControl
    {
        private const int PAGE_VIDEO = 1;
        private const int PAGE_TEXT = 2;
        private const int PAGE_IMAGE = 3;
        private const int PAGE_MULTI_IMAGE = 4;
        private int TabPageID = 0;

        Frm_TextOverlay frm_TextOverlay;
        Transparent_BackGround backGround;

        private Page_VideoScreen page_VideoScreen = new Page_VideoScreen();
        private Page_Text page_Text = new Page_Text();
        private Page_Image page_Image = new Page_Image();
        private Page_Multi_Image page_Multi_Image = new Page_Multi_Image();

        private PanelContainer panel_Video;
        private PanelContainer panel_Text;
        private PanelContainer panel_Image;
        private PanelContainer panel_Multi_Image;

        public CustomForm()
        {
            InitializeComponent();

            panel_Video = new PanelContainer(this);
            panel_Text = new PanelContainer(this);
            panel_Image = new PanelContainer(this);
            panel_Multi_Image = new PanelContainer(this);
        }
        public void Close()
        {
            switch (TabPageID)
            {
                case PAGE_IMAGE:
                    page_Image.Close();
                    panel_Image.Visible = false;

                    break;

                case PAGE_TEXT:
                    page_Text.Close();
                    panel_Text.Visible = false;

                    break;

                case PAGE_VIDEO:
                    page_VideoScreen.StopVideo();
                    panel_Video.Visible = false;
                    break;

                case PAGE_MULTI_IMAGE:
                    page_Multi_Image.Close();
                    panel_Multi_Image.Visible = false;
                    break;
            }

            if (frm_TextOverlay != null)
            {
                try
                {
                    frm_TextOverlay.Dispose();
                }
                catch { }
            }
            if (backGround != null)
            {
                try
                {
                    backGround.Dispose();
                }
                catch { }
            }
        }

        private void ShowPanel(PanelContainer panel, UserControl uc)
        {
            panel.Controls.Clear();
            panel.Controls.Add(uc);
            uc.Dock = DockStyle.Fill;
            uc.BringToFront();
            uc.Focus();

            switch (TabPageID)
            {
                case PAGE_IMAGE:
                    //guna2Transition1.HideSync(panel_Image);
                    page_Image.Close();
                    panel_Image.Visible = false;

                    break;

                case PAGE_TEXT:
                    //guna2Transition1.HideSync(panel_Text);
                    page_Text.Close();
                    panel_Text.Visible = false;

                    break;

                case PAGE_VIDEO:
                    page_VideoScreen.StopVideo();
                    //guna2Transition1.HideSync(panel_Video);
                    panel_Video.Visible = false;

                    //panel.Visible = true;
                    //guna2Transition1.ShowSync(panel);
                    break;

                case PAGE_MULTI_IMAGE:
                    panel_Multi_Image.Visible = false;
                    page_Multi_Image.Close();
                    break;

            }
            try
            {
                panel.Visible = true;
                //guna2Transition1.ShowSync(panel, true);
            }
            catch
            { }
        }

        public void CustomForm_FitToContainer(int Height, int Width)
        {
            Utility.FitUserControlToContainer(this, Height, Width);
        }



        //-----------------------------------API Show Page Text, Video, Image, Text Overlay-----------------------------//
        public void ShowVideo(string Url, int IdleTime = 0, int loopNum = Page_VideoScreen.MAXVALUE, int Duration = Page_VideoScreen.MAXVALUE)
        {
            ShowPanel(panel_Video, page_VideoScreen);
            page_VideoScreen.ShowVideo(Url, IdleTime, loopNum, Duration);
            TabPageID = PAGE_VIDEO;
        }
        public void ShowText(DisplayScheduleType ScheduleType, string Text)
        {
            panel_Text.Visible = false;

            page_Text.PageText_FitToContainer(panel_Text.Height, panel_Text.Width);
            ShowPanel(panel_Text, page_Text);
            page_Text.ShowText(ScheduleType, Text);
            TabPageID = PAGE_TEXT;
        }
        public void ShowImage(string ImageURL, int Duration)
        {
            page_Image.ShowImage(ImageURL, Duration);
            ShowPanel(panel_Image, page_Image);
            TabPageID = PAGE_IMAGE;
        }
        public void Show_TextOverlay(string Content)
        {
            if (frm_TextOverlay != null)
            {
                try
                {
                    frm_TextOverlay.Dispose();
                }
                catch { }
            }
            if (backGround != null)
            {
                try
                {
                    backGround.Dispose();
                }
                catch { }
            }

            frm_TextOverlay = new Frm_TextOverlay();
            backGround = new Transparent_BackGround();

            backGround.StartPosition = FormStartPosition.Manual;
            backGround.Size = panel_TextOverlay.Size;
            backGround.Location = panel_TextOverlay.Location;
            backGround.ShowInTaskbar = false;
            backGround.Show();
            frm_TextOverlay.Owner = backGround;

            frm_TextOverlay.Location = panel_TextOverlay.Location;
            frm_TextOverlay.StartPosition = FormStartPosition.Manual;
            frm_TextOverlay.ShowInTaskbar = false;
            //frm_TextOverlay.Size = panel_TextOverlay.Size;
            frm_TextOverlay.Notify_TextOverlay_Finish += (object o, Notify_TextRun_Finish e) =>
            {
                if(this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        frm_TextOverlay.Dispose();
                        backGround.Dispose();
                    });
                }
                else
                {
                    frm_TextOverlay.Dispose();
                    backGround.Dispose();
                }
            };
            frm_TextOverlay.TxtOverlay_FitToContainer(panel_TextOverlay.Height, panel_TextOverlay.Width);
            frm_TextOverlay.ShowTextOverlay("");
            //frm_TextOverlay.BringToFront();
            frm_TextOverlay.Show();
            
        }
        public void Show_Multi_Image(string [] ImageURLs, int Number_Image)
        {
            page_Multi_Image.Show_Multi_Image(ImageURLs, Number_Image);
            ShowPanel(panel_Multi_Image, page_Multi_Image);
            TabPageID = PAGE_MULTI_IMAGE;
        }
        public void Test()
        {
            page_Text.Test();
        }

        //-----------------------------------------------------------------------------------------------------//
    }
}
