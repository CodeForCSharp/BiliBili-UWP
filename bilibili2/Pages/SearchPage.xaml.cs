using bilibili2.Class;
using Newtonsoft.Json;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        IncrementalLoadingCollection<object> All { get; }
        IncrementalLoadingCollection<BanSearchBanViewModel> Bans { get; }
        IncrementalLoadingCollection<BanSearchUpViewModel> UPs { get; }
        IncrementalLoadingCollection<BanSearchMoiveViewModel> Moives { get; }
        IncrementalLoadingCollection<BanSearchSpecialViewModel> Special { get; } 
        public SearchPage()
        {
            this.InitializeComponent();
        }
        private string keyword = "";
        WebClientClass wc;
        //搜索视频
        public void SearchType()
        {
            var url = $"http://app.bilibili.com/x/v2/search/type?_device=android&appkey=422fd9d7289a1dd9&build=411005&keyword={Uri.EscapeDataString("LOL")}&pn={1}&ps=20&platform=android&type={2}";
            url += $"&sign={ApiHelper.GetSign(url)}";

        }

        public void SearchAll()
        {
            var url = $"http://app.bilibili.com/x/v2/search?_device=android&appkey=422fd9d7289a1dd9&build=411005&keyword={Uri.EscapeDataString("LOL")}&pn={1}&ps=20&platform=android&duration={0}&order={"default"}";//rid={}
            url += $"&sign={ApiHelper.GetSign(url)}";
        }
        //开始搜索
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            if (e.NavigationMode == NavigationMode.New)
            {
                keyword = Uri.EscapeDataString((string)e.Parameter);
            }
        }

        private void Seach_listview_Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), ((SVideoModel)e.ClickedItem).aid);
        }

        private void Seach_listview_Up_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(UserInfoPage), ((SUpModel)e.ClickedItem).mid);
        }

        private void Seach_listview_Ban_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(BanInfoPage), ((SBanModel)e.ClickedItem).season_id);
            //this.Frame.Navigate(typeof(BanSeasonNewPage), ((SeachBanModel)e.ClickedItem).season_id);
        }

        private void Seach_listview_Sp_ItemClick(object sender, ItemClickEventArgs e)
        {
            //this.Frame.Navigate(typeof(BanSeasonPage), ((SSpModel)e.ClickedItem).spid);
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
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
