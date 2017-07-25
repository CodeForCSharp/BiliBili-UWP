using bilibili2.Class;
using bilibili2.Pages;
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
        private ObservableCollection<RecommendPartitionViewModel> Partitions { get; } = new ObservableCollection<RecommendPartitionViewModel>();
        public AVRecommendFragment()
        {
            this.InitializeComponent();
            SetHomeInfo();
        }

        public async void SetHomeInfo()
        {
            try
            {
                var url = $"http://app.bilibili.com/x/v2/show?_device=android&_ulv=10000&appkey={ApiHelper._appKey}&build=434000&platform=android";
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
                            Danmaku = token["danmaku"]?.Value<int>().ToString()??"-",
                            Play = token["play"]?.Value<int>().ToString()??"-",
                            Param = token["param"].Value<string>()
                        }).Take(4);
                    var bangumis = model["data"]
                        .First(token => token["type"].Value<string>() == "bangumi")["body"]
                        .Select(token => new RecommendBangumiViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            Index = $"第{token["index"].Value<string>()}话",
                            Param = token["param"].Value<string>(),
                            Mtime = Converter.TimeToSimplifyTime(token["mtime"].Value<string>())
                        });
                    var lives = model["data"]
                        .First(token => token["type"].Value<string>() == "live")["body"].
                        Select(token => new RecommendLiveViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            Area = token["area"].Value<string>(),
                            AreaId = token["area_id"].Value<int>().ToString(),
                            Name = token["name"].Value<string>(),
                            Online = token["online"].Value<int>(),
                            Param = token["param"].Value<string>()
                        });
                    var regions = model["data"]
                        .Where(token => token["type"].Value<string>() == "region")
                        .Select(token => new
                        {
                            Title = token["title"].Value<string>(),
                            Key = token["param"].Value<string>(),
                            Items = token["body"].Select(item => new AVItemViewModel
                            {
                                Cover = item["cover"].Value<string>(),
                                Title = item["title"].Value<string>(),
                                Danmaku = item["danmaku"]?.Value<int>().ToString()??"-",
                                Play = item["play"]?.Value<int>().ToString()??"-",
                                Param = item["param"].Value<string>()
                            })
                        });
                    var activities = model["data"]
                        .First(token => token["type"].Value<string>() == "activity")["body"]
                        .Select(token => new ActivityViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            Uri = token["uri"].Value<string>(),
                            Param = token["param"].Value<string>()
                        });
                    var topics = model["data"]
                        .Where(token => token["banner"]?["bottom"] != null)
                        .Select(token => token["banner"]["bottom"].Select(item => new RegionBannerViewModel
                        {
                            Image = item["image"].Value<string>(),
                            Title = item["title"].Value<string>(),
                            Uri = item["uri"].Value<string>(),
                            Id = item["id"].Value<int>()
                        }))
                        .Aggregate((a, b) => a.Concat(b));
                    PartitionsPanel.Header = new AVRecommendHeaderViewModel { Banners = banners.ToList() };
                    Partitions.Add(new RecommendPartitionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_header_hot.png",
                        Name = "热门推荐",
                        Param = "",
                        Type = "recommend",
                        Items = new ObservableCollection<object>(recommends)
                    });
                    Partitions.Add(new RecommendPartitionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_head_live.png",
                        Name = "正在直播",
                        Param = "",
                        Type = "live",
                        Items =new ObservableCollection<object>(lives)
                    });
                    Partitions.Add(new RecommendPartitionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_category_t13.png",
                        Name = "番剧推荐",
                        Param = "13",
                        Type = "bangumi",
                        Items=new ObservableCollection<object>(bangumis)
                    });
                    foreach(var region in regions)
                    {
                        Partitions.Add(new RecommendPartitionViewModel
                        {
                            Icon = $"ms-appx:///Assets/Icon/ic_category_t{region.Key}.png",
                            Name = region.Title,
                            Param = region.Key,
                            Type = "region",
                            Items = new ObservableCollection<object>(region.Items)
                        });
                    }
                    Partitions.Add(new RecommendPartitionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_header_topic.png",
                        Name = "话题",
                        Param = "",
                        Type = "topic",
                        Items = new ObservableCollection<object>(topics)
                    });
                    Partitions.Add(new RecommendPartitionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_header_activity_center.png",
                        Name = "活动中心",
                        Param = "",
                        Type = "activity",
                        Items =new ObservableCollection<object>(activities)
                    });
                }
                PrLoad.Visibility = Visibility.Visible;           
            }
            catch (Exception ex)
            {
                MessagePanel.Show(ex.Message,3000);
            }
            finally
            {
                PrLoad.Visibility = Visibility.Collapsed;
            }
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            switch (e.ClickedItem)
            {
                case AVItemViewModel av:
                    frame.Navigate(typeof(VideoInfoPage), av.Param);
                    break;
                case RecommendLiveViewModel live:
                    frame.Navigate(typeof(LiveInfoPage), live.Param);
                    break;
                case RegionBannerViewModel topic:
                    frame.Navigate(typeof(WebViewPage), topic.Uri);
                    break;
                case RecommendBangumiViewModel bangumi:
                    frame.Navigate(typeof(BanInfoPage), bangumi.Param);
                    break;
                case ActivityViewModel activity:
                    break;
            }
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
                case RecommendLiveViewModel live:
                    return LiveItemTemplate;
                case RegionBannerViewModel topic:
                    return TopicTemplate;
                case RecommendBangumiViewModel bangumi:
                    return BangumiItemTemplate;
                case ActivityViewModel activity:
                    return ActivityTemplate;
            }
            return null;
        }
    }
}
