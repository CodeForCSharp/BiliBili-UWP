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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2.Controls
{
    public sealed partial class SubPartionFragment : UserControl
    {
        List<SubPartionSelectionViewModel> Selections { get; } = new List<SubPartionSelectionViewModel>
        {
            new SubPartionSelectionViewModel{ Name="最新",Key="senddate" },
            new SubPartionSelectionViewModel{ Name="播放",Key="view"},
            new SubPartionSelectionViewModel{ Name="弹幕",Key="danmaku"},
            new SubPartionSelectionViewModel{ Name="评论",Key="reply"},
            new SubPartionSelectionViewModel{ Name="收藏",Key="favorite"}
        };
        WebClientClass wc = new WebClientClass();
        public SubPartionViewModel SubPartion { get; set; }
        string currentKey = "senddate";
        Class.IncrementalLoadingCollection<RegionItemViewModel> Items { get; } = new Class.IncrementalLoadingCollection<RegionItemViewModel>();
        public SubPartionFragment()
        {
            Items.LoadDataTask = async () =>
            {
                var url = $"http://app.bilibili.com/x/v2/region/show/child/list?platform=android&build=421000&rid={SubPartion.Tid}&pn={Items.CurrentPage}&ps={20}&order={currentKey}&appkey={ApiHelper._appKey}";
                url += $"&sign={ApiHelper.GetSign(url)}";
                var result = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(result);
                if(model["code"].Value<int>()==0)
                {
                    var items = model["data"].Select(item => new RegionItemViewModel
                    {
                        Cover = item["cover"].Value<string>(),
                        Danmaku = item["danmaku"]?.Value<string>() ?? "-",
                        Play = item["play"]?.Value<string>() ?? "-",
                        Param = item["param"].Value<string>(),
                        Title = item["title"].Value<string>(),
                        Name = item["name"].Value<string>()
                    });
                    return Tuple.Create(items,items.Any());
                }
                else
                {
                    throw new Exception("Vaild Parameter");
                }
            };
            this.InitializeComponent();
        }

        private void SelectionPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentKey = (SelectionPanel.SelectedItem as SubPartionSelectionViewModel).Key;
            Items.Reset();
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is RegionItemViewModel model)
            {
                if(Window.Current.Content is Frame frame)
                {
                    frame.Navigate(typeof(VideoInfoPage), model.Param);
                }
            }
        }
    }
}
