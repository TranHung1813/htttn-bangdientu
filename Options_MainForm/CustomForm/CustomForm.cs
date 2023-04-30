using Serilog;
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

        private const int MAXVALUE = 1000 * 1000 * 1000;

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

            CloseTextOverlay();
        }

        public bool CheckMessage_Available()
        {
            bool returnValue = true;
            switch (TabPageID)
            {
                case PAGE_IMAGE:
                    returnValue = page_Image.form_Image._is_ImageAvailable;
                    break;

                case PAGE_TEXT:
                    returnValue =  page_Text.form_Text._is_VanBanAvailable;
                    break;

                case PAGE_VIDEO:
                    returnValue = page_VideoScreen._is_VideoAvailable;
                    break;

                case PAGE_MULTI_IMAGE:
                    returnValue = false;
                    break;
            }

            return returnValue;
        }
        private void CloseTextOverlay()
        {
            try
            {
                frm_TextOverlay?.CloseForm();
                frm_TextOverlay?.Dispose();
                frm_TextOverlay = null;
            }
            catch { }

            try
            {
                backGround?.Dispose();
                backGround = null;
            }
            catch { }
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

                    if(panel != panel_Image) CloseTextOverlay();

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

                    if (panel != panel_Video) CloseTextOverlay();
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
        public void ShowVideo(string Url, string ScheduleId, int Priority = 0, int StartPos = 0)
        {
            ShowPanel(panel_Video, page_VideoScreen);
            page_VideoScreen.ShowVideo(Url, ScheduleId, Priority, StartPos);
            TabPageID = PAGE_VIDEO;
        }
        public void ShowText(DisplayScheduleType ScheduleType, string Text, string ColorValue = "")
        {
            panel_Text.Visible = false;

            page_Text.PageText_FitToContainer(panel_Text.Height, panel_Text.Width);
            ShowPanel(panel_Text, page_Text);
            page_Text.ShowText(ScheduleType, Text, ColorValue);
            TabPageID = PAGE_TEXT;
        }
        public void ShowText(string Title, string Content, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            panel_Text.Visible = false;

            page_Text.PageText_FitToContainer(panel_Text.Height, panel_Text.Width);
            ShowPanel(panel_Text, page_Text);
            page_Text.ShowText(Title, Content, ScheduleId, Priority, Duration);
            TabPageID = PAGE_TEXT;
        }
        public void ShowImage(string ImageURL, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            ShowPanel(panel_Image, page_Image);
            page_Image.ShowImage(ImageURL, ScheduleId, Priority, Duration);
            TabPageID = PAGE_IMAGE;
        }
        public void Show_TextOverlay(string Content, string ColorValue = "", int Duration = MAXVALUE)
        {
            if (frm_TextOverlay?.CurrentContent == Content && frm_TextOverlay?.TextColorValue == ColorValue) return;
            try
            {
                frm_TextOverlay?.CloseForm();
                frm_TextOverlay?.Dispose();
            }
            catch { }

            try
            {
                backGround?.Dispose();
            }
            catch { }

            frm_TextOverlay = new Frm_TextOverlay();
            backGround = new Transparent_BackGround();

            backGround.StartPosition = FormStartPosition.Manual;
            backGround.Size = panel_TextOverlay.Size;
            backGround.Location = panel_TextOverlay.Location;
            backGround.ShowInTaskbar = false;
            //backGround.Show(); // Tam thoi bỏ Background
            frm_TextOverlay.Owner = backGround;

            frm_TextOverlay.Location = panel_TextOverlay.Location;
            frm_TextOverlay.StartPosition = FormStartPosition.Manual;
            frm_TextOverlay.ShowInTaskbar = false;
            //frm_TextOverlay.Size = panel_TextOverlay.Size;
            //frm_TextOverlay.Notify_TextOverlay_Finish += (object o, Notify_TextRun_Finish e) =>
            //{
            //    if(this.InvokeRequired)
            //    {
            //        this.Invoke((MethodInvoker)delegate
            //        {
            //            frm_TextOverlay.Dispose();
            //            backGround.Dispose();
            //        });
            //    }
            //    else
            //    {
            //        frm_TextOverlay.Dispose();
            //        backGround.Dispose();
            //    }
            //};
            frm_TextOverlay.TxtOverlay_FitToContainer(panel_TextOverlay.Height, panel_TextOverlay.Width);
            frm_TextOverlay.ShowTextOverlay(Content, ColorValue, Duration);
            frm_TextOverlay.BringToFront();
            frm_TextOverlay.Show();
            
        }
        public void Show_Multi_Image(string [] ImageURLs, int Number_Image)
        {
            page_Multi_Image.Show_Multi_Image(ImageURLs, Number_Image);
            ShowPanel(panel_Multi_Image, page_Multi_Image);
            TabPageID = PAGE_MULTI_IMAGE;
        }

        public void GetScheduleInfo(ref string ScheduleID, ref string PlayingFile, ref int PlayState, ref bool IsSpkOn, ref int Volume)
        {
            if (TabPageID != PAGE_VIDEO) return;

            page_VideoScreen.GetScheduleInfo(ref ScheduleID, ref PlayingFile, ref PlayState, ref IsSpkOn, ref Volume);
        }
        public void SetVolume(int Volume)
        {
            if (TabPageID != PAGE_VIDEO) return;

            page_VideoScreen.SetVolume(Volume);
        }
        public void Close_by_Id(string ScheduleId)
        {
            switch (TabPageID)
            {
                case PAGE_IMAGE:
                    //guna2Transition1.HideSync(panel_Image);
                    if(page_Image.form_Image.ScheduleID_Image == ScheduleId)
                    {
                        Log.Information("Ban tin Hinh Anh het thoi gian Valid!");
                        page_Image.Close();

                        CloseTextOverlay();
                        panel_Image.Visible = false;
                    }

                    break;

                case PAGE_TEXT:
                    //guna2Transition1.HideSync(panel_Text);
                    if(page_Text.form_Text.ScheduleID_VanBan == ScheduleId)
                    {
                        Log.Information("Ban tin Van Ban het thoi gian Valid!");
                        page_Text.Close();

                        panel_Text.Visible = false;
                    }

                    break;

                case PAGE_VIDEO:
                    if(page_VideoScreen.ScheduleID_Video == ScheduleId)
                    {
                        Log.Information("Ban tin Video het thoi gian Valid!");
                        page_VideoScreen.StopVideo();

                        CloseTextOverlay();
                        panel_Video.Visible = false;
                    }


                    break;
            }
        }
        public bool CheckPriority(int Priority)
        {
            switch (TabPageID)
            {
                case PAGE_IMAGE:
                    if (page_Image.form_Image._Priority_Image < Priority)
                    {
                        return false;
                    }
                    break;

                case PAGE_TEXT:
                    if (page_Text.form_Text._Priority_VanBan < Priority)
                    {
                        return false;
                    }
                    break;

                case PAGE_VIDEO:
                    if (page_VideoScreen._Priority_Video < Priority)
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        //-----------------------------------------------------------------------------------------------------//
    }
}
