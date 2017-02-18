using bilibili2.Model;
using bilibili2.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2.Controls
{
    public sealed partial class LiveHomeFragment : UserControl
    {
        public LiveHomeFragment()
        {
            this.InitializeComponent();
            GetLiveInfo();
        }
        WebClientClass wc = new WebClientClass();
        public delegate void PlayHandler(string aid);
        public event PlayHandler PlayEvent;
        public event PlayHandler ErrorEvent;
        private List<LiveNavigationViewModel> navigations = new List<LiveNavigationViewModel>
        {
            new LiveNavigationViewModel{ Icon ="ms-appx:///Assets/Icon/live_home_follow_anchor.png",Name="关注",Index=0 },
            new LiveNavigationViewModel{Icon="ms-appx:///Assets/Icon/live_home_live_center.png",Name="中心",Index=1 },
            new LiveNavigationViewModel{Icon ="ms-appx:///Assets/Icon/live_home_clip_video.png",Name="小视频",Index=2 },
            new LiveNavigationViewModel{ Icon="ms-appx:///Assets/Icon/live_home_search_room.png",Name="搜索",Index=3},
            new LiveNavigationViewModel{ Icon="ms-appx:///Assets/Icon/live_home_all_category.png",Name="分类",Index=4 }
        };
        public async void GetLiveInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                //&access_key={0}
                string url = $"http://live.bilibili.com/AppIndex/home?_device=android&_ulv=10000&appkey={ApiHelper._appKey}&build=434000&platform=android&scale=xxhdpi";
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(results);
                if (model["code"].Value<int>() == 0)
                {
                    var banners = model["data"]["banner"].Select(item => new LiveBannerViewModel
                    {
                        Img = item["img"].Value<string>(),
                        Title = item["title"].Value<string>(),
                        Link = item["link"].Value<string>(),
                        Remark = item["remark"].Value<string>()
                    }).ToList();
                    var partitions = model["data"]["partitions"].Select(item => new LivePartitionViewModel
                    {
                        Icon = item["partition"]["sub_icon"]["src"].Value<string>(),
                        Name = item["partition"]["name"].Value<string>(),
                        Id = item["partition"]["id"].Value<int>(),
                        Lives = item["lives"].Select(token => new LiveItemViewModel
                        {
                            Face = token["owner"]["face"].Value<string>(),
                            Mid = token["owner"]["mid"].Value<int>(),
                            Name = token["owner"]["name"].Value<string>(),
                            Online = token["online"].Value<int>(),
                            RoomId = token["room_id"].Value<string>(),
                            Src = token["cover"]["src"].Value<string>(),
                            Title = token["title"].Value<string>()
                        }).ToList()
                    }).ToList();
                    HomeLivePanel.Header = new LiveHomeHeaderViewModel { Banners = banners, Navigations = navigations };
                    HomeLivePanel.ItemsSource = partitions;
                }
                else
                {
                    ErrorEvent("读取直播失败" + model["message"]);
                }
            }
            catch (Exception ex)
            {
                ErrorEvent("读取直播失败" + ex.Message);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void gridview_Hot_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlayEvent((e.ClickedItem as LiveItemViewModel).RoomId);
        }

        private void ButtonPanel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BannerPanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            if(e.OriginalSource is LiveBannerViewModel model)
            {
                frame.Navigate(typeof(WebViewPage),model.Link);
            }
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            if(e.ClickedItem is LiveNavigationViewModel model)
            {
                switch(model.Index)
                {
                    case 3: frame.Navigate(typeof(SearchLivePage)); break;
                    case 4:frame.Navigate(typeof(AllLivePage));break;
                }
            }
        }
    }
}
