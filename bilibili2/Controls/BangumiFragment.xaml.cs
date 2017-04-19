using bilibili2.Pages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2.Controls
{
    public sealed partial class BangumiFragment : UserControl
    {
        ObservableCollection<BangumiRecommendViewModel> Recommends = new ObservableCollection<BangumiRecommendViewModel>();
        Frame frame = Window.Current.Content as Frame;
        public BangumiFragment()
        {
            this.InitializeComponent();
            SetRecommendAsync();
        }
        WebClientClass wc = new WebClientClass();
        public async void SetRecommendAsync()
        {
            var url = $"https://bangumi.bilibili.com/appindex/follow_index_page?appkey={ApiHelper._appKey}&build=411005&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpen}000";
            url += $"&sign={ApiHelper.GetSign(url)}";
            var result = await wc.GetResults(new Uri(url));
            var model = JObject.Parse(result);
            if(model["code"].Value<int>()==0)
            {
                var recommends = new List<BangumiRecommendViewModel>
                {
                    new BangumiRecommendViewModel
                    {
                          Icon="ms-appx:///Assets/Icon/bangumi_follow_ic_recommend.png",
                          Title ="番剧推荐",
                          More = "更多番剧",
                          BannerCover =model["result"]["recommend_jp"]["foot"].First["cover"].Value<string>(),
                          BannerTitle =model["result"]["recommend_jp"]["foot"].First["title"].Value<string>(),
                          BannerDesc = model["result"]["recommend_jp"]["foot"].First["desc"]?.Value<string>()??"",
                          IsNew = model["result"]["recommend_jp"]["foot"].First["is_new"].Value<int>()==1?true:false,
                          Link =model["result"]["recommend_jp"]["foot"].First["link"].Value<string>(),
                           Items = model["result"]["recommend_jp"]["recommend"].Select(item=>new BangumiItemViewModel
                           {
                               Cover =item["cover"].Value<string>(),
                               Favourites =item["favourites"].Value<string>(),
                               SeasonId =item["season_id"].Value<int>(),
                               Status =$"更新至第{item["newest_ep_index"].Value<string>()}话",
                               Title = item["title"].Value<string>()
                           }).ToList()
                    },
                    new BangumiRecommendViewModel
                    {
                          Icon="ms-appx:///Assets/Icon/bangumi_follow_ic_domestic_recommend.png",
                          Title ="国漫推荐",
                          More = "更多国漫",
                          BannerCover =model["result"]["recommend_cn"]["foot"].First["cover"].Value<string>(),
                          BannerTitle =model["result"]["recommend_cn"]["foot"].First["title"].Value<string>(),
                          BannerDesc = model["result"]["recommend_cn"]["foot"].First["desc"]?.Value<string>(),
                          IsNew = model["result"]["recommend_cn"]["foot"].First["is_new"]==null?true:false,
                          Link =model["result"]["recommend_cn"]["foot"].First["link"].Value<string>(),
                           Items = model["result"]["recommend_cn"]["recommend"].Select(item=>new BangumiItemViewModel
                           {
                               Cover =item["cover"].Value<string>(),
                               Favourites =item["favourites"].Value<string>(),
                               SeasonId =item["season_id"].Value<int>(),
                               Status =$"更新至第{item["newest_ep_index"].Value<string>()}话",
                               Title = item["title"].Value<string>()
                           }).ToList()
                    }
                };
                recommends.ForEach(item =>Recommends.Add(item));
            }
        }

        private void IndexHyperlink_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(BanTagPage));
        }

        private void TimelineHyperlink_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(BanTimelinePage));
        }
    }
}
