﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Popups;

namespace bilibili2.Class
{
    /**
        下载文件夹管理示例
        -视频库
        ----Bili-Down文件夹
        --------视频、番剧标题文件夹
        ------------视频信息.Json
        ------------集数文件夹
        ----------------视频.mp4
        ----------------弹幕.XML
        ----------------配置文件.Json
        **/
   public class DownloadManage:IDisposable
    {
        ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        WebClientClass wc;

        public static List<string> Downloaded = new List<string>();//保存已经下载过的CID数据
        public void Dispose()
        {
            if (wc != null)
            {
                wc = null;
            }
        }

        public string ReplaceSymbol(string input)
        {
            string reg = @"\:" + @"|\;" + @"|\/" + @"|\\" + @"|\|" + @"|\," + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";//特殊字符
            Regex r = new Regex(reg);
            string strFiltered = r.Replace(input, "_");//将特殊字符替换为"_"
            return strFiltered;

        }

        public async void DownDanMu(string cid, StorageFolder folder)
        {
            try
            {
                wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://comment.bilibili.com/" + cid + ".xml"));
                //将弹幕存在在应用文件夹
                //StorageFolder folder = ApplicationData.Current.LocalFolder;
                //StorageFolder DowFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("Bili-Download", CreationCollisionOption.OpenIfExists);
                StorageFile fileWrite = await folder.CreateFileAsync(cid + ".xml", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(fileWrite, results);
            }
            catch (Exception)
            {
                //return null;
            }

        }

        public async Task<string> GetVideoUri(string cid, int quality)
        {
            //http://interface.bilibili.com/playurl?platform=android&cid=5883400&quality=2&otype=json&appkey=84956560bc028eb7&type=mp4
            try
            {
                wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://interface.bilibili.com/playurl?platform=android&cid=" + cid + "&quality=" + quality + "&otype=json&appkey=84956560bc028eb7&type=mp4"));
                VideoUriModel model = JsonConvert.DeserializeObject<VideoUriModel>(results);
                List<VideoUriModel> model1 = JsonConvert.DeserializeObject<List<VideoUriModel>>(model.durl.ToString());
                return model1[0].url;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private readonly SettingHelper _setting = new SettingHelper();
        public async void StartDownload(DownModel downModel)
        {
            try
            {
                var downloader = new BackgroundDownloader()
                {
                    TransferGroup = DownModel.group
                };
                if (_setting.SettingContains("UseWifi"))
                {
                    if ((bool)_setting.GetSettingValue("UseWifi"))
                    {
                        downloader.CostPolicy = BackgroundTransferCostPolicy.Always;
                    }
                    else
                    {
                        downloader.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
                    }
                }
                else
                {
                    downloader.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
                    _setting.SetSettingValue("UseWifi",false);
                }
                StorageFolder DowFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("Bili-Down", CreationCollisionOption.OpenIfExists);
                StorageFolder VideoFolder = await DowFolder.CreateFolderAsync(ReplaceSymbol(downModel.title), CreationCollisionOption.OpenIfExists);
                StorageFolder PartFolder = await VideoFolder.CreateFolderAsync(downModel.part, CreationCollisionOption.OpenIfExists);
                StorageFile file = await PartFolder.CreateFileAsync(downModel.mid + ".mp4", CreationCollisionOption.OpenIfExists);
                DownloadOperation downloadOp = downloader.CreateDownload(new Uri(downModel.url), file);
                downloadOp.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
                BackgroundTransferStatus downloadStatus = downloadOp.Progress.Status;
                downModel.Guid = downloadOp.Guid.ToString();
                downModel.path = downloadOp.ResultFile.Path;
                string jsonInfo = JsonConvert.SerializeObject(downModel);

                StorageFile fileWrite = await PartFolder.CreateFileAsync(downModel.Guid + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(fileWrite, jsonInfo);

                StorageFile fileWrite2 = await DowFolder.CreateFileAsync(downModel.Guid + ".bili", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(fileWrite2, WebUtility.UrlEncode(PartFolder.Path));
                DownDanMu(downModel.mid, PartFolder);
                await downloadOp.StartAsync();
            }
            catch (Exception ex)
            {
                //WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }
        }

        public async void GetDownOk()
        {
            try
            {
                DownloadManage.Downloaded.Clear();
                await Task.Run(async () =>
                {
                    StorageFolder DownFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("Bili-Down", CreationCollisionOption.OpenIfExists);
                    //List<DownloadManage.FolderModel> list = new List<DownloadManage.FolderModel>();
                    foreach (var item in await DownFolder.GetFoldersAsync())
                    {
                        //DownloadManage.FolderModel model = new DownloadManage.FolderModel()
                        //{
                        //    title = item.Name,
                        //    count = 0,
                        //    downedCount = 0,
                        //};
                        //List<DownloadManage.DownModel> list_file = new List<DownloadManage.DownModel>();
                        foreach (var item1 in await item.GetFoldersAsync())
                        {
                            foreach (var item2 in await item1.GetFilesAsync())
                            {
                                if (item2.FileType == ".json")
                                {
                                    StorageFile files = item2;
                                    string json = await FileIO.ReadTextAsync(item2);
                                    DownModel model123 = JsonConvert.DeserializeObject<DownModel>(json);
                                    if (model123.downloaded == true)
                                    {
                                        ///list_file.Add(model123);
                                        //model.downedCount++;
                                        Downloaded.Add(model123.mid);
                                    }
                                    //model.aid = model123.aid;
                                }
                            }
                            //model.count++;
                        }
                        //model.path = item.Path;
                        //model.downModel = list_file;
                        //list.Add(model);
                    }
                });
            }
            catch (Exception)
            {
            }

        }



        public class DownModel
        {
            public static StorageFolder DownFlie = null;//下载文件夹
            public static BackgroundTransferGroup group = BackgroundTransferGroup.CreateGroup("BILIBILI-UWP-20");//下载组，方便管理
            public string aid { get; set; }
            public string mid { get; set; }
            public string part { get; set; }//第几P
            public string path { get; set; }
            public bool isBangumi { get; set; }
            public string danmuPath { get; set; }
            public string danmuUrl { get; set; }
            public bool downloaded { get; set; }
            public int quality { get; set; }
            public string title { get; set; }
            public string Guid { get; set; }
            public string url { get; set; }
            public string partTitle { get; set; }
        }

        public class HandleModel : INotifyPropertyChanged
        {
            public DownModel downModel { get; set; }
            public CancellationTokenSource cts = new CancellationTokenSource();
            public event PropertyChangedEventHandler PropertyChanged;
            protected void ThisPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public DownloadOperation DownOp { get; set; }

            private double _progress;
            public double Progress
            {
                get => _progress;
                set
                {
                    _progress = value;
                    ThisPropertyChanged("Progress");
                }
            }

            private string _size;
            public string Size
            {
                get => _size;
                set
                {
                    _size = (Convert.ToDouble(value) / 1024 / 1024).ToString("0.0") + "M/" + ((double)DownOp.Progress.TotalBytesToReceive / 1024 / 1024).ToString("0.0") + "M";
                    ThisPropertyChanged("Size");
                }
            }
            public string Guid => DownOp.Guid.ToString();
            private string _status;
            public string Status
            {
                get { ThisPropertyChanged("Status"); return _status; }
                set
                {
                    switch (DownOp.Progress.Status)
                    {
                        case BackgroundTransferStatus.Idle:
                            _status = "空闲中";
                            break;
                        case BackgroundTransferStatus.Running:
                            _status = "下载中";
                            break;
                        case BackgroundTransferStatus.PausedByApplication:
                            _status = "暂停中";
                            break;
                        case BackgroundTransferStatus.PausedCostedNetwork:
                            _status = "因网络暂停";
                            break;
                        case BackgroundTransferStatus.PausedNoNetwork:
                            _status = "没有连接至网络";
                            break;
                        case BackgroundTransferStatus.Completed:
                            _status = "完成";
                            break;
                        case BackgroundTransferStatus.Canceled:
                            _status = "取消";
                            break;
                        case BackgroundTransferStatus.Error:
                            _status = "下载错误";
                            break;
                        case BackgroundTransferStatus.PausedSystemPolicy:
                            _status = "因系统问题暂停";
                            break;
                        default:
                            _status = "Wait...";
                            break;
                    }
                    ThisPropertyChanged("status");
                }
            }
        }

        public class FolderModel
        {
            public string aid { get; set; }
            public string sid { get; set; }
            public string title { get; set; }
            public string path { get; set; }
            public int count { get; set; }
            public List<DownModel> downModel { get; set; }
            public int downedCount { get; set; }
            public bool IsBangumi { get; set; }
        }

    }

   
    
}
