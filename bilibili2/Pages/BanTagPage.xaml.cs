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
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class BanTagPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        ObservableCollection<BanTagItemViewModel> Tags { get; } = new ObservableCollection<BanTagItemViewModel>();
        public BanTagPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            if (e.NavigationMode == NavigationMode.New)
            {
                GetTagInfo();
            }
        }

        //索引
        public async void GetTagInfo()
        {
            try
            {
                PrLoadBan.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string url = $"http://bangumi.bilibili.com/api/tags?_device=wp&_ulv=10000&appkey=422fd9d7289a1dd9&build=411005&page={1}&pagesize=60&platform=android&ts={ApiHelper.GetTimeSpen}000";
                url += $"&sign={ApiHelper.GetSign(url)}";
                string results = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(results);
                if (model["code"].Value<int>() == 0)
                {
                    var vms = model["result"].Select(token => new BanTagItemViewModel
                    {
                        Cover = token["cover"].Value<string>(),
                        TagId = token["tag_id"].Value<string>(),
                        TagName = token["tag_name"].Value<string>()
                    });
                    foreach(var vm in vms)
                    {
                        Tags.Add(vm);
                    }
                }
            }
            catch (Exception ex)
            {
                messShow.Show("读取索引信息失败！\r\n" + ex.Message, 3000);

            }
            finally
            {
                PrLoadBan.Visibility = Visibility.Collapsed;
                // IsLoading = false;
            }
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is BanTagItemViewModel model)
            {
                Frame.Navigate(typeof(BanByTagPage), new string[] { model.TagId, model.TagName });
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
    }
}
