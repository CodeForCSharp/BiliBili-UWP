using bilibili2.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        IncrementalLoadingCollection<BanSearchArchiveViewModel> Archives { get; }
        IncrementalLoadingCollection<BanSearchBanViewModel> Bans { get; }
        IncrementalLoadingCollection<BanSearchUpViewModel> UPs { get; }
        IncrementalLoadingCollection<BanSearchMoiveViewModel> Moives { get; }
        IncrementalLoadingCollection<BanSearchSpecialViewModel> Specials { get; } 
        public SearchPage()
        {
            Archives = new IncrementalLoadingCollection<BanSearchArchiveViewModel>
            {
                LoadDataTask = async () =>
                {
                    var model = await SearchAllAsync(Archives.CurrentPage);
                    if(model["code"].Value<int>()==0)
                    {
                        var items = model["data"]["items"]["archive"].Select(token => new BanSearchArchiveViewModel
                        {
                            Author = token["author"].Value<string>(),
                            Cover = token["cover"].Value<string>(),
                            Danmaku = token["danmaku"].Value<int>(),
                            Duration = Converter.DurationToLongDuration(token["duration"].Value<string>()),
                            Param = token["param"].Value<string>(),
                            Play = $"{token["play"].Value<int>()}",
                            Title = token["title"].Value<string>()
                        });
                        return Tuple.Create(items,items.Any());
                    }
                    throw new Exception("Vaild Parameter");
                }
            };
            Bans = new IncrementalLoadingCollection<BanSearchBanViewModel>
            {
                LoadDataTask = async () =>
                {
                    var model = await SearchTypeAsync(1, Bans.CurrentPage);
                    if (model["code"].Value<int>()==0)
                    {
                        Bans.MaxPage = model["data"]["pages"].Value<int>();
                        var items = model["data"]["items"].Select(token => new BanSearchBanViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            CatDesc = token["cat_desc"].Value<string>(),
                            Param = token["param"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            NewestSeason = token["newest_season"].Value<string>(),
                            Index = token["finish"]?.Value<int>() ==null? $"更新至第{token["index"].Value<string>()}话" : $"{token["index"].Value<string>()}话全"
                        });
                        return Tuple.Create(items,Bans.CurrentPage<=Bans.MaxPage);
                    }
                    throw new Exception("Vaild Parameter");
                }
            };
            UPs = new IncrementalLoadingCollection<BanSearchUpViewModel>
            {
                LoadDataTask = async () =>
                {
                    var model = await SearchTypeAsync(2, UPs.CurrentPage);
                    if(model["code"].Value<int>()==0)
                    {
                        UPs.MaxPage = model["data"]["pages"].Value<int>();
                        var items = model["data"]["items"].Select(token => new BanSearchUpViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Title = token["title"].Value<string>(),
                            Param = token["param"].Value<string>(),
                            Sign = token["sign"]?.Value<string>() ?? "",
                            Archives = $"视频数: {token["archives"].Value<int>()}",
                            Fans = $"粉丝: {token["fans"].Value<int>()}"
                        });
                        return Tuple.Create(items,UPs.CurrentPage<=UPs.MaxPage);
                    }
                    throw new Exception("Vaild Parameter");
                }
            };
            Moives = new IncrementalLoadingCollection<BanSearchMoiveViewModel>
            {
                LoadDataTask = async () =>
                {
                    var model = await SearchTypeAsync(3, Moives.CurrentPage);
                    if(model["code"].Value<int>()==0)
                    {
                        Moives.MaxPage = model["data"]["pages"].Value<int>();
                        var items = model["data"]["items"].Select(token => new BanSearchMoiveViewModel
                        {
                            Cover = token["cover"].Value<string>(),
                            Goto = token["goto"].Value<string>(),
                            Param = token["param"].Value<string>(),
                            Title = token["title"].Value<string>()
                        });
                        return Tuple.Create(items,Moives.CurrentPage<=Moives.MaxPage);
                    }
                    throw new Exception("Vaild Parameter");
                }
            };
            Specials = new IncrementalLoadingCollection<BanSearchSpecialViewModel>
            {
                LoadDataTask = async () =>
                {
                    var model = await SearchTypeAsync(4, Specials.CurrentPage);
                    if(model["code"].Value<int>()==0)
                    {
                        Specials.MaxPage = model["data"]["pages"].Value<int>();
                        var items = model["data"]["items"].Select(token => new BanSearchSpecialViewModel
                        {
                            Title = token["title"].Value<string>(),
                            Cover = token["cover"].Value<string>(),
                            Desc = token["desc"].Value<string>(),
                            Param = token["param"].Value<string>(),
                            Archives = $"视频: {token["archives"].Value<int>()}",
                            Play = $"播放: {token["play"].Value<int>()}"
                        });
                        return Tuple.Create(items,Specials.CurrentPage<=Specials.MaxPage);
                    }
                    throw new Exception("Vaild Parameter");
                }
            };
            this.InitializeComponent();
        }
        private string keyword = "";

        private readonly WebClientClass _wc = new WebClientClass();
        //搜索视频
        public async Task<JObject> SearchTypeAsync(int type,int pn)
        {
            var url = $"http://app.bilibili.com/x/v2/search/type?_device=android&appkey=84956560bc028eb7&build=434000&keyword={Uri.EscapeDataString(keyword)}&pn={pn}&ps=20&platform=android&type={type}";
            url += $"&sign={ApiHelper.GetSign(url)}";
            var result = await _wc.GetResults(new Uri(url));
            return JObject.Parse(result);
        }

        public async Task<JObject> SearchAllAsync(int pn)
        {
            var url = $"http://app.bilibili.com/x/v2/search?_device=android&appkey=84956560bc028eb7&build=434000&keyword={Uri.EscapeDataString(keyword)}&pn={pn}&ps=20&platform=android&order={"default"}";//rid={}
            url += $"&sign={ApiHelper.GetSign(url)}";
            var result = await _wc.GetResults(new Uri(url));
            return JObject.Parse(result);
        }
        //开始搜索
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is string word)
            {
                keyword = word;
                SearchPanel.Text = word;
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
                BackEvent?.Invoke();
            }
        }

        private void SearchAllPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is BanSearchArchiveViewModel model)
            {
                Frame.Navigate(typeof(VideoInfoPage), model.Param);
            }
        }

        private void BanPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is BanSearchBanViewModel model)
            {
                Frame.Navigate(typeof(BanInfoPage), model.Param);
            }
        }

        private void UpPanel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is BanSearchUpViewModel model)
            {
                Frame.Navigate(typeof(UserInfoPage), model.Param);
            }
        }
    }
}
