using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PartionPage : Page
    {
        Class.IncrementalLoadingCollection<RegionItemViewModel> Dynamics { get; } = new Class.IncrementalLoadingCollection<RegionItemViewModel>();
        private WebClientClass wc = new WebClientClass();
        private PartionViewModel partion;
        public PartionPage()
        {
            Dynamics.LoadDataTask = async () =>
            {
                var url = $"http://app.bilibili.com/x/v2/region/show/dynamic?platform=android&build=421000&rid={partion.Tid}&pn={Dynamics.CurrentPage}&ps=20&appkey={ApiHelper._appKey}";
                url +=$"&sign={ApiHelper.GetSign(url)}";
                var result = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(result);
                if(model["code"].Value<int>()==0)
                {
                    var items = Parse(model["data"]);
                    return Tuple.Create(items.AsEnumerable(),items.Any());
                }
                else
                {
                    throw new Exception("Vaild Parameter");
                }
            };
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is PartionViewModel model)
            {
                partion = model;
                SetHeaderAsync();
                RegionNamePanel.Text = partion.Name;
                foreach(var item in model.SubPartions)
                {
                    PivotPanel.Items.Add(new PivotItem
                    {
                        Header = item.Name,
                        Content = new Controls.SubPartionFragment
                        {
                             SubPartion = item
                        }
                    });
                }
            }
            base.OnNavigatedTo(e);
        }

        private async void SetHeaderAsync()
        {
            var url = $"http://app.bilibili.com/x/v2/region/show?platform=android&build=421000&rid={partion.Tid}&channel=*&appkey={ApiHelper._appKey}";
            url += $"&sign={ApiHelper.GetSign(url)}";
            var result = await wc.GetResults(new Uri(url));
            var model = JObject.Parse(result);
            if (model["code"].Value<int>()==0)
            {
                var header = new RegionHeaderViewModel
                {
                    Banners = model["data"]["banner"]["top"].Select(token => new RegionBannerViewModel
                    {
                        Id = token["id"].Value<int>(),
                        Image = token["image"].Value<string>(),
                        Title = token["title"].Value<string>(),
                        Uri = token["uri"].Value<string>()
                    }).ToList(),
                    SubPartions = partion.SubPartions,
                    Hots = Parse(model["data"]["recommend"]),
                    News = Parse(model["data"]["new"])
                };
                RecommendPanel.Header = header;
            }
        }

        private List<RegionItemViewModel> Parse(JToken token)
        {
            return token.Select(item => new RegionItemViewModel
            {
                Cover = item["cover"].Value<string>(),
                Danmaku = item["danmaku"]?.Value<string>() ?? "-",
                Play = item["play"]?.Value<string>() ?? "-",
                Param = item["param"].Value<string>(),
                Title = item["title"].Value<string>()
            }).ToList();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if(Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void RegionItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is RegionItemViewModel model)
            {
                Frame.Navigate(typeof(VideoInfoPage), model.Param);
            }
        }

        private void BannerPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is RegionBannerViewModel model)
            {
                Frame.Navigate(typeof(WebViewPage), model.Uri);
            }
        }

        private void SubPartionPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is SubPartionViewModel model)
            {
                PivotPanel.SelectedIndex = model.Index;
            }
        }
    }
}
