using Serilog;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Display
{
    public partial class Frm_TextOverlay : CSWinFormLayeredWindow.PerPixelAlphaForm
    {
        System.Timers.Timer Moving_Tmr;
        private float _Text_Size = 38;
        private float _OutlineWidth = (float)4.8;

        public string CurrentContent = "";
        public string TextColorValue = "";
        private bool _isValid = false;

        System.Timers.Timer Duration_TextOverlay_Tmr;

        private const int MAXVALUE = 1000 * 1000 * 1000;

        private int MaxPosition = 0;
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams baseParams = base.CreateParams;

        //        const int WS_EX_NOACTIVATE = 0x08000000;
        //        const int WS_EX_TOOLWINDOW = 0x00000080;
        //        baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);

        //        return baseParams;
        //    }
        //}
        public Frm_TextOverlay()
        {
            InitializeComponent();
            Moving_Tmr = new System.Timers.Timer();
            Moving_Tmr.Interval = 2000;
            Moving_Tmr.Elapsed += Moving_Tmr_Elapsed;

            Duration_TextOverlay_Tmr = new System.Timers.Timer();
        }
        private void Moving_Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Moving_Tmr.Interval = 12;
            if (this.Location.X < -MaxPosition)
            {
                if (_isValid == false)
                {
                    Moving_Tmr.Stop();
                    return;
                }
                this.Location = new Point((int)Screen.PrimaryScreen.Bounds.Size.Width, this.Location.Y);
            }
            this.Location = new Point(this.Location.X - 1, this.Location.Y);
        }

        public void ShowTextOverlay(string Content, string ColorValue, int Duration = MAXVALUE)
        {
            CurrentContent = Content;// "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
            TextColorValue = ColorValue;
            Font font = new Font("Arial", _Text_Size, FontStyle.Bold);
            Color TextColor = Color.Red;
            if (ColorValue != "") TextColor = ColorTranslator.FromHtml(ColorValue);

            // Draw Text
            int Meas_Width = (int)this.CreateGraphics().MeasureString(CurrentContent, font).Width;
            MaxPosition = Meas_Width;
            int Meas_Height = (int)(this.CreateGraphics().MeasureString(CurrentContent, font).Height * 1.3);
            this.SelectBitmap(ConvertTextToImage(CurrentContent, font, Color.Transparent, Color.Honeydew, TextColor, _OutlineWidth, Meas_Width, Meas_Height));

            // Set First Location
            int NewLocationY = this.Location.Y + ((int)Screen.PrimaryScreen.Bounds.Size.Height - this.Location.Y - (int)(Meas_Height / 1.3)) / 2;
            this.Location = new Point(this.Location.X + (int)Screen.PrimaryScreen.Bounds.Size.Width, NewLocationY);

            Moving_Tmr.Stop();
            Moving_Tmr.Start();

            // Duration Handle
            Duration_Handle(ref Duration_TextOverlay_Tmr, Duration, () =>
            {
                // Stop Media
                _isValid = false;
                Log.Information("Text Overlay Stop!");
            });
            _isValid = true;
        }

        private void Duration_Handle(ref System.Timers.Timer tmr, int Duration, Action action)
        {
            try
            {
                tmr.Stop();
                tmr.Dispose();
            }
            catch { }
            tmr = new System.Timers.Timer();
            // Xu ly Duration cho text content
            tmr.Interval = Duration + 1000;
            tmr.Elapsed += (o, ev) =>
            {
                action();
                // Stop this Timer
                System.Timers.Timer thisTimer = (System.Timers.Timer)o;
                thisTimer.Stop();
                thisTimer.Dispose();
            };
            tmr.Start();
        }

        public Bitmap ConvertTextToImage(string txt, Font font, Color bgcolor, Color fcolor,
                                    Color OutlineForeColor, float OutlineWidth, int width, int Height)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {

                //Font font = new Font(fontname, fontsize);
                //graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                //graphics.DrawString(txt, font, new SolidBrush(fcolor), 0, 0);
                //graphics.Flush();
                //font.Dispose();
                //graphics.Dispose();

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Rectangle rectangle = new Rectangle(0, 0, bmp.Width, bmp.Height);

                //graphics.FillRectangle(new SolidBrush(bgcolor), rectangle);
                using (GraphicsPath gp = new GraphicsPath())
                using (Pen outline = new Pen(OutlineForeColor, OutlineWidth)
                { LineJoin = LineJoin.Round })
                using (StringFormat sf = new StringFormat())
                using (Brush foreBrush = new SolidBrush(fcolor))
                {
                    gp.AddString(txt, font.FontFamily, (int)font.Style,
                        font.Size, rectangle, sf);
                    graphics.ScaleTransform(1.3f, 1.35f);

                    graphics.DrawPath(outline, gp);
                    graphics.FillPath(foreBrush, gp);
                }
                graphics.FillRectangle(new SolidBrush(Color.Transparent), rectangle);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }
            return bmp;
        }
        public void CloseForm()
        {
            Moving_Tmr?.Stop();
            Moving_Tmr?.Dispose();

            Duration_TextOverlay_Tmr?.Stop();
            Duration_TextOverlay_Tmr?.Dispose();

            CurrentContent = "";
            TextColorValue = "";
            _isValid = false;
    }

        public void TxtOverlay_FitToContainer(int Height, int Width)
        {
            int h = this.Height;
            Utility.fitFormToContainer(this, this.Height, this.Width, Height, Width);
            _Text_Size = _Text_Size * ((float)Height / (float)h);
            _OutlineWidth = _OutlineWidth * ((float)Height / (float)h);
        }
    }
}
