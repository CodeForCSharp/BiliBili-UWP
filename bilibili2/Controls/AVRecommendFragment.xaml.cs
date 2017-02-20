using bilibili2.Class;
using bilibili2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2
{
    public sealed partial class AVRecommendFragment : UserControl
    {
        private WebClientClass wc = new WebClientClass();
        private ObservableCollection<BannerViewModel> Banners { get; } = new ObservableCollection<BannerViewModel>();
        private List<RecommendPartitionViewModel> Partitions { get; } = new List<RecommendPartitionViewModel>
                {
                    new RecommendPartitionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_header_hot.png",
                        Name ="热门推荐",
                        Param ="",
                        Type = "recommend"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_head_live.png",
                        Name ="正在直播",
                        Param="",
                        Type="live"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_category_t13.png",
                        Name ="番剧推荐",
                        Param="13",
                        Type="bangumi"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t1.png",
                        Name="动画区",
                        Param="1",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t3.png",
                        Name="音乐区",
                        Param="3",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t129.png",
                        Name="舞蹈区",
                        Param="129",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t4.png",
                        Name="游戏区",
                        Param="4",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t119.png",
                        Name="鬼畜区",
                        Param="119",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t160.png",
                        Name="生活区",
                        Param="160",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t36.png",
                        Name="科技区",
                        Param="36",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t155.png",
                        Name="时尚区",
                        Param="155",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t165.png",
                        Name="广告区",
                        Param="165",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t5.png",
                        Name="娱乐区",
                        Param="5",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t11.png",
                        Name="电视剧区",
                        Param="11",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_category_t23.png",
                        Name="电影区",
                        Param="23",
                        Type="region"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_header_topic.png",
                        Name="话题",
                        Param="",
                        Type="topic"
                    },
                    new RecommendPartitionViewModel
                    {
                        Icon ="ms-appx:///Assets/Icon/ic_header_activity_center.png",
                        Name="活动中心",
                        Param="",
                        Type="activity"
                    },
                };
        public AVRecommendFragment()
        {
            this.InitializeComponent();
            //http://app.bilibili.com/x/banner?plat=4&build=412001
        }

        public async void SetHomeInfo()
        {
            try
            {
                var url = $"http://app.bilibili.com/x/v2/show?_device=android&_ulv=10000&appkey={appkey}&build=434000&platform=android";
                url += $"&sign={ApiHelper.GetSign(url)}";
                var result = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(result);
                if(model["code"].Value<int>()==0)
                {
                    var banners = model["data"]
                        .First(token => token["banner"]["top"] != null)["banner"]["top"]
                        .Select(token => new BannerViewModel
                        {
                            Image = token["image"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            Uri = token["uri"].Value<string>(),
                            IsAd = token["is_ad"].Value<bool>()
                        });
                    var recommends = model["data"]
                        .First(token => token["type"].Value<string>() == "recommend")["body"]
                        .Select(token => new AVItemViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            Danmaku = token["danmaku"].Value<int>().ToString(),
                            Play = token["play"].Value<int>().ToString(),
                            Param = token["param"].Value<string>()
                        });
                    var bangumis = model["data"]
                        .First(token => token["type"].Value<string>() == "bangumi")["body"]
                        .Select(token => new BangumiItemViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            Index = token["index"].Value<string>(),
                            Param = token["param"].Value<string>(),
                            Mtime = Converter.TimeToSimplifyTime(token["mtime"].Value<string>())
                        });
                    var lives = model["data"]
                        .First(token => token["type"].Value<string>() == "live")["body"].
                        Select(token => new RecommendLiveViewModel
                        {
                            Cover=token["cover"].Value<string>()
                        });
                    var regions = model["data"]
                        .Where(token => token["type"].Value<string>() == "region")
                        .Select(token => token["body"].Select(item =>new AVItemViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            Danmaku = token["danmaku"].Value<int>().ToString(),
                            Play = token["play"].Value<int>().ToString(),
                            Param = token["param"].Value<string>()
                        }));
                }
                PrLoad.Visibility = Visibility.Visible;           
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
            finally
            {
                PrLoad.Visibility = Visibility.Collapsed;
            }
        }

        private void items_listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(VideoInfoPage));
        }
        private void home_GridView_FJ_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlayEvent((e.ClickedItem as AVItemViewModel).Aid);
        }
    }

    public class RecommendItemSelector : DataTemplateSelector
    {
        public DataTemplate AVItemTemplate { get; set; }
        public DataTemplate BangumiItemTemplate { get; set; }
        public DataTemplate LiveItemTemplate { get; set; }
        public DataTemplate TopicTemplate { get; set; }
        public DataTemplate ActivityTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch(item)
            {
                case AVItemViewModel av:
                    return AVItemTemplate;
                case LiveItemViewModel live:
                    return LiveItemTemplate;
                case BanTopicViewModel topic:
                    return TopicTemplate;
                case BangumiItemViewModel bangumi:
                    return BangumiItemTemplate;
                case ActivityViewModel activity:
                    return ActivityTemplate;
            }
            return null;
        }
    }
}
