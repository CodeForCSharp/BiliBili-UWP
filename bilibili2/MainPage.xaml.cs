using bilibili2.Class;
using bilibili2.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace bilibili2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        Frame rootFrame = (Window.Current.Content as Frame);

        public MainPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            PivotHome.SelectedIndex = 1;
            ChangeTitbarColor();
        }

        string navInfo = string.Empty;
        private SettingHelper settings = new SettingHelper();
        DispatcherTimer timer = new DispatcherTimer();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                GetLoadInfo();
                timer.Interval = new TimeSpan(0, 0, 5);
                timer.Start();
                timer.Tick += Timer_Tick;
            }
            GetSetting();
            ChangeTheme();
            ChangeDrak();
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (await HasMessage())
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    bor_HasMessage.Visibility = Visibility.Visible;
                });
            }
            else
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    bor_HasMessage.Visibility = Visibility.Collapsed;
                });
            }
        }

        private async Task<bool> HasMessage()
        {
            try
            {
                wc = new WebClientClass();
                // http://message.bilibili.com/api/msg/query.room.list.do?access_key=a36a84cc8ef4ea2f92c416951c859a25&actionKey=appkey&appkey=c1b107428d337928&build=414000&page_size=100&platform=android&ts=1461404884000&sign=5e212e424761aa497a75b0fb7fbde775
                string url = string.Format("http://message.bilibili.com/api/notify/query.notify.count.do?_device=wp&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=434000&platform=android&ts={2}", ApiHelper.access_key, ApiHelper._appKey, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                MessageModel model = JsonConvert.DeserializeObject<MessageModel>(results);
                if (model.code == 0)
                {
                    MessageModel list = JsonConvert.DeserializeObject<MessageModel>(model.data.ToString());
                    if (list.reply_me != 0 || list.chat_me != 0 || list.notify_me != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
                //messShow.Show("读取通知失败", 3000);
            }
        }

        private void GetSetting()
        {
            if (!settings.SettingContains("PlayLocal"))
            {
                settings.SetSettingValue("PlayLocal", true);
            }
        }

        //侧滑来源http://www.cnblogs.com/hebeiDGL/p/4775377.html
        #region  从屏幕左侧边缘滑动屏幕时，打开 SplitView 菜单

        // SplitView 控件模板中，Pane部分的 Grid
        Grid PaneRoot;

        //  引用 SplitView 控件中， 保存从 Pane “关闭” 到“打开”的 VisualTransition
        //  也就是 <VisualTransition From="Closed" To="OpenOverlayLeft"> 这个 
        VisualTransition from_ClosedToOpenOverlayLeft_Transition;

        private void Border_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;

            // 仅当 SplitView 处于 Overlay 模式时（窗口宽度最小时）
            if (SpView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                if (PaneRoot == null)
                {
                    // 找到 SplitView 控件中，模板的父容器
                    Grid grid = FindVisualChild<Grid>(SpView);

                    PaneRoot = grid.FindName("PaneRoot") as Grid;

                    if (from_ClosedToOpenOverlayLeft_Transition == null)
                    {
                        // 获取 SplitView 模板中“视觉状态集合”
                        IList<VisualStateGroup> stateGroup = VisualStateManager.GetVisualStateGroups(grid);

                        //  获取 VisualTransition 对象的集合。
                        IList<VisualTransition> transitions = stateGroup[0].Transitions;

                        // 找到 SplitView.IsPaneOpen 设置为 true 时，播放的 transition
                        from_ClosedToOpenOverlayLeft_Transition = transitions?.Where(train => train.From == "Closed" && train.To == "OpenOverlayLeft").First();
                    }
                }


                // 默认为 Collapsed，所以先显示它
                PaneRoot.Visibility = Visibility.Visible;

                // 当在 Border 上向右滑动，并且滑动的总距离需要小于 Panel 的默认宽度。否则会脱离左侧窗口，继续向右拖动
                if (e.Cumulative.Translation.X >= 0 && e.Cumulative.Translation.X < SpView.OpenPaneLength)
                {
                    CompositeTransform ct = PaneRoot.RenderTransform as CompositeTransform;
                    ct.TranslateX = (e.Cumulative.Translation.X - SpView.OpenPaneLength);
                }
            }
        }

        private void Border_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;

            // 仅当 SplitView 处于 Overlay 模式时（窗口宽度最小时）
            if (SpView.DisplayMode == SplitViewDisplayMode.Overlay && PaneRoot != null)
            {
                // 因为当 IsPaneOpen 为 true 时，会通过 VisualStateManager 把 PaneRoot.Visibility  设置为
                // Visibility.Visible，所以这里把它改为 Visibility.Collapsed，以回到初始状态
                PaneRoot.Visibility = Visibility.Collapsed;

                // 恢复初始状态 
                CompositeTransform ct = PaneRoot.RenderTransform as CompositeTransform;


                // 如果大于 MySplitView.OpenPaneLength 宽度的 1/2 ，则显示，否则隐藏
                if ((SpView.OpenPaneLength + ct.TranslateX) > SpView.OpenPaneLength / 2)
                {
                    SpView.IsPaneOpen = true;

                    // 因为上面设置 IsPaneOpen = true 会再次播放向右滑动的动画，所以这里使用 SkipToFill()
                    // 方法，直接跳到动画结束状态
                    from_ClosedToOpenOverlayLeft_Transition?.Storyboard?.SkipToFill();

                }

                ct.TranslateX = 0;
            }
        }


        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            int count = Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }
        #endregion
        //页面大小改变
        private async void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!settings.SettingContains("HideTitleBar"))
            {
                settings.SetSettingValue("HideTitleBar", true);
            }
            ApplicationView av = ApplicationView.GetForCurrentView();
            switch (av.Orientation)
            {
                case ApplicationViewOrientation.Landscape:
                    if ((bool)settings.GetSettingValue("HideTitleBar"))
                    {
                        if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(StatusBar).ToString()))
                        {
                            StatusBar statusBar = StatusBar.GetForCurrentView();
                            await statusBar.HideAsync();
                        }
                    }
                    break;
                case ApplicationViewOrientation.Portrait:
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(StatusBar).ToString()))
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        await statusBar.ShowAsync();
                    }
                    break;
                default:
                    break;
            }
        }
        UserClass getLogin = new UserClass();
        WebClientClass wc = new WebClientClass();

        //用户登录成功，读取用户信息
        private void MainPage_LoginEd()
        {
            GetLoadInfo();
        }
        //读取用户信息
        private async void GetLoadInfo()
        {
            if (!getLogin.IsLogin())
            {
                //设置是否存在
                if (container.Values["UserName"] != null && container.Values["UserPass"] != null && container.Values["AutoLogin"] != null)
                {
                    //用户名、密码是否为空
                    if (container.Values["AutoLogin"].ToString() == "true" && container.Values["UserName"].ToString() != "" && container.Values["UserPass"].ToString() != "")
                    {
                        //读取登录结果
                        string result = await ApiHelper.LoginBilibili(container.Values["UserName"].ToString(), container.Values["UserPass"].ToString());
                        GetLoginInfoModel model = await getLogin.GetUserInfo();
                        if (model != null)
                        {
                            TxtUserName.Text = model.name;
                            TxtSign.Visibility = Visibility.Visible;
                            TxtSign.Text = model.RankStr;
                            ImgUser.ImageSource = new BitmapImage(new Uri(model.face));
                        }
                        MessageShow.Show(result, 3000);
                    }
                    else
                    {
                        TxtUserName.Text = "请登录";
                        TxtSign.Visibility = Visibility.Collapsed;
                        ImgUser.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/other/NoAvatar.png"));
                    }
                }
                else
                {
                    container.Values["UserName"] = "";
                    container.Values["UserPass"] = "";
                    container.Values["AutoLogin"] = "";
                    TxtUserName.Text = "请登录";
                    TxtSign.Visibility = Visibility.Collapsed;
                    ImgUser.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/other/NoAvatar.png"));
                }
            }
            else
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
                ApiHelper.access_key = await FileIO.ReadTextAsync(file);
                GetLoginInfoModel model = await getLogin.GetUserInfo();
                if (model != null)
                {
                    TxtUserName.Text = model.name;
                    TxtSign.Visibility = Visibility.Visible;
                    TxtSign.Text = model.RankStr;
                    ImgUser.ImageSource = new BitmapImage(new Uri(model.face));
                }
            }
        }

        //改变主题
        private void ChangeTheme()
        {
            string ThemeName = string.Empty;
            if (settings.SettingContains("Theme"))
            {
                ThemeName = settings.GetSettingValue("Theme") as string;
            }
            else
            {
                ThemeName = "Pink";
                settings.SetSettingValue("Theme", "Pink");
            }
            ResourceDictionary newDictionary = new ResourceDictionary();
            switch (ThemeName)
            {
                case "Red":
                    newDictionary.Source = new Uri("ms-appx:///Theme/RedTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Blue":
                    newDictionary.Source = new Uri("ms-appx:///Theme/BlueTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Green":
                    newDictionary.Source = new Uri("ms-appx:///Theme/GreenTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Pink":
                    newDictionary.Source = new Uri("ms-appx:///Theme/PinkTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Purple":
                    newDictionary.Source = new Uri("ms-appx:///Theme/PurpleTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Yellow":
                    newDictionary.Source = new Uri("ms-appx:///Theme/YellowTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                default:
                    break;
            }
            ChangeTitbarColor();
        }
        private void ChangeTitbarColor()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundOpacity = 100;
            }
            //电脑标题栏颜色
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
        }
        private void ChangeDrak()
        {
            if (!settings.SettingContains("Drak"))
            {
                settings.SetSettingValue("Drak", false);
            }
            if ((bool)settings.GetSettingValue("Drak"))
            {
                RequestedTheme = ElementTheme.Dark;
                txt_D_L.Text = "日间模式";
                font_D_L.Glyph = "\uE706";
            }
            else
            {
                RequestedTheme = ElementTheme.Light;
                txt_D_L.Text = "夜间模式";
                font_D_L.Glyph = "\uE708";
            }
            ChangeTitbarColor();
        }

        //主题更换
        private void MainPage_ChangeTheme()
        {
            ChangeTheme();
        }

        private void ListMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tag = (e.ClickedItem as StackPanel).Tag as string;
            if (string.IsNullOrEmpty(tag)) return;
            bool isLogin = getLogin.IsLogin();
            switch (tag)
            {
                case "M_Drak_Light":
                    if (RequestedTheme == ElementTheme.Dark)
                    {
                        settings.SetSettingValue("Drak", false);
                        txt_D_L.Text = "夜间模式";
                        font_D_L.Glyph = "\uE708";
                        RequestedTheme = ElementTheme.Light;
                    }
                    else
                    {
                        settings.SetSettingValue("Drak", true);
                        txt_D_L.Text = "日间模式";
                        font_D_L.Glyph = "\uE706";
                        RequestedTheme = ElementTheme.Dark;
                    }
                    ChangeTitbarColor();
                    break;
                case "Favbox":
                    if (isLogin)
                    {
                        Frame.Navigate(typeof(FavPage));
                    }
                    else
                    {
                        MessageShow.Show("请先登录！", 3000);
                    }
                    break;
                case "History":
                    if (isLogin)
                    {
                        Frame.Navigate(typeof(HistoryPage));
                    }
                    else
                    {
                        MessageShow.Show("请先登录！", 3000);
                    }
                    break;
                case "Message":
                    if (isLogin)
                    {
                        Frame.Navigate(typeof(MessagePage));
                    }
                    else
                    {
                        MessageShow.Show("请先登录！", 3000);
                    }
                    break;
                case "Download":
                    Frame.Navigate(typeof(DownloadPage));
                    break;
                case "Setting":
                    Frame.Navigate(typeof(SettingPage));
                    break;
                default:
                    break;
            }
        }

        private void BtnUserInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!getLogin.IsLogin())
            {
                Frame.Navigate(typeof(LoginPage));
            }
            else
            {
                Frame.Navigate(typeof(UserInfoPage));
            }
        }

        private void BtnOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            SpView.IsPaneOpen = !SpView.IsPaneOpen;
        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
