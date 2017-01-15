﻿using bilibili2.Model;
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
            var result = await wc.GetResults(new Uri($"http://live.bilibili.com/AppIndex/areas?_device=wp&_ulv=10000&appkey={ApiHelper._appKey}&build=411005&platform=android&scale=xxhdpi"));
            var model = JsonConvert.DeserializeObject<LiveAreasModel>(result);
            if (model.Code == 0)
            {
                foreach(var item in model.Data)
                {
                    Areas.Add(new LiveAreasViewModel
                    {
                        Icon = item.EntranceIcon.Src,
                        Name = item.Name,
                        Id = item.Id
                    });
                }
            }
            PrLoad.Visibility = Visibility.Collapsed;
        }
    }
}
