using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Display
{
    public class DownloadFile
    {
        private string Url = "";
        private string PathLocation = "";
        private string ScheduleId = "";

        BackgroundWorker downloadBackgroundWorker;

        private int Percentage = -1;
        private long totalSizeBytes = 0;
        private long downloadedBytes = 0;

        private const int FILESIZE_NOT_PROPERTY = 1;
        private const int DOWNLOAD_CANCELLED = 2;
        private int CancelMessageId = 0;

        public DownloadFile(string Url, string PathLocation, string ScheduleId = "")
        {
            this.Url = Url;
            this.PathLocation = PathLocation;
            this.ScheduleId = ScheduleId;

            try
            {
                NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
            }
            catch { }
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable == false)
            {
                Log.Information("Internet lost connection!!");
            }
            else
            {
                Log.Information("Internet Reconnected!!");
            }
        }

        public void DownloadAsync()
        {
            //using background worker now!
            downloadBackgroundWorker = new BackgroundWorker();
            downloadBackgroundWorker.DoWork += (sender, e) => DownloadFile_DoWork(sender, e);
            downloadBackgroundWorker.ProgressChanged += DownloadFile_ProgressChanged;
            downloadBackgroundWorker.RunWorkerCompleted += DownloadBackgroundWorker_RunWorkerCompleted;
            downloadBackgroundWorker.WorkerReportsProgress = true;
            downloadBackgroundWorker.WorkerSupportsCancellation = true;
            downloadBackgroundWorker.RunWorkerAsync();
        }

        public void StopDownLoad()
        {
            try
            {
                if (downloadBackgroundWorker != null)
                {
                    downloadBackgroundWorker.CancelAsync();
                }
            }
            catch { }
        }

        private void DownloadFile_DoWork(object sender, DoWorkEventArgs e)
        {
            string downloadPath = PathLocation;

            string path = Url;

            long iFileSize = 0;
            int iBufferSize = 1024;
            iBufferSize *= 1000;
            long iExistLen = 0;
            FileStream saveFileStream;

            // Check if file exists. If true, then check amount of bytes
            if (File.Exists(downloadPath))
            {
                iExistLen = new FileInfo(downloadPath).Length;
            }
            if (iExistLen > 0)
                saveFileStream = new FileStream(downloadPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            else
                saveFileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

            totalSizeBytes = Size(path);

            HttpWebRequest hwRq;
            hwRq = (HttpWebRequest)HttpWebRequest.Create(path);
            if (iExistLen >= totalSizeBytes)
            {
                e.Cancel = true;
                CancelMessageId = 1;
                hwRq.Abort();
                
                saveFileStream.Close();
                return;
            }
            hwRq.AddRange((int)iExistLen);

            using (HttpWebResponse hwRes = (HttpWebResponse)hwRq.GetResponse())
            {
                using (Stream smRespStream = hwRes.GetResponseStream())
                {
                    iFileSize = hwRes.ContentLength;

                    int iByteSize;
                    byte[] downBuffer = new byte[iBufferSize];
                    Log.Information("Start Download {A} Bytes, Exist Size: {B} Bytes", totalSizeBytes.ToString("N0"), iExistLen.ToString("N0"));

                    try
                    {
                        while ((iByteSize = smRespStream.Read(downBuffer, 0, downBuffer.Length)) > 0)
                        {
                            if (downloadBackgroundWorker.CancellationPending)
                            {
                                e.Cancel = true;
                                CancelMessageId = 2;
                                hwRes.Close();
                                hwRes.Dispose();
                                hwRq.Abort();
                                saveFileStream.Close();
                                return;
                            }
                            saveFileStream.Write(downBuffer, 0, iByteSize);

                            downloadedBytes = new FileInfo(downloadPath).Length;

                            // Report progress
                            int percentage = Convert.ToInt32(100.0 / totalSizeBytes * downloadedBytes);

                            (sender as BackgroundWorker).ReportProgress(percentage, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Result = ex;
                        hwRes.Close();
                        hwRes.Dispose();
                        hwRq.Abort();
                        saveFileStream.Close();
                        return;
                    }
                }
            }
            saveFileStream.Close();
        }
        private void DownloadFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if ((e.ProgressPercentage % 10 == 0 && Percentage != e.ProgressPercentage) || (Percentage == -1))
            {
                Percentage = e.ProgressPercentage;
                Log.Information("Downloaded {A}%: {B} / {C} Bytes to: {D}", Percentage, downloadedBytes.ToString("N0"), totalSizeBytes.ToString("N0"), PathLocation);

                if (e.ProgressPercentage == 100)
                {
                    // Save to List
                    SavedFile_Type videoFile = new SavedFile_Type();
                    videoFile.PathLocation = PathLocation;
                    videoFile.ScheduleId = ScheduleId;
                    videoFile.Link = Url;

                    SaveFileDownloaded(videoFile);
                }
            }
        }

        private void DownloadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                if (CancelMessageId == 2)
                {
                    Log.Information("Download cancelled.");
                }
                else if (CancelMessageId == 1)
                {
                    if (File.Exists(PathLocation))
                    {
                        File.Delete(PathLocation);
                        Log.Information("File size not property. Delete then download again.");
                    }
                    Thread.Sleep(5000);
                    Percentage = -1;
                    DownloadAsync();
                }
                return;
            }
            if (e.Result != null)
            {
                Log.Error(e.Result.ToString());

                Thread.Sleep(5000);
                Percentage = -1;
                DownloadAsync();
                return;
            }
            if (e.Error != null)
            {
                Log.Error(e.Error.ToString());
                return;
            }
            Log.Information("Download is complete.");
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

        public long Size(string url)
        {
            WebRequest req = HttpWebRequest.Create(url);
            req.Method = "HEAD";
            WebResponse resp = req.GetResponse();
            resp.Close();
            return resp.ContentLength;
        }
    }
}
