using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace bilibili2.Controls
{
    public sealed class BilibiliLoadControl : UserControl
    {
        private Image image = new Image
        {
            Source = new BitmapImage(new Uri("ms-appx:///Assets/other/bili_anim_tv_chan_1.png"))
        };
        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval=new TimeSpan(0,0,0,0,100)
        };
        private Random random =new Random();
        private List<BitmapImage> pngList;
        public BilibiliLoadControl()
        {
            Content = image;
            pngList = Enumerable.Range(1, 6).Select(i => new BitmapImage(new Uri($"ms-appx:///Assets/other/bili_anim_tv_chan_{i}.png"))).ToList();
            timer.Tick += (o,e) =>
            {
                image.Source = pngList[random.Next(0, 6)];
            };
            timer.Start();
        }
    }
}
