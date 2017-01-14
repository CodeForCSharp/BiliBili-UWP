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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
                string url = $"http://live.bilibili.com/AppIndex/home?_device=wp&_ulv=10000&appkey={ApiHelper._appKey}&build=411005&platform=android&scale=xxhdpi";
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                HomeLiveModel model = JsonConvert.DeserializeObject<HomeLiveModel>(results);
                if (model.Code == 0)
                {
                    var partition = model.Data.Partitions.Select(item => new LivePartitionViewModel
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
                    foreach(var item in partition)
                    {
                        Partitions.Add(item);
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

    }
}
