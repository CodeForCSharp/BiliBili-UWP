using bilibili2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2.Controls
{
    public sealed partial class MyLiveControl : UserControl
    {
        public MyLiveControl()
        {
            this.InitializeComponent();
        }
        public delegate void PlayHandler(string aid);
        public event PlayHandler PlayEvent;
        public event PlayHandler ErrorEvent;
        public bool isLoaded= false;
        private ObservableCollection<LivePartitionViewModel> Partitions { get; } = new ObservableCollection<LivePartitionViewModel>();
        public async void GetLiveInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                //&access_key={0}
                string url = $"http://live.bilibili.com/AppIndex/home?_device=android&_ulv=10000&appkey={ApiHelper._appKey}&build=434000&platform=android&scale=xxhdpi";
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                HomeLiveModel model = JsonConvert.DeserializeObject<HomeLiveModel>(results);
                if (model.Code == 0)
                {
                    foreach(var item in model.Data.Partitions)
                    {
                        Partitions.Add(new LivePartitionViewModel
                        {
                            Icon = item.Partition.SubIcon.Src,
                            Name = item.Partition.Name,
                            Lives = item.Lives.Select(live => new LiveItemViewModel
                            {
                                Face = live.Owner.Face,
                                Name = live.Owner.Name,
                                Mid = live.Owner.Mid,
                                Src = live.Cover.Src,
                                RoomId = live.RoomId.ToString(),
                                Online = live.Online,
                                Title = live.Title
                            }).ToList()
                        });
                    }
                    isLoaded = true;
                }
                else
                {
                    ErrorEvent("读取直播失败" + model.Message);
                    isLoaded = false;
                }
            }
            catch (Exception ex)
            {
                ErrorEvent("读取直播失败" + ex.Message);
                isLoaded = false;
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void gridview_Hot_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlayEvent((e.ClickedItem as LiveItemViewModel).RoomId);
        }

        private void HomeLivePanel_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
