using Microsoft.Toolkit.Uwp;
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
using System.Threading;
using System.Threading.Tasks;
using bilibili2.Class;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveAreaPage : Page
    {
        ObservableCollection<LiveTagViewModel> Tags { get; } = new ObservableCollection<LiveTagViewModel>();
        IncrementalLoadingCollection<LiveItemViewModel> Lives { get; set; }      
        WebClientClass wc = new WebClientClass();
        private string selectedTag;
        private int id;
        private string name;
        public LiveAreaPage()
        {
            Lives = new IncrementalLoadingCollection<LiveItemViewModel>
            {
                LoadDataTask = async () =>
                {
                    var url = $"http://live.bilibili.com/mobile/rooms?_device=wp&_ulv=10000&build=411005&platform=android&appkey={ApiHelper._appKey}&page={Lives.CurrentPage}&area_id={id}&page={Lives.CurrentPage}";
                    if (selectedTag != "全部")
                    {
                        url += $"&tag={selectedTag}";
                    }
                    url += $"&sign={ApiHelper.GetSign(url)}";
                    var result = await wc.GetResults(new Uri(url));
                    var model = JObject.Parse(result);
                    if (model["code"].Value<int>() == 0)
                    {
                        var items = model["data"].Select(token => new LiveItemViewModel
                        {
                            Face = token["owner"]["face"].Value<string>(),
                            Mid = token["owner"]["mid"].Value<int>(),
                            Name = token["owner"]["name"].Value<string>(),
                            Src = token["cover"]["src"].Value<string>(),
                            Online = token["online"].Value<int>(),
                            RoomId = token["room_id"].Value<string>(),
                            Title = token["title"].Value<string>()
                        });
                        return Tuple.Create(items, items.Any());
                    }
                    else
                    {
                        throw new Exception("Vaild Paramters");
                    }
                }
            };
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is Tuple<int,string> tuple)
            {
                id = tuple.Item1;
                name = tuple.Item2;
            }
            TopTxtHeader.Text = name;
            GetTagInfo();
            base.OnNavigatedTo(e);
        }
        private async void GetTagInfo()
        {
            var url = $"http://live.bilibili.com/AppIndex/tags?_device=wp&_ulv=10000&build=411005&platform=android&appkey={ApiHelper._appKey}";
            url += $"&sign={ApiHelper.GetSign(url)}";
            var result = await wc.GetResults(new Uri(url));
            var tags = JObject.Parse(result)["data"][$"{id}"].Select(token => new LiveTagViewModel { Name = token.Value<string>()});
            Tags.Add(new LiveTagViewModel { Name = "全部" });
            foreach(var tag in tags)
            {
                Tags.Add(tag);
            }
            TagPanel.SelectedIndex = 0;
        }

        private void TagPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(TagPanel.SelectedItem is LiveTagViewModel tag)
            {
                selectedTag = tag.Name;
                Lives.Reset();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if(this.Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void LivesPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is LiveItemViewModel model)
            {
                Frame.Navigate(typeof(LiveInfoPage), model.RoomId);
            }
        }
    }
}
