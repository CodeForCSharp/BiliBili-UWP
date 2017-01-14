using bilibili2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2
{
    public sealed partial class MyControl : UserControl
    {
        public delegate void PlayHandler(string aid);
        public event PlayHandler PlayEvent;
        public event PlayHandler ErrorEvent;
        private List<string> order = new List<string> { "hot", "review", "default" };
        private WebClientClass wc = new WebClientClass();
        private List<AVPartitionViewModel> Partitions { get; } = new List<AVPartitionViewModel>
                {
                    new AVPartitionViewModel{ Icon = "ms-appx:///Assets/PartIcon/BFJ.png", Name="番剧", Uid = 13,AVs =new ObservableCollection<AVItemViewModel>(), Order =0},
                    new AVPartitionViewModel{ Icon = "ms-appx:///Assets/PartIcon/BDH.png",Name="动画",Uid = 1,AVs=new ObservableCollection<AVItemViewModel>(),Order =0},
                    new AVPartitionViewModel{ Icon = "ms-appx:///Assets/PartIcon/BYY.png",Name="音乐",Uid=3,AVs=new ObservableCollection<AVItemViewModel>(),Order =0},
                    new AVPartitionViewModel{ Icon ="ms-appx:///Assets/PartIcon/BWD.png",Name="舞蹈",Uid=20,AVs =new ObservableCollection<AVItemViewModel>() ,Order =0},
                    new AVPartitionViewModel{ Icon ="ms-appx:///Assets/PartIcon/BYX.png",Name="游戏",Uid=4,AVs =new ObservableCollection<AVItemViewModel>() ,Order =0},
                    new AVPartitionViewModel{ Icon="ms-appx:///Assets/PartIcon/BKJ.png",Name="科技",Uid=36,AVs =new ObservableCollection<AVItemViewModel>() ,Order =0},
                    new AVPartitionViewModel{ Icon = "ms-appx:///Assets/PartIcon/BYL.png",Name="娱乐",Uid=5,AVs =new ObservableCollection<AVItemViewModel>() ,Order =0},
                    new AVPartitionViewModel{ Icon ="ms-appx:///Assets/PartIcon/BGC.png",Name="鬼畜",Uid=119,AVs =new ObservableCollection<AVItemViewModel>() ,Order =0},
                    new AVPartitionViewModel{ Icon = "ms-appx:///Assets/PartIcon/BDY.png",Name="电影",Uid=23,AVs =new ObservableCollection<AVItemViewModel>() ,Order =0},
                    new AVPartitionViewModel{ Icon ="ms-appx:///Assets/PartIcon/BDSJ.png",Name="电视剧",Uid=11,AVs =new ObservableCollection<AVItemViewModel>() ,Order =0},
                    new AVPartitionViewModel{ Icon ="ms-appx:///Assets/PartIcon/BSS.png",Name="时尚",Uid=155,AVs =new ObservableCollection<AVItemViewModel>(),Order =0 }
                };
        public MyControl()
        {
            this.InitializeComponent();
        }

        public async void SetHomeInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                foreach (var item in Partitions)
                {
                    string result = await wc.GetResults(new Uri($"http://api.bilibili.com/list?type=json&appkey={ApiHelper._appKey}&tid={item.Uid}&page=1&pagesize={10}&order={order[item.Order]}&ver=2&rnd={new Random().Next(1000, 9999)}"));
                    var model = JsonConvert.DeserializeObject<HomeAVModel>(result);
                    if(model.Code==0)
                    {
                        var avs = model.List.Select(av => new AVItemViewModel
                        {
                            Pic = av.Pic,
                            Play = av.Play,
                            Title = av.Title,
                            VideoReview = av.VideoReview,
                            Aid = av.Aid
                        });
                        foreach(var av in avs)
                        {
                            item.AVs.Add(av);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void items_listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(VideoInfoPage));
        }
        private void home_GridView_FJ_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlayEvent((e.ClickedItem as AVItemViewModel).Aid);
        }

        private async void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton button)
            {
                var uid = (int)button.Tag;
                var partition = Partitions.Find(p => p.Uid == uid);
                string result = await wc.GetResults(new Uri($"http://api.bilibili.com/list?type=json&appkey={ApiHelper._appKey}&tid={uid}&page=1&pagesize={10}&order={order[partition.Order]}&ver=2&rnd={new Random().Next(1000, 9999)}"));
                partition.Order++;
                if (partition.Order > 2) { partition.Order = 0; }
                var model = JsonConvert.DeserializeObject<HomeAVModel>(result);
                if (model.Code == 0)
                {
                    var avs = model.List.Select(av => new AVItemViewModel
                    {
                        Pic = av.Pic,
                        Play = av.Play,
                        Title = av.Title,
                        VideoReview = av.VideoReview,
                        Aid = av.Aid
                    });
                    partition.AVs.Clear();
                    foreach (var av in avs)
                    {
                        partition.AVs.Add(av);
                    }
                }
            }
        }
    }
}
