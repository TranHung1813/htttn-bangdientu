using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    //this is a utility static class
    public static class Utility
    {
        public static void fitFormToScreen(Form form, int h, int w)
        {

            //scale the form to the current screen resolution
            int form_h = form.Height;
            int form_w = form.Width;
            form.Height = Screen.PrimaryScreen.Bounds.Size.Height;
            form.Width = Screen.PrimaryScreen.Bounds.Size.Width;
            //form.Height = (int)Math.Round((float)form.Height * ((float)Screen.PrimaryScreen.Bounds.Size.Height / (float)h));
            //form.Width = (int)Math.Round((float)form.Width * ((float)Screen.PrimaryScreen.Bounds.Size.Width / (float)w));

            //here font is scaled like width
            //form.Font = new Font(form.Font.FontFamily, form.Font.Size * ((float)Screen.PrimaryScreen.Bounds.Size.Width / (float)w));

            foreach (Control item in form.Controls)
            {
                fitControlsToContainer(item, form_h, form_w, form.Height, form.Width);
            }
            form.StartPosition = FormStartPosition.CenterScreen;
            ReallyCenterToScreen(form);

        }
        public static void fitFormToContainer(Form form, int Form_Height, int Form_Width, int Container_Height, int Container_Width)
        {

            //scale the form to the current screen resolution
            form.Height = Container_Height;
            form.Width = Container_Width;

            //here font is scaled like width
            form.Font = new Font(form.Font.FontFamily, form.Font.Size * ((float)Container_Width / (float)Form_Width));

            foreach (Control item in form.Controls)
            {
                fitControlsToContainer(item, Form_Height, Form_Width, Container_Height, Container_Width);
                
            }
            //form.StartPosition = FormStartPosition.CenterScreen;
            //ReallyCenterToScreen(form);

        }
        private static void ReallyCenterToScreen(Form form)
        {
            Screen screen = Screen.FromControl(form);

            Rectangle workingArea = screen.WorkingArea;
            form.Location = new Point()
            {
                X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - form.Width) / 2),
                Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - form.Height) / 2)
            };
        }
        public static void FitUserControlToContainer(UserControl form, int Container_Height, int Container_Width)
        {
            //scale the form to the current screen resolution
            int h = form.Height;
            int w = form.Width;
            form.Height = Container_Height;
            form.Width = Container_Width;

            //here font is scaled like width
            form.Font = new Font(form.Font.FontFamily, form.Font.Size * ((float)Container_Height / (float)h), form.Font.Style);

            foreach (Control item in form.Controls)
            {
                //fitControlsToContainer(item, h, w, Container_Height, Container_Width);
                if (item is PanelContainer)
                {
                    //fitControlsToContainer(item, h, w, Container_Height, Container_Width);
                }
                else
                {
                    fitControlsToContainer(item, h, w, Container_Height, Container_Width);
                }
                //if(item is GroupBox)
                //{
                //    fitControlsToContainer(item, h, w, Container_Height, Container_Width);
                //}
            }
        }

        public static void fitControlsToScreen(Control cntrl, int h, int w)
        {
            if (Screen.PrimaryScreen.Bounds.Size.Height != h)
            {

                cntrl.Height = (int)Math.Round((float)cntrl.Height * ((float)Screen.PrimaryScreen.Bounds.Size.Height / (float)h));
                cntrl.Top = (int)Math.Round((float)cntrl.Top * ((float)Screen.PrimaryScreen.Bounds.Size.Height / (float)h));

            }
            if (Screen.PrimaryScreen.Bounds.Size.Width != w)
            {

                cntrl.Width = (int)Math.Round((float)cntrl.Width * ((float)Screen.PrimaryScreen.Bounds.Size.Width / (float)w));
                cntrl.Left = (int)Math.Round((float)cntrl.Left * ((float)Screen.PrimaryScreen.Bounds.Size.Width / (float)w));

                cntrl.Font = new Font(cntrl.Font.FontFamily, (float)(cntrl.Font.Size * ((float)Screen.PrimaryScreen.Bounds.Size.Width / (float)w)), cntrl.Font.Style);

            }

            foreach (Control item in cntrl.Controls)
            {
                fitControlsToScreen(item, h, w);
            }
        }
        static void fitControlsToContainer(Control cntrl, int h, int w, int Container_Height, int Container_Width)
        {
            if (Container_Height != h)
            {

                cntrl.Height = (int)Math.Round((float)cntrl.Height * ((float)Container_Height / (float)h));
                cntrl.Top = (int)Math.Round((float)cntrl.Top * ((float)Container_Height / (float)h));

            }
            if (Container_Width != w)
            {

                cntrl.Width = (int)Math.Round((float)cntrl.Width * ((float)Container_Width / (float)w));
                cntrl.Left = (int)Math.Round((float)cntrl.Left * ((float)Container_Width / (float)w));

                cntrl.Font = new Font(cntrl.Font.FontFamily, (float)(cntrl.Font.Size * ((float)Container_Height / (float)h)), cntrl.Font.Style);

            }

            foreach (Control item in cntrl.Controls)
            {
                fitControlsToContainer(item, h, w, Container_Height, Container_Width);
            }
        }
    }
}
