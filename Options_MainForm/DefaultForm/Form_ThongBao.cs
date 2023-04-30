using Serilog;
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
    public partial class Form_ThongBao : CSWinFormLayeredWindow.PerPixelAlphaForm
    {
        private const int MAXVALUE = 1000 * 1000 * 1000;
        private int MaxPosition = 0;

        System.Timers.Timer Duration_ThongBao_Tmr;
        System.Timers.Timer Moving_Tmr;

        private bool _is_ThongBaoAvailable = false;
        public int _Priority_ThongBao = 1000;
        public string ScheduleID_ThongBao = "";
        public bool isValid = false;
        int speed = 0;

        private Point Default_Location = new Point();

        public int SetSpeed
        {
            get { return speed; }
            set { speed = value; Invalidate(); }
        }
        public Form_ThongBao()
        {
            InitializeComponent();

            Moving_Tmr = new System.Timers.Timer();
            Moving_Tmr.Interval = 10000;
            Moving_Tmr.Elapsed += Moving_Tmr_Elapsed;

            Duration_ThongBao_Tmr = new System.Timers.Timer();
        }

        private void Moving_Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Moving_Tmr.Interval = 30;
            if (this.Location.Y < -MaxPosition)
            {
                if (isValid == false)
                {
                    _is_ThongBaoAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_ThongBao = 1000;
                    Log.Information("BanTinThongBao Stop!");
                    this.BackColor = Color.Black;
                    //OnNotifyEndProcess_TextRun();

                    Moving_Tmr.Stop();
                    return;
                }
                this.Location = new Point(this.Location.X, (int)Screen.PrimaryScreen.Bounds.Size.Height);
            }
            this.Location = new Point(this.Location.X, this.Location.Y - speed);
        }

        public void SetLocation_ThongBao(Point TB_Location)
        {
            Default_Location = TB_Location;
        }

        public void ShowText(string Title, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            this.Location = Default_Location;
            Log.Information("ShowText: Tiêu đề: {A}", Title);
            if (txtThongBao.Text == Title) return;
            try
            {
                txtThongBao.Text = Title.Trim().ToUpper() + "\n";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ShowText: Tiêu đề: {A}", Title);
            }

            this.Visible = true;

            MaxPosition = txtThongBao.Height;
            Bitmap bmTitle = ConvertTextToImage(txtThongBao);

            this.SelectBitmap(bmTitle);

            if (MaxPosition < (int)Screen.PrimaryScreen.Bounds.Size.Height)
            {
                speed = 0;
            }
            else
            {
                Moving_Tmr.Stop();
                Moving_Tmr.Interval = 10000;
                Moving_Tmr.Start();
            }

            Duration_Handle(ref Duration_ThongBao_Tmr, Duration, () =>
            {
                // Stop Media
                isValid = false;
                if (speed == 0)
                {
                    _is_ThongBaoAvailable = false;
                    AutoHideScreen_Check();
                    _Priority_ThongBao = 1000;
                    Log.Information("BanTinThongBao Stop!");
                    //OnNotifyEndProcess_TextRun();

                    Moving_Tmr.Stop();
                    return;
                }
            });
            isValid = true;

            _is_ThongBaoAvailable = true;
            _Priority_ThongBao = Priority;
            ScheduleID_ThongBao = ScheduleId;
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
        public Bitmap ConvertTextToImage(Control control)
        {
            var bitmap = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

            return bitmap;
        }

        private void AutoHideScreen_Check()
        {
            if (_is_ThongBaoAvailable == false)
            {
                this.Visible = false;
            }
        }

        public void CloseForm()
        {
            txtThongBao.Text = "";
            panel_TextRun.Stop();

            Duration_ThongBao_Tmr?.Stop();
            Duration_ThongBao_Tmr?.Dispose();

            Moving_Tmr?.Stop();

            isValid = false;
            _is_ThongBaoAvailable = false;
            AutoHideScreen_Check();
            _Priority_ThongBao = 1000;

            this.Location = Default_Location;
        }

        public void PageText_FitToContainer(int Height, int Width)
        {
            Utility.fitFormToContainer(this, Height, this.Width, Height, Width);

            txtThongBao.MaximumSize = new Size(panel_TextRun.Width, 0);
        }
    }
}
