using bilibili2.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TopicPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        IncrementalLoadingCollection<BanTopicViewModel> Topics { get; }
        WebClientClass wc = new WebClientClass();
        public TopicPage()
        {
            Topics = new IncrementalLoadingCollection<BanTopicViewModel>
            {
                LoadDataTask = async () =>
                {
                    var url = $"http://api.bilibili.com/topic/getlist?_device=android&appkey=84956560bc028eb7&build=434000&pagesize=20&page={Topics.CurrentPage}";
                    url += $"&sign={ApiHelper.GetSign(url)}";
                    var result = await wc.GetResults(new Uri(url));
                    var model = JObject.Parse(result);
                    if (model["code"].Value<int>() == 0)
                    {
                        Topics.MaxPage = model["pages"].Value<int>();
                        var items = model["list"].Select(token => new BanTopicViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Link = token["link"].Value<string>(),
                            Title = token["title"].Value<string>()
                        });
                        return Tuple.Create(items, Topics.CurrentPage <= Topics.MaxPage);
                    }
                    else
                    {
                        throw new Exception("Vaild Parameter");
                    }
                }
            };
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                Topics.Reset();
            }
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

        private void ListTopicPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is BanTopicViewModel model)
            {
                Frame.Navigate(typeof(WebViewPage), model.Link);
            }
        }
    }
}
