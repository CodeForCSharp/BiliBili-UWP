using bilibili2.Class;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BanTimelinePage : Page
    {
        ObservableCollection<BanTimeLinePartitionViewModel> Partitions { get; } = new ObservableCollection<BanTimeLinePartitionViewModel>();
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public BanTimelinePage()
        {
            this.InitializeComponent();
        }
       
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                GetBangumiTimeLine();
                bg.Color = ((SolidColorBrush)Frame.Tag).Color;
            }
        }
        //时间表
        public async void GetBangumiTimeLine()
        {
            try
            {
                PrLoadBan.Visibility = Visibility.Visible;
               WebClientClass  wc = new WebClientClass();
                var url = $"https://bangumi.bilibili.com/api/timeline_v4?_device=android&_ulv=10000&build=434000&platform=android&appkey=1d8b6e7d45233436&area_id=1{Uri.EscapeDataString(",")}2{Uri.EscapeDataString(",")}-1";
                url += $"&sign={ApiHelper.GetSign(url)}";
                var results = await wc.GetResults(new Uri(url));
                var model = JObject.Parse(results);
                if(model["code"].Value<int>()==0)
                {
                    var vms = model["result"].Select(token => new
                    {
                        Cover = token["cover"].Value<string>(),
                        Title = token["title"].Value<string>(),
                        SeasonId = token["season_id"].Value<int>(),
                        Ontime = token["ontime"].Value<string>(),
                        PubDate = token["pub_date"].Value<string>(),
                        EpIndex = token["ep_index"].Value<string>()
                    })
                    .OrderBy(item => DateTime.Parse($"{item.PubDate} {item.Ontime}"))
                    .GroupBy(item => item.PubDate)
                    .Select(item => new BanTimeLinePartitionViewModel
                    {
                        Week = Converter.DateToWeek(item.Key),
                        PubDate = Converter.DateToSimplifyDate(item.Key),
                        WeekIconPath = Converter.WeekToWeekIcon(item.Key),
                        BanTimeItems = item.Select(value => new BanTimeLineItemViewModel
                        {
                            Cover = value.Cover,
                            EpIndex = int.TryParse(value.EpIndex,out var index) ?$"第{value.EpIndex}话": $"{value.EpIndex}",
                            Ontime = value.Ontime,
                            PubDate = value.PubDate,
                            SeasonId = value.SeasonId,
                            Title = value.Title
                        }).ToList()
                    });
                    foreach(var vm in vms)
                    {
                        Partitions.Add(vm);
                    }
                }
                PrLoadBan.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog("读取番剧更新失败\r\n" + ex.Message);
                await md.ShowAsync();
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

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is BanTimeLineItemViewModel model)
            {
                Frame.Navigate(typeof(BanInfoPage), model.SeasonId.ToString());
            }
        }
    }
}
