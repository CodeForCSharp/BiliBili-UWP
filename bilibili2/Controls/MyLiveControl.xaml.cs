using bilibili2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            gridList = new List<GridView> { gridview_Hot, gridview_MZ, gridview_HH, gridview_YZ, gridview_SH, gridview_DJ, gridview_WL, gridview_JJ, gridview_FY };
        }
        public delegate void PlayHandler(string aid);
        public event PlayHandler PlayEvent;
        public event PlayHandler ErrorEvent;
        public bool isLoaded= false;
        private List<GridView> gridList;
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                ViewBox2_num.Width = ActualWidth / 2 - 15;
                double d = ((ViewBox2_num.Width + 12) / 1.15) * 2;
                GridSizeChange(d);
                //PageCount = 4;
            }
            else if (this.ActualWidth <= 800)
            {
                ViewBox2_num.Width = ActualWidth / 3 - 15;
                double d = ((ViewBox2_num.Width + 12) / 1.15) * 2;
                GridSizeChange(d);
            }
            else
            {
                int i = Convert.ToInt32(ActualWidth / 200);
                ViewBox2_num.Width = ActualWidth / i - 15;
                double d = ((ViewBox2_num.Width + 12) / 1.15);
                GridSizeChange(d);
            }
        }
        private void GridSizeChange(double d)
        {
            gridList.ForEach(grid => { grid.Height = d; });
        }
        private void GridClear()
        {
            gridList.ForEach(grid => { grid.Items.Clear(); });
        }
        public async void GetLiveInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                GridClear();
                WebClientClass wc = new WebClientClass();
                //&access_key={0}
                string url = $"http://live.bilibili.com/AppIndex/home?_device=wp&_ulv=10000&appkey={ApiHelper._appKey}&build=411005&platform=android&scale=xxhdpi";
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                HomeLiveModel model = JsonConvert.DeserializeObject<HomeLiveModel>(results);
                if (model.Code == 0)
                {
                    var nameList = new List<string> { "手机直播", "唱见舞见", "绘画专区", "御宅文化", "手游直播", "单机联机", "网络游戏", "电子竞技", "放映厅" };
                    foreach (var item in model.Data.Partitions)
                    {
                        var index = nameList.FindIndex(s => s == item.Partition.Name);
                        if (index == -1) continue; 
                        var lives = item.Lives.Select(live => new LiveItemViewModel
                        {
                            Face = live.Owner.Face,
                            Name = live.Owner.Name,
                            Mid = live.Owner.Mid,
                            Src = live.Cover.Src,
                            RoomId = live.RoomId.ToString(),
                            Online = live.Online,
                            Title = live.Title
                        });
                        foreach(var live in lives)
                        {
                            gridList[index].Items.Add(live);
                        }
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
