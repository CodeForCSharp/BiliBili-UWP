
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
    public sealed partial class AllLivePage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        private WebClientClass wc = new WebClientClass();
        ObservableCollection<LiveAreasViewModel> Areas { get; } = new ObservableCollection<LiveAreasViewModel>();
        public AllLivePage()
        {
            this.InitializeComponent();
            GetAllAreas();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Btn_back_Click(object sender, RoutedEventArgs e)
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
        private async void GetAllAreas()
        {
            PrLoad.Visibility = Visibility.Visible;
            var result = await wc.GetResults(new Uri($"http://live.bilibili.com/AppIndex/areas?_device=wp&_ulv=10000&appkey={ApiHelper._appKey}&build=434000&platform=android&scale=xxhdpi"));
            var model = JObject.Parse(result);
            if (model["code"].Value<int>() == 0)
            {
                var vms = model["data"].Select(token => new LiveAreasViewModel
                {
                    Icon = token["entrance_icon"]["src"].Value<string>(),
                    Name = token["name"].Value<string>(),
                    Id =token["id"].Value<int>()
                });
                foreach(var vm in vms)
                {
                    Areas.Add(vm);
                }
            }
            PrLoad.Visibility = Visibility.Collapsed;
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is LiveAreasViewModel model)
            {
                Frame.Navigate(typeof(LiveAreaPage), Tuple.Create(model.Id, model.Name));
            }
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(AreasPanel.ItemsPanelRoot is ItemsWrapGrid item)
            {
                item.ItemWidth = e.NewSize.Width / 4;
                item.ItemHeight = 240;
            }
        }
    }
}
