using bilibili2.Pages;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class RegionPageFragment : UserControl
    {
        public RegionPageFragment()
        {
            this.InitializeComponent();
            SetGridAsync();
        }
        ObservableCollection<PartionViewModel> Partions { get;  } = new ObservableCollection<PartionViewModel>();
        private async void SetGridAsync()
        { 
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Data/region.json"));
            var stream = await file.OpenReadAsync();
            var json = await stream.ReadTextAsync(Encoding.UTF8);
            var model = JArray.Parse(json);
            var vms = model.Select(token => new PartionViewModel
            {
                Icon = $"ms-appx:///Assets/Icon/ic_category_t{token["tid"].Value<int>()}.png",
                Name = token["name"].Value<string>(),
                Reid = token["reid"].Value<int>(),
                Tid = token["tid"].Value<int>(),
                SubPartions = token["children"].Select(item => new SubPartionViewModel
                {
                    Icon = $"ms-appx:///Assets/Icon/ic_category_t{item["tid"].Value<int>()}.png",
                    Tid = item["tid"].Value<int>(),
                    Reid = item["reid"].Value<int>(),
                    Name = item["name"].Value<string>()
                }).ToList()
            });
            foreach (var vm in vms)
            {
                Partions.Add(vm);
            }
        }

        private void PartionPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is PartionViewModel model)
            {
                if(Window.Current.Content is Frame frame)
                {
                    frame.Navigate(typeof(PartionPage), model);
                }
            }
        }

        private void PartionPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (PartionPanel.ItemsPanelRoot is ItemsWrapGrid item)
            {
                item.ItemWidth = e.NewSize.Width / 4;
                item.ItemHeight = 240;
            }
        }
    }
}
