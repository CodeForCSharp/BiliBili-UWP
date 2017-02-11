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
        public bool IsPlaying = true;
        public MediaPlaybackState state;
        private int row = 0;//行数
        private int maxRow = 10;
        /// <summary>
        /// 添加滚动弹幕
        /// </summary>
        /// <param name="model">弹幕参数</param>
        /// <param name="Myself">是否自己发送的</param>
        public void AddFloatDanmaku(DanmakuViewModel model, bool Myself)
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
                var moveTransform = new TranslateTransform
                {
                    X = grid_Danmu.ActualWidth
                };
                tx.RenderTransform = moveTransform;
                //将弹幕加载入控件中,并且设置位置
                grid_Danmu.Children.Add(tx);
                Grid.SetRow(tx, row);
                row++;
                if (row == maxRow)
                {
                    row = 0;
                }
                //if (Myself)
                //{
                //    tx.BorderThickness = new Thickness(2);
                //    tx.BorderBrush = new SolidColorBrush(Colors.Gray);
                //}
                //更新弹幕UI，不更新无法获得弹幕的ActualWidth
                tx.UpdateLayout();
                //创建动画
                Duration duration = new Duration(TimeSpan.FromSeconds(Speed));
                DoubleAnimation myDoubleAnimationX = new DoubleAnimation
                {
                    Duration = duration,
                    To = -(tx.ActualWidth),//到达
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
                //grid_Danmu.Resources.Remove("justintimeStoryboard");
                //grid_Danmu.Resources.Add("justintimeStoryboard", justintimeStoryboard);
                justintimeStoryboard.Begin();
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = new TimeSpan(0, 0, 0, 1)
                };
                int i = 0;
                timer.Tick += async (sender, args) =>
                {
                    if (!IsPlaying)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            justintimeStoryboard.Pause();
                        });
                    }
                    else
                    {
                        if (i == Speed * 2)
                        {
                            grid_Danmu.Children.Remove(tx);
                        }
                        i++;
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            justintimeStoryboard.Resume();
                        });
                    }
                };
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 清除全部弹幕
        /// </summary>
        public void ClearDanmu()
        {
            row = 0;
            grid_Danmu.Children.Clear();
            grid_Danmu2.Children.Clear();
        }

        private bool Handling = false;//是否正在监听
        /// <summary>
        /// 添加顶部及底部弹幕
        /// </summary>
        /// <param name="model">弹幕参数</param>
        /// <param name="istop">是否顶部</param>
        /// <param name="Myself">是否自己发送的</param>
        public async void AddTopButtomDanmu(DanmakuViewModel model, bool istop,bool Myself)
        {
            TextBlock tx = new TextBlock();
            TextBlock tx2 = new TextBlock();
            Grid grid = new Grid();
            if (fontFamily != "默认")
            {
                tx.FontFamily = new FontFamily(fontFamily);
                tx2.FontFamily = new FontFamily(fontFamily);
            }

                tx2.Text =model.Text;
                tx.Text = model.Text ;
            tx2.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            tx.Foreground = model.Color;//new SolidColorBrush(co[rd.Next(0, 7)]);
            double size = model.Size;
            if (size == 25)
            {
                tx2.FontSize = fontSize;
                tx.FontSize = fontSize;
            }
            else
            {
                tx2.FontSize = fontSize - 2;
                tx.FontSize = fontSize - 2;
            }
            //grid包含弹幕文本信息
            grid.Children.Add(tx2);
            grid.Children.Add(tx);

            // tx.FontSize = Double.Parse(model.DanSize) - fontSize;
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Top;
            tx2.Margin = new Thickness(1);
            if (Myself)
            {
                grid.BorderThickness = new Thickness(2);
                grid.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            grid.Opacity = Tran;
            grid.DataContext = model;
            grid.UpdateLayout();

            if (istop)
            {
                D_Top.Children.Add(grid);
                await Task.Delay(5000);
                if (state== MediaPlaybackState.Paused)
                {
                    ClearTopButtomDanmuku();
                }
                else
                {
                    D_Top.Children.Remove(grid);
                }
            }
            else
            {
                D_Bottom.Children.Add(grid);
                await Task.Delay(5000);
                if (state == MediaPlaybackState.Paused)
                {
                    ClearTopButtomDanmuku();
                }
                else
                {
                    D_Bottom.Children.Remove(grid);
                }
            }
        }
        /// <summary>
        /// 清除顶部及底部弹幕
        /// </summary>
        private async void ClearTopButtomDanmuku()
        {
            //一定要检查是否正在循环，多个while死循环会爆CPU
            if (!Handling)
            {
                Handling = true;
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsPlaying)
                        {
                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                D_Bottom.Children.Clear();
                                D_Top.Children.Clear();
                            });
                            break;
                        }
                        //循环速度不要太快，否则也会爆CPU
                        await Task.Delay(200);
                    }
                });
                Handling = false;
            }
        }
        /// <summary>
        /// 设置弹幕上下间距
        /// </summary>
        /// <param name="value"></param>
        public void SetSpacing(double value)
        {
            Jianju.Height = new GridLength(value, GridUnitType.Pixel);
        }
        /// <summary>
        /// 读取弹幕屏幕中的弹幕
        /// </summary>
        /// <returns></returns>
        public List<DanmakuViewModel> GetScreenDanmu()
        {
            List<DanmakuViewModel> list = new List<DanmakuViewModel>();
            foreach (Grid item in D_Top.Children)
            {
                list.Add(item.DataContext as DanmakuViewModel);
            }
            foreach (Grid item in D_Bottom.Children)
            {
                list.Add(item.DataContext as DanmakuViewModel);
            }
            foreach (Grid item in grid_Danmu.Children)
            {
                list.Add(item.DataContext as DanmakuViewModel);
            }
            return list;
        }
        /// <summary>
        /// 移除当前屏幕中的弹幕
        /// </summary>
        public void RemoveDanmu(DanmakuViewModel model)
        {
            foreach (Grid item in grid_Danmu.Children)
            {
                if (item.DataContext== model)
                {
                    grid_Danmu.Children.Remove(item);
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
        /// <param name="IsVisible">是否可见</param>
        /// <param name="mode">模式</param>
        public void SetDanmuVisibility(bool IsVisible, DanmakuMode mode)
        {
            if (IsVisible)
            {
                switch (mode)
                {
                    case DanmakuMode.Roll:
                        grid_Danmu.Visibility = Visibility.Visible;
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
                        grid_Danmu.Visibility = Visibility.Collapsed;
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
