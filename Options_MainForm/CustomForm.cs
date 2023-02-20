using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class CustomForm : UserControl
    {
        private const int PAGE_VIDEO = 1;
        private const int PAGE_TEXT = 2;
        private const int PAGE_IMAGE = 3;
        private const int PAGE_MULTI_IMAGE = 4;
        private int TabPageID = 0;

        private Page_VideoScreen page_VideoScreen = new Page_VideoScreen();
        private Page_Text page_Text = new Page_Text();
        private Page_Image page_Image = new Page_Image();
        private TextOverlay text_Overlay = new TextOverlay();
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
                    guna2Transition1.HideSync(panel_Image);
                    panel_Image.Visible = false;
                    break;

                case PAGE_TEXT:
                    guna2Transition1.HideSync(panel_Text);
                    panel_Text.Visible = false;
                    break;

                case PAGE_VIDEO:
                    page_VideoScreen.StopVideo();
                    guna2Transition1.HideSync(panel_Video);
                    panel_Video.Visible = false;
                    break;
            }
            try
            {
                guna2Transition1.ShowSync(panel, true);
            }
            catch
            { }
        }

    //-----------------------------------API Show Page Text, Video, Image, Text Overlay-----------------------------//
        public void ShowVideo(string Url)
        {
            ShowPanel(panel_Video, page_VideoScreen);
            page_VideoScreen.ShowVideo(Url);
            TabPageID = PAGE_VIDEO;
        }
        public void ShowText(string Text)
        {
            page_Text = new Page_Text();
            page_Text.ShowText(Text);
            ShowPanel(panel_Text, page_Text);
            TabPageID = PAGE_TEXT;
        }
        public void ShowImage(string ImageURL)
        {
            page_Image.ShowImage(ImageURL);
            ShowPanel(panel_Image, page_Image);
            TabPageID = PAGE_IMAGE;
        }
        public void Show_TextOverlay(string Content)
        {
            //panel_TextOverlay.Visible = true;

            panel_TextOverlay.Controls.Clear();
            panel_TextOverlay.Controls.Add(text_Overlay);
            text_Overlay.Dock = DockStyle.Fill;
            text_Overlay.BringToFront();

            panel_TextOverlay.BringToFront();

            try
            {
                guna2Transition1.ShowSync(panel_TextOverlay, true);
            }
            catch { }

            text_Overlay.ShowTextOverlay(Content);
        }
        public void Show_Multi_Image(string [] ImageURLs, int Number_Image)
        {
            ShowPanel(panel_Multi_Image, page_Multi_Image);
            TabPageID = PAGE_MULTI_IMAGE;

            page_Multi_Image.Show_Multi_Image(ImageURLs, 4);
        }

        //-----------------------------------------------------------------------------------------------------//
    }
}
