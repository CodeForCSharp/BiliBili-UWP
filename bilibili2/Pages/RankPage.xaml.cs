using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.Web.Http;
using Windows.UI.Core;
using bilibili2.Class;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RankPage : Page
    {
        WebClientClass wc = new WebClientClass();
        List<RegionRankViewModel> Regions { get; }
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public RankPage()
        {
            Regions = new List<RegionRankViewModel>
            {
                new RegionRankViewModel{Tid = 0,Name="原创" },
                new RegionRankViewModel{ Tid = 0,Name="全站"},
                new RegionRankViewModel{ Tid = 0,Name="B番"},
                new RegionRankViewModel{ Tid = 13 , Name = "番剧"},
                new RegionRankViewModel{Tid = 1,Name="动画"},
                new RegionRankViewModel{ Tid = 3 , Name="音乐"},
                new RegionRankViewModel{ Tid = 129,Name="舞蹈"},
                new RegionRankViewModel{Tid = 4,Name="游戏" },
                new RegionRankViewModel{Tid= 36 ,Name="科技" },
                new RegionRankViewModel{ Tid = 160 ,Name="生活"},
                new RegionRankViewModel{Tid = 119,Name="鬼畜" },
                new RegionRankViewModel{ Tid = 155 ,Name = "时尚"},
                new RegionRankViewModel{ Tid = 5,Name="娱乐"},
                new RegionRankViewModel{ Tid= 23,Name = "电影"},
                new RegionRankViewModel{ Tid = 11,Name="电视剧"}
            };
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;     
        }      

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                BackEvent();
            }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Pivot.SelectedItem is RegionRankViewModel region)
            {
                if (region.Items.Count != 0) return;
                string url = "";
                if (region.Tid != 0)
                {
                    url = $"http://app.bilibili.com/x/v2/rank/region?rid={region.Tid}&pn=1&ps=20&appkey={ApiHelper._appKey}";
                    url += ApiHelper.GetSign(url);
                }
                else
                {
                    string order = "";
                    switch(region.Name)
                    {
                        case "原创": order = "origin";break;
                        case "全站":order = "all";break;
                        case "B番":order = "bangumi";break;
                    }
                    url = $"http://app.bilibili.com/x/v2/rank?order={order}&pn=1&ps=20&appkey={ApiHelper._appKey}";
                    url += ApiHelper.GetSign(url);
                }
                var result = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(result);
                if(model["code"].Value<int>()==0)
                {
                    var vms = model["data"].Select((token, i) => new RankItemViewModel
                    {
                        Cover = token["cover"].Value<string>(),
                        Param = token["param"].Value<string>(),
                        Name = token["name"].Value<string>(),
                        Pts = $"综合评分: {token["pts"].Value<string>()}",
                        Rank = i + 1,
                        Title = token["title"].Value<string>(),
                        RankFontSize = Converter.RankIndexToFontSize(i + 1),
                        RankForeground = new int[] { 1, 2, 3 }.Any(index => i + 1 == index) ? "#FFDF85A0" : "#FF808080"
                    });
                    foreach(var vm in vms)
                    {
                        region.Items.Add(vm);
                    }
                }
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is RankItemViewModel model)
            {
                Frame.Navigate(typeof(VideoInfoPage), model.Param);
            }
        }
    }
}
