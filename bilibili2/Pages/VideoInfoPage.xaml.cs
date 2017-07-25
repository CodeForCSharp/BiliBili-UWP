using bilibili2.Class;
using bilibili2.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoInfoPage : Page,INotifyPropertyChanged
    {
        private WebClientClass wc = new WebClientClass();
        public ObservableCollection<VideoReplyViewModel> HotReplies { get; } = new ObservableCollection<VideoReplyViewModel>();
        private Frame frame = Window.Current.Content as Frame;
        public IncrementalLoadingCollection<VideoReplyViewModel> Replies { get; } = new IncrementalLoadingCollection<VideoReplyViewModel>();
        private string aid;
        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private VideoInfoViewModel info;

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoInfoViewModel Info
        {
            get
            {
                return info;
            }
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        public VideoInfoPage()
        {
            Replies.LoadDataTask = async () =>
            {
                var url = $"http://api.bilibili.com/x/v2/reply?_device=android&_ulv=10000&oid={aid}&appkey={ApiHelper._appKey}&build=411005&plat=2&platform=android&type=1&pn={Replies.CurrentPage}&ps=20&nohot=1&sort=0";
                url += $"&sign={ApiHelper.GetSign(url)}";
                var results = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(results);
                if (model["code"].Value<int>() == 0)
                {
                    Replies.MaxPage = model["data"]["page"]["count"].Value<int>();
                    var vms = model["data"]["replies"].Select(token => new VideoReplyViewModel
                    {
                        Content = token["content"]["message"].Value<string>(),
                        Ctime = Converter.TickToDate(token["ctime"].Value<long>()),
                        Face = token["member"]["avatar"].Value<string>(),
                        Floor = $"#{token["floor"].Value<int>()}",
                        Level = $"ms-appx:///Assets/Icon/ic_lv{token["member"]["level_info"]["current_level"].Value<int>()}_large.png",
                        Like = token["like"].Value<int>(),
                        Mid = token["member"]["mid"].Value<string>(),
                        Name = token["member"]["uname"].Value<string>(),
                        Oid = token["oid"].Value<int>(),
                        Rcount = $"总共有{token["rcount"].Value<int>()}条回复",
                        Rpid = token["rpid"].Value<int>(),
                        Replies = token["replies"].Select(item=> new VideoReplyViewModel
                        {
                            Content = item["content"]["message"].Value<string>(),
                            Ctime = Converter.TickToDate(item["ctime"].Value<long>()),
                            Name = item["member"]["uname"].Value<string>(),
                        }).ToList()
                    });
                    return Tuple.Create(vms,Replies.CurrentPage<=Replies.MaxPage);
                }
                throw new Exception("Invaild Parameter");
            };
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = info.Title;
            request.Data.Properties.Description = info.Desc + "\r\n——分享自BiliBili UWP";
            request.Data.SetWebLink(new Uri($"http://www.bilibili.com/video/av{aid}"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if(e.Parameter is string aidString)
                {
                    aid = aidString;
                    HeaderTxt.Text = $"AV{aidString}";
                    GetVideoInfo(aidString);
                    GetReplyHotAsync(aidString);
                    Replies.Reset();
                    VideoInfoPanel.SelectedIndex = 0;
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }      
        }

        private async void GetReplyHotAsync(string aid)
        {
            var url = $"http://api.bilibili.com/x/v2/reply?_device=android&_ulv=10000&oid={aid}&appkey={ApiHelper._appKey}&build=411005&plat=2&platform=android&type=1&pn=1&ps=10&sort=2&nohot=1";
            url += $"&sign={ApiHelper.GetSign(url)}";
            var results = await wc.GetResults(new Uri(url));
            var model = JObject.Parse(results);
            if (model["code"].Value<int>()==0)
            {
                var vms = model["data"]["replies"].Select(token => new VideoReplyViewModel
                {
                    Content = token["content"]["message"].Value<string>(),
                    Ctime = Converter.TickToDate(token["ctime"].Value<long>()),
                    Face = token["member"]["avatar"].Value<string>(),
                    Floor = $"#{token["floor"].Value<int>()}",
                    Level = $"ms-appx:///Assets/Icon/ic_lv{token["member"]["level_info"]["current_level"].Value<int>()}_large.png",
                    Like = token["like"].Value<int>(),
                    Mid = token["member"]["mid"].Value<string>(),
                    Name = token["member"]["uname"].Value<string>(),
                    Oid = token["oid"].Value<int>(),
                    Rcount = $"总共有{token["rcount"].Value<int>()}条回复",
                    Rpid = token["rpid"].Value<int>()
                });
                HotReplies.Clear();
                foreach(var vm in vms)
                {
                    HotReplies.Add(vm);
                }
            }
        }
        /// <summary>
        /// 读取视频信息
        /// </summary>
        /// <param name="aid"></param>
        private async void GetVideoInfo(string aid)
        {
            try
            {
                var url = $"https://app.bilibili.com/x/v2/view?_device=android&_ulv=10000&aid={aid}&appkey={ApiHelper._appKey}&build=411005&platform=android&plat=0&ts={ApiHelper.GetTimeSpen}";
                url += $"&sign={ApiHelper.GetSign(url)}";
                var results = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(results);
                if(model["code"].Value<int>()==0)
                {
                    var vm = new VideoInfoViewModel
                    {
                        Coin = Converter.SimplifyTimes(model["data"]["stat"]["coin"].Value<int>()),
                        Danmaku = Converter.SimplifyTimes(model["data"]["stat"]["danmaku"].Value<int>()),
                        Share = Converter.SimplifyTimes(model["data"]["stat"]["share"].Value<int>()),
                        Reply = Converter.SimplifyTimes(model["data"]["stat"]["reply"].Value<int>()),
                        Favorite = Converter.SimplifyTimes(model["data"]["stat"]["favorite"].Value<int>()),
                        View = Converter.SimplifyTimes(model["data"]["stat"]["view"].Value<int>()),
                        Title = model["data"]["title"].Value<string>(),
                        Desc = model["data"]["desc"].Value<string>(),
                        Face = model["data"]["owner"]["face"].Value<string>(),
                        Mid = model["data"]["owner"]["mid"].Value<int>(),
                        Name = model["data"]["owner"]["name"].Value<string>(),
                        Pic = model["data"]["pic"].Value<string>(),
                        PubDate = Converter.TickToDate(model["data"]["pubdate"].Value<long>()),
                        Tags = model["data"]["tag"]?.Select(item => new VideoTagViewModel
                        {
                            Hates = item["hates"].Value<int>(),
                            Likes = item["likes"].Value<int>(),
                            TagId = item["tag_id"].Value<int>(),
                            TagName = item["tag_name"].Value<string>()
                        }).ToList()??new List<VideoTagViewModel>(),
                        Relates = model["data"]["relates"]?.Select(item => new RegionItemViewModel
                        {
                            Cover = item["pic"].Value<string>(),
                            Title = item["title"].Value<string>(),
                            Param = item["aid"].Value<int>().ToString(),
                            Danmaku = item["stat"]["danmaku"]?.Value<string>() ?? "-",
                            Name = item["owner"]["name"].Value<string>(),
                            Play = item["stat"]["view"]?.Value<string>() ?? "-"
                        }).ToList() ?? new List<RegionItemViewModel>(),
                        Pages = model["data"]["pages"].Select(item =>new VideoPageViewModel
                        {
                            Cid = item["cid"].Value<int>(),
                            Page = item["page"].Value<int>(),
                            Part = item["part"].Value<string>() != "" ? item["part"].Value<string>() : model["data"]["title"].Value<string>(),
                            Aid = model["data"]["aid"].Value<int>()
                        }).ToList()
                    };
                    Info = vm;
                }
            }
            catch (Exception ex)
            {
                MessageShow.Show("读取视频信息\r\n"+ex.Message,3000);
            }
        }

        private void PartPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is VideoPageViewModel model)
            {
                frame.Navigate(typeof(PlayerPage), new KeyValuePair<List<VideoPageViewModel>,int>(Info.Pages, model.Page));
            }
        }

        private void PayCoinButton_Click(object sender, RoutedEventArgs e)
        {
            PayCoinPanel.IsOpen = true;
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            SharePanel.IsOpen = true;
        }

        private void ShareApps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void ShareLink_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void BrowserOpen_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadPanel.IsOpen = true;
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if(PagesSelectionPanel.SelectedItems.Count == Info.Pages.Count)
            {
                PagesSelectionPanel.SelectedIndex = -1;
                SelectAllButton.Content = "选择全部";
            }
            else
            {
                PagesSelectionPanel.SelectAll();
                SelectAllButton.Content = "取消全选";
            }
        }

        private void BeginDownloadButton_Click(object sender, RoutedEventArgs e)
        {

        }
    } 
}
