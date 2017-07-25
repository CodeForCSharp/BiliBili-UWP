using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2.Controls
{
    public sealed partial class MyDanmaku : UserControl
    {
        public MyDanmaku()
        {
            timer.Tick += (sender,args) =>
            {
                ClearTopButtomDanmuku();
            };
            timer.Start();
            this.InitializeComponent();
        }
        /// <summary>
        /// 弹幕字体
        /// </summary>
        public string fontFamily = "微软雅黑";
        /// <summary>
        /// 弹幕字体大小
        /// </summary>
        public double fontSize = 22;
        /// <summary>
        /// 弹幕速度
        /// </summary>
        public int Speed = 12;
        /// <summary>
        /// 弹幕透明度
        /// </summary>
        public double Tran = 1;
        /// <summary>
        /// 是否正在播放
        /// </summary>
        private bool isPlaying = true;
        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
            set
            {
                isPlaying = value;
                if(value)
                {
                    Resume();
                    timer.Start();
                }
                else
                {
                    Pause();
                    timer.Stop();
                }
            }
        }
        private bool[] isOccupied;
        private int rows = 0;
        private List<Storyboard> storyBoards = new List<Storyboard>();
        private int rowHeight = 36;
        private int row = 0;//行数
        private DispatcherTimer timer = new DispatcherTimer
        {
             Interval =new TimeSpan(0,0,0,0,500)
        };

        private void DanmakuGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetDanmakuGrid();
        }
        private void ClearAllRow()
        {
            for (int i = 0; i < rows; i++)
            {
                isOccupied[i] = false;
            }
        }

        private int GetAvailableRow()
        {
            for (int i = 0; i < rows; i++)
            {
                if(!isOccupied[i])
                {
                    isOccupied[i] = true;
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 计算并设置Grid的行数
        /// </summary>
        private void SetDanmakuGrid()
        {
            rows = Convert.ToInt32(DanmakuGrid.RenderSize.Height / rowHeight) - 1;
            if (rows < 0) return;
            var currentRows = DanmakuGrid.RowDefinitions.Count;
            if (currentRows < rows)
            {
                for (int i = currentRows; i < rows; i++)
                {
                    DanmakuGrid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(rowHeight)
                    });
                }
            }
            else if (currentRows > rows)
            {
                for (int i = 0; i < currentRows - rows; i++)
                {
                    DanmakuGrid.RowDefinitions.RemoveAt(currentRows - i - 1);
                }
            }
            isOccupied = new bool[rows];
        }
        /// <summary>
        /// 添加滚动弹幕
        /// </summary>
        /// <param name="model">弹幕参数</param>
        /// <param name="isMyself">是否自己发送的</param>
        public void AddFloatDanmaku(DanmakuViewModel model, bool isMyself)
        {
            try
            {
                //创建基础控件
                TextBlock tx = new TextBlock
                {
                    Text = model.Text,
                    Foreground = model.Color,
                    FontSize = model.Size == 25 ? fontSize : fontSize - 2,
                    FontFamily = new FontFamily(fontFamily),
                    DataContext = model
                };
                TextBlock tx2 = new TextBlock
                {
                    Margin = new Thickness(1,1,0,0),
                    Text = model.Text,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                    FontSize = model.Size == 25 ? fontSize : fontSize - 2,
                    FontFamily = new FontFamily(fontFamily),
                };
                Grid grid = new Grid();
                grid.Children.Add(tx2);
                grid.Children.Add(tx);
                var moveTransform = new TranslateTransform
                {
                    X = DanmakuGrid.ActualWidth
                };
                grid.RenderTransform = moveTransform;
                //将弹幕加载入控件中,并且设置位置
                DanmakuGrid.Children.Add(grid);
                int i = GetAvailableRow();
                if (i == -1)
                {
                    ClearAllRow();
                    i = GetAvailableRow();
                }
                Grid.SetRow(grid, i);
                if (row == rows)
                {
                    row = 0;
                }
                if (isMyself)
                {
                    grid.BorderThickness = new Thickness(2);
                    grid.BorderBrush = new SolidColorBrush(Colors.Gray);
                }
                //更新弹幕UI，不更新无法获得弹幕的ActualWidth
                grid.UpdateLayout();
                //创建动画
                Duration duration = new Duration(TimeSpan.FromSeconds(Speed));
                DoubleAnimation myDoubleAnimationX = new DoubleAnimation
                {
                    Duration = duration,
                    To = -(grid.ActualWidth),//到达
                };
                //创建故事版
                Storyboard justintimeStoryboard = new Storyboard
                {
                    Duration = duration
                };
                justintimeStoryboard.Children.Add(myDoubleAnimationX);
                Storyboard.SetTarget(myDoubleAnimationX, moveTransform);
                //故事版加入动画
                Storyboard.SetTargetProperty(myDoubleAnimationX, "X");
                justintimeStoryboard.Completed += (sender, args) =>
                {
                    DanmakuGrid.Children.Remove(grid);
                    storyBoards.Remove(justintimeStoryboard);
                    if (i < isOccupied.Length)
                    {
                        isOccupied[i] = false;
                    }
                };
                storyBoards.Add(justintimeStoryboard);
                justintimeStoryboard.Begin();
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 暂停所有弹幕
        /// </summary>
        public void Pause()
        {
            storyBoards.ForEach(s => s.Pause());
        }
        /// <summary>
        /// 恢复所有被暂停的弹幕
        /// </summary>
        public void Resume()
        {
            storyBoards.ForEach(s => s.Resume());
        }
        /// <summary>
        /// 清除全部弹幕
        /// </summary>
        public void ClearDanmu()
        {
            row = 0;
            DanmakuGrid.Children.Clear();
            grid_Danmu2.Children.Clear();
        }
        /// <summary>
        /// 添加顶部及底部弹幕
        /// </summary>
        /// <param name="model">弹幕参数</param>
        /// <param name="isTop">是否顶部</param>
        /// <param name="isMyself">是否自己发送的</param>
        public void AddTopButtomDanmaku(DanmakuViewModel model, bool isTop, bool isMyself)
        {
            TextBlock tx = new TextBlock
            {
                Text = model.Text,
                Foreground = model.Color,
                FontSize = model.Size == 25 ? fontSize : fontSize - 2,
                FontFamily = new FontFamily(fontFamily),
                DataContext = model
            };
            TextBlock tx2 = new TextBlock
            {
                Margin = new Thickness(1, 1, 0, 0),
                Text = model.Text,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                FontSize = model.Size == 25 ? fontSize : fontSize - 2,
                FontFamily = new FontFamily(fontFamily),
            };
            Grid grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = 0
            };
            grid.Children.Add(tx2);
            grid.Children.Add(tx);
            if (isMyself)
            {
                grid.BorderThickness = new Thickness(2);
                grid.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            grid.UpdateLayout();

            if (isTop)
            {
                D_Top.Children.Add(grid);
            }
            else
            {
                D_Bottom.Children.Add(grid);
            }
        }
        /// <summary>
        /// 清除顶部及底部弹幕
        /// </summary>
        private void ClearTopButtomDanmuku()
        {
            ClearDanmaku(D_Bottom);
            ClearDanmaku(D_Top);
        }
        /// <summary>
        /// 清除Stack中的弹幕
        /// </summary>
        /// <param name="panel"></param>
        private void ClearDanmaku(StackPanel panel)
        {
            foreach (Grid item in panel.Children)
            {
                if (item.Tag is int span)
                {
                    if (span >= 5000)
                    {
                        panel.Children.Remove(item);
                    }
                }
            }
            foreach (Grid item in panel.Children)
            {
                if (item.Tag is int span)
                {
                    if (span < 5000)
                    {
                        item.Tag = span + 500;
                    }
                }
            }
        }
        /// <summary>
        /// 读取弹幕屏幕中的弹幕
        /// </summary>
        /// <returns></returns>
        public List<DanmakuViewModel> GetScreenDanmaku()
        {
            return D_Top.Children.Concat(D_Bottom.Children)
                .Concat(DanmakuGrid.Children)
                .Select(grid=>(grid as Grid).DataContext as DanmakuViewModel)
                .ToList();
        }
        /// <summary>
        /// 移除当前屏幕中的弹幕
        /// </summary>
        public void RemoveDanmaku(DanmakuViewModel model)
        {
            foreach (Grid item in DanmakuGrid.Children)
            {
                if (item.DataContext== model)
                {
                    DanmakuGrid.Children.Remove(item);
                }
            }
            foreach (Grid item in D_Bottom.Children)
            {
                if (item.DataContext == model)
                {
                    D_Bottom.Children.Remove(item);
                }
            }
            foreach (Grid item in D_Top.Children)
            {
                if (item.DataContext == model)
                {
                    D_Top.Children.Remove(item);
                }
            }

        }
        /// <summary>
        /// 弹幕可见
        /// </summary>
        /// <param name="isVisible">是否可见</param>
        /// <param name="mode">模式</param>
        public void SetDanmuVisibility(bool isVisible, DanmakuMode mode)
        {
            if (isVisible)
            {
                switch (mode)
                {
                    case DanmakuMode.Roll:
                        DanmakuGrid.Visibility = Visibility.Visible;
                        break;
                    case DanmakuMode.Top:
                        D_Top.Visibility = Visibility.Visible;
                        break;
                    case DanmakuMode.Buttom:
                        D_Bottom.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (mode)
                {
                    case DanmakuMode.Roll:
                        DanmakuGrid.Visibility = Visibility.Collapsed;
                        break;
                    case DanmakuMode.Top:
                        D_Top.Visibility = Visibility.Collapsed;
                        break;
                    case DanmakuMode.Buttom:
                        D_Bottom.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
        }

        public enum DanmakuMode
        {
            Roll=0,
            Top=1,
            Buttom=2
        }
    }
}
