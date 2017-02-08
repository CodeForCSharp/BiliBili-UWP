using bilibili2.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        private string keyword = "";
        private int totalRoom;
        private int totalUser;
        public SearchLivePage()
        {
            Lives = new IncrementalLoadingCollection<LiveItemViewModel>
            {
                LoadDataTask = async () =>
                {
                    string url = $"http://live.bilibili.com/AppSearch/index?_device=wp&_ulv=10000&appkey={ApiHelper._appKey}&build=434000&keyword={WebUtility.UrlEncode(keyword)}&page={Lives.CurrentPage}&pagesize=20&platform=android&type=room";
                    url += "&sign=" + ApiHelper.GetSign(url);
                    string results = await wc.GetResults(new Uri(url));
                    var model = JObject.Parse(results);
                    if (model["code"].Value<int>() == 0)
                    {
                        Lives.MaxPage = model["data"]["room"]["total_page"].Value<int>();
                        totalRoom = model["data"]["room"]["total_room"].Value<int>();
                        var items = model["data"]["room"]["list"].Select(token => new LiveItemViewModel
                        {
                            Face = token["face"].Value<string>(),
                            Name = token["name"].Value<string>(),
                            Src = token["cover"].Value<string>(),
                            Online = token["online"].Value<int>(),
                            RoomId = token["roomid"].Value<string>(),
                            Title = token["title"].Value<string>()
                        });
                        return Tuple.Create(items, Lives.CurrentPage <= Lives.MaxPage);
                    }
                    else
                    {
                        throw new Exception("Vaild Paramters");
                    }
                }
            };
            Users = new IncrementalLoadingCollection<LiveSearchUserViewModel>
            {
                LoadDataTask = async () =>
                {

                    string url = $"http://live.bilibili.com/AppSearch/index?_device=wp&_ulv=10000&appkey={ApiHelper._appKey}&build=434000&keyword={WebUtility.UrlEncode(keyword)}&page={Users.CurrentPage}&pagesize=20&platform=android&type=user";
                    url += "&sign=" + ApiHelper.GetSign(url);
                    string results = await wc.GetResults(new Uri(url));
                    var model = JObject.Parse(results);
                    if (model["code"].Value<int>() == 0)
                    {
                        Users.MaxPage = model["data"]["user"]["total_page"].Value<int>();
                        totalUser = model["data"]["user"]["total_user"].Value<int>();
                        var items = model["data"]["user"]["list"].Select(token => new LiveSearchUserViewModel
                        {
                            AreaName = $"分区:  {token["areaName"].Value<string>()}",
                            Face = token["face"].Value<string>(),
                            FansNum = $"关注数:  {token["fansNum"].Value<int>()}",
                            LiveStatusColor = token["live_status"].Value<int>() == 0 ? "#FF808080" : "#FFDF85A0",
                            LiveStatusString = token["live_status"].Value<int>() == 0 ? "闲置中" : "直播中",
                            Name = token["name"].Value<string>(),
                            RoomId = token["roomid"].Value<int>(),
                            RoomTags = token["roomTags"].Select(tag => new LiveTagViewModel { Name = tag.Value<string>() }).Where(vm=>!string.IsNullOrEmpty(vm.Name)).ToList()
                        });
                        return Tuple.Create(items, Users.CurrentPage <= Users.MaxPage);
                    }
                    else
                    {
                        throw new Exception("Vaild Paramters");
                    }
                }
            };
            this.InitializeComponent();
        }

        private void TxtFind_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (TxtFind.Text.Length == 0)
            {
                MessageShow.Show("搜索内容不能为空！", 3000);
            }
            else if(TxtFind.Text.Length<2||TxtFind.Text.Length>50)
            {
                MessageShow.Show("关键字长度必须在2-50字节以内", 3000);
            }
            else
            {
                keyword = TxtFind.Text;
                Users.Reset();
                Lives.Reset();
                Pivot.Visibility = Visibility.Visible;
            }
        }

        private void LivesPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is LiveItemViewModel model)
            {
                NavigationCacheMode = NavigationCacheMode.Enabled;
                Frame.Navigate(typeof(LiveInfoPage), model.RoomId);
            }
        }

        private void UserList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is LiveSearchUserViewModel model)
            {
                NavigationCacheMode = NavigationCacheMode.Enabled;
                Frame.Navigate(typeof(LiveInfoPage), model.RoomId);
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
