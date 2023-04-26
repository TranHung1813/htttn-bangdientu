﻿using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public partial class Form_HinhAnh : CSWinFormLayeredWindow.PerPixelAlphaForm
    {
        private string PathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Files");

        System.Timers.Timer Duration_HinhAnh_Tmr;

        private string _ImageName = "";
        public bool _is_ImageAvailable = false;
        public string _ScheduleID_Image = "";
        public int _Priority_Image = 1000;

        private const int MAXVALUE = 1000 * 1000 * 1000;

        public Form_HinhAnh()
        {
            InitializeComponent();

            Duration_HinhAnh_Tmr = new System.Timers.Timer();
        }

        public void ShowImage(string Url, string ScheduleId, int Priority = 0, int Duration = MAXVALUE)
        {
            if (_Priority_Image < Priority) return;
            Log.Information("ShowImage: {A}", Url);
            _ScheduleID_Image = ScheduleId;
            _Priority_Image = Priority;

            // Duration Handle
            Duration_Handle(Duration_HinhAnh_Tmr, ref Duration_HinhAnh_Tmr, Duration, () =>
            {
                // Stop Media
                _is_ImageAvailable = false;
                this.Visible = false;
                _Priority_Image = 1000;
                Log.Information("Image Stop!");
            });

            _is_ImageAvailable = true;

            List<DataUser_SavedFiles> SavedFiles = SqLiteDataAccess.Load_SavedFiles_Info();

            if (SavedFiles != null)
            {
                // Kiem tra xem File da download chua, neu roi thi Play
                int index = SavedFiles.FindIndex(s => (s.ScheduleId == ScheduleId) && (s.Link == Url));
                if (index != -1)
                {
                    if (File.Exists(SavedFiles[index].PathLocation))
                    {
                        try
                        {
                            Task tsk = Task.Run(() =>
                            {
                                Bitmap bm = new Bitmap(SavedFiles[index].PathLocation);
                                Bitmap clone = new Bitmap(bm, new Size(this.Width, this.Height));
                                clone = new Bitmap(clone.Width, clone.Height, PixelFormat.Format32bppArgb);

                                using (Graphics gr = Graphics.FromImage(clone))
                                {
                                    gr.DrawImage(bm, new Rectangle(0, 0, clone.Width, clone.Height));
                                }
                                this.SelectBitmap(clone);

                                bm.Dispose();
                                clone.Dispose();
                            });

                            OnNotifyStartProcess();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message, "LoadImage");
                        }
                    }
                    else
                    {
                        LoadImage_Async(Url, ScheduleId);
                    }
                }
                else
                {
                    // Neu chua download thi play link nhu binh thuong
                    LoadImage_Async(Url, ScheduleId);
                }
            }
            else
            {
                // Neu chua download thi play link nhu binh thuong
                LoadImage_Async(Url, ScheduleId);
            }
        }
        private void Duration_Handle(System.Timers.Timer tmr, ref System.Timers.Timer return_tmr, int Duration, Action action)
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
                tmr.Stop();
                tmr.Dispose();
            };
            tmr.Start();

            return_tmr = tmr;
        }
        private void LoadImage_Async(string Url, string ScheduleId)
        {
            string fileExtension = "";
            Uri uri = new Uri(Url);
            try
            {
                fileExtension = Path.GetExtension(uri.LocalPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Image_GetExtension: {Url}", Url);
            }
            _ImageName = Path.Combine(PathFile, "SaveImage--" + ScheduleId + fileExtension);

            WebClient webClient = new WebClient();
            webClient.DownloadFileAsync(uri, _ImageName);
            webClient.DownloadFileCompleted += (sender, e) =>
            {
                Log.Information("DownloadImageCompleted: {A}", Url);
                try
                {
                    Task tsk = Task.Run(() =>
                    {
                        Bitmap bm = new Bitmap(_ImageName);
                        Bitmap clone = new Bitmap(bm, new Size(this.Width, this.Height)); //resize
                        clone = new Bitmap(clone.Width, clone.Height, PixelFormat.Format32bppArgb); // convert to Format32bppArgb

                        using (Graphics gr = Graphics.FromImage(clone))
                        {
                            gr.DrawImage(bm, new Rectangle(0, 0, clone.Width, clone.Height));
                        }
                        this.SelectBitmap(clone);

                        bm.Dispose();
                        clone.Dispose();

                        if (File.Exists(_ImageName))
                        {
                            File.Delete(_ImageName);
                            _ImageName = "";
                        }
                    });

                    OnNotifyStartProcess();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, "DownloadAsync_Image_Completed");
                }
            };
        }

        public void SetLocation_HinhAnh(Point HA_Location)
        {
            this.Location = HA_Location;
        }

        public void CloseForm()
        {
            this.Visible = false;
            //panel_TextRun.Stop();

            if (Duration_HinhAnh_Tmr != null)
            {
                Duration_HinhAnh_Tmr.Stop();
                Duration_HinhAnh_Tmr.Dispose();
            }

            _is_ImageAvailable = false;
            _Priority_Image = 1000;
        }

        private void SaveFileDownloaded(SavedFile_Type file)
        {
            List<DataUser_SavedFiles> SavedFiles = SqLiteDataAccess.Load_SavedFiles_Info();
            DataUser_SavedFiles info_Save = new DataUser_SavedFiles();
            if (SavedFiles != null)
            {
                int index = SavedFiles.FindIndex(s => s.ScheduleId == file.ScheduleId);
                if (index == -1)
                {
                    info_Save.Id = SavedFiles.Count + 1;
                }
                else
                {
                    info_Save.Id = index + 1;
                }
            }
            else
            {
                info_Save.Id = 1;
            }
            info_Save.ScheduleId = file.ScheduleId;
            info_Save.PathLocation = file.PathLocation;
            info_Save.Link = file.Link;

            SqLiteDataAccess.SaveInfo_SavedFiles(info_Save);
        }

        public void PageImage_FitToContainer(int Height, int Width)
        {
            Utility.fitFormToContainer(this, Height, this.Width, Height, Width);
        }

        private event EventHandler<NotifyStartProcess> _NotifyStartProcess;
        public event EventHandler<NotifyStartProcess> NotifyStartProcess
        {
            add
            {
                _NotifyStartProcess += value;
            }
            remove
            {
                _NotifyStartProcess -= value;
            }
        }
        protected virtual void OnNotifyStartProcess()
        {
            if (_NotifyStartProcess != null)
            {
                _NotifyStartProcess(this, new NotifyStartProcess());
            }
        }
    }
}
