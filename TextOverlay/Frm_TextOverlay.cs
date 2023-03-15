using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Display
{
    public partial class Frm_TextOverlay : Form
    {
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;

                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);

                return baseParams;
            }
        }
        private const int MAX_REPEAT_TIME = 2;
        private string _Text = "";
        private float _Text_Size = 31;
        public Frm_TextOverlay()
        {
            InitializeComponent();

            BackColor = Color.Crimson;
            TransparencyKey = Color.Crimson;

            panel_TxtOverlay.NotifyEndProcess_TextRun += (object o, NotifyEndProcess e) =>
            {
                OnNotify_TextRun_Finish(true);
                //panel_TxtOverlay.NotifyEndProcess_TextRun = new EventHandler<NotifyEndProcess>();
            };
        }
        private event EventHandler<Notify_TextRun_Finish> _Notify_TextOverlay_Finish;
        public event EventHandler<Notify_TextRun_Finish> Notify_TextOverlay_Finish
        {
            add
            {
                _Notify_TextOverlay_Finish += value;
            }
            remove
            {
                _Notify_TextOverlay_Finish -= value;
            }
        }
        protected virtual void OnNotify_TextRun_Finish(bool isFinished)
        {
            if (_Notify_TextOverlay_Finish != null)
            {
                _Notify_TextOverlay_Finish(this, new Notify_TextRun_Finish(isFinished));
            }
        }

        public void ShowTextOverlay(string Txt)
        {
            _Text = Txt;
            _Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ làm công tác TTCS cấp tỉnh, cấp huyện và cấp xã được cấp tài khoản để sử dụng các chức năng trên HTTT nguồn cấp tỉnh thực hiện công tác TTCS.";
            //_Text = "HTTT nguồn cấp tỉnh là hệ thống dùng chung phục vụ hoạt động TTCS ở cả 3 cấp tỉnh, huyện và xã. Cán bộ";

            //txtOverlay.Text = _Text;
            Font font = new Font("Arial", _Text_Size, FontStyle.Bold);
            pictureBox1.Width = (int)this.CreateGraphics().MeasureString(_Text, font).Width;
            pictureBox1.Height = (int)(this.CreateGraphics().MeasureString(_Text, font).Height * 1.3);
            pictureBox1.Image = ConvertTextToImage(_Text, font, Color.Transparent, Color.Honeydew,Color.Red, (float)3.5, pictureBox1.Width, pictureBox1.Height);

            panel_TxtOverlay.Visible = false;
            timer_DelayText.Interval = 1000;
            timer_DelayText.Start();
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

        public Bitmap ConvertTextToImage(string txt, Font font, Color bgcolor, Color fcolor, int width, int Height)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                Rectangle rectF1 = new Rectangle(0, 0, width, Height);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), rectF1);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();


            }
            return bmp;
        }

        private void timer_DelayText_Tick(object sender, EventArgs e)
        {
            panel_TxtOverlay.Visible = true;
            panel_TxtOverlay.SetSpeed = 2;
            panel_TxtOverlay.Max_Repeat_Time = MAX_REPEAT_TIME;
            panel_TxtOverlay.Start(pictureBox1.Width);

            timer_DelayText.Stop();
        }

        public void TxtOverlay_FitToContainer(int Height, int Width)
        {
            int h = this.Height;
            Utility.fitFormToContainer(this, this.Height, this.Width, Height, Width);
            _Text_Size = _Text_Size * ((float)Height / (float)h);
        }
    }
    public class Notify_TextRun_Finish : EventArgs
    {
        private bool _isFinished = false;
        public Notify_TextRun_Finish(bool isFinished)
        {
            _isFinished = isFinished;
        }
    }

}
