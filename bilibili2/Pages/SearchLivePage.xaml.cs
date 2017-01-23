using bilibili2.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
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
    public sealed partial class SearchLivePage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        IncrementalLoadingCollection<LiveItemViewModel> Lives { get; }
        IncrementalLoadingCollection<LiveSearchUserViewModel> Users { get; }
        WebClientClass wc = new WebClientClass();
        private string keyword;
        public SearchLivePage()
        {
            Lives = new IncrementalLoadingCollection<LiveItemViewModel>
            {
                LoadDataTask = async () =>
                {
                    string url = $"http://live.bilibili.com/AppSearch/index?_device=wp&_ulv=10000&appkey={ApiHelper._appKey}&build=411005&keyword={WebUtility.UrlEncode(keyword)}&page={Lives.CurrentPage}&pagesize=20&platform=android&type=room";
                    url += "&sign=" + ApiHelper.GetSign(url);
                    string results = await wc.GetResults(new Uri(url));
                    var model = JObject.Parse(results);
                    if (model["code"].Value<int>() == 0)
                    {
                        Lives.MaxPage = model["data"]["total_page"].Value<int>();
                        var items = model["data"]["room"].Select(token => new LiveItemViewModel
                        {
                            Face=token["face"].Value<string>(),
                            Name = token["name"].Value<string>(),
                            Src = token["cover"].Value<string>(),
                            Online = token["online"].Value<int>(),
                            RoomId = token["room_id"].Value<string>(),
                            Title = token["title"].Value<string>()
                        });
                        return Tuple.Create(items, Lives.CurrentPage<=Lives.MaxPage));
                    }
                    else
                    {
                        throw new Exception("Vaild Paramters");
                    }
                }
            };
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
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

        //偷懒。。。pagesize就直接写100.。。。
        private async void GetSearchResults(string keyword)
        {
            try
            {
                PrLoad.Visibility = Visibility.Visible;
                //http://live.bilibili.com/AppSearch/index?_device=wp&_ulv=10000&access_key={0}&appkey={1}&build=411005&keyword={2}&page=1&pagesize=20&platform=android&type=all
                WebClientClass wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/AppSearch/index?_device=wp&_ulv=10000&access_key={0}&appkey={1}&build=411005&keyword={2}&page=1&pagesize=100&platform=android&type=all", ApiHelper.access_key, ApiHelper._appKey, WebUtility.UrlEncode(keyword));
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                SearchLiveModel model = JsonConvert.DeserializeObject<SearchLiveModel>(results);
                if (model.code == 0)
                {
                    SearchLiveModel dataModel = JsonConvert.DeserializeObject<SearchLiveModel>(model.data.ToString());
                    SearchLiveModel roomModel = JsonConvert.DeserializeObject<SearchLiveModel>(dataModel.room.ToString());
                    SearchLiveModel UserModel = JsonConvert.DeserializeObject<SearchLiveModel>(dataModel.user.ToString());
                    btn_Liveing.Content = "正在直播(" + roomModel.total_room + ")";
                    btn_User.Content = "主播(" + UserModel.total_user + ")";
                    live_List.ItemsSource = JsonConvert.DeserializeObject<List<SearchLiveModel>>(roomModel.list.ToString());
                    list_User.ItemsSource = JsonConvert.DeserializeObject<List<SearchLiveModel>>(UserModel.list.ToString());
                }
                else
                {
                    messShow.Show(model.message, 3000);
                }
            }
            catch (Exception)
            {
                messShow.Show("读取信息失败", 3000);
            }
            finally
            {
                PrLoad.Visibility = Visibility.Collapsed;
            }
        }

        private void live_HOT_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(LiveInfoPage), (e.ClickedItem as SearchLiveModel).roomid);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(LiveInfoPage),(e.ClickedItem as SearchLiveModel).roomid);
        }

        private void txt_Find_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (txt_Find.Text.Length==0)
            {
                messShow.Show("搜索内容不能为空！",3000);
                return;
            }
            GetSearchResults(txt_Find.Text);
        }
    }
}
