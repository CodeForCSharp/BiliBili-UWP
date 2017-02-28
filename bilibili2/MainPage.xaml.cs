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
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            ChangeTitbarColor();
            //this.RequestedTheme = ElementTheme.Dark;
        }

        private void Liveinfo_PlayEvent(string aid)
        {
            infoFrame.Navigate(typeof(LiveInfoPage),aid);
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
           
            navInfo = infoFrame.GetNavigationState();
            infoFrame.Tag = (SolidColorBrush)top_grid.Background;
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (await HasMessage())
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    bor_HasMessage.Visibility = Visibility.Visible;
                });
            }
            else
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
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
                    if (list.reply_me != 0||list.chat_me!=0|| list.notify_me!=0)
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
                settings.SetSettingValue("PlayLocal",true);
            }
        }
        //首页错误
        private void Home_Items_ErrorEvent(string aid)
        {
            messShow.Show("读取首页信息失败\r\n" + aid, 3000);
        }
        //首页跳转
        private void Home_Items_PlayEvent(string aid)
        {
            infoFrame.Navigate(typeof(VideoInfoPage), aid);

            //jinr.From = this.ActualWidth;
            //storyboardPopIn.Begin();
        }
        //双击退出
        bool IsClicks = false;
        private async void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (infoFrame.Content != null)
            {
                e.Handled = true;
                if (infoFrame.CanGoBack)
                {
                    // e.Handled = true;
                    infoFrame.GoBack();
                }
                else
                {
                    // e.Handled = true;
                    tuic.To = this.ActualWidth;
                    storyboardPopOut.Begin();
                }
            }
            else
            {
                if (e.Handled == false)
                {
                    if (IsClicks)
                    {
                        Application.Current.Exit();
                    }
                    else
                    {
                        IsClicks = true;
                        e.Handled = true;
                        txt_GG.Text = "再按一次退出程序";
                        grid_GG.Visibility = Visibility.Visible;
                        await Task.Delay(1500);
                        IsClicks = false;
                        grid_GG.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        //打开汉堡菜单
        private void btn_OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sp_View.IsPaneOpen)
            {
                sp_View.IsPaneOpen = false;
            }
            else
            {
                sp_View.IsPaneOpen = true;
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
            if (sp_View.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                if (PaneRoot == null)
                {
                    // 找到 SplitView 控件中，模板的父容器
                    Grid grid = FindVisualChild<Grid>(sp_View);

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
                if (e.Cumulative.Translation.X >= 0 && e.Cumulative.Translation.X < sp_View.OpenPaneLength)
                {
                    CompositeTransform ct = PaneRoot.RenderTransform as CompositeTransform;
                    ct.TranslateX = (e.Cumulative.Translation.X - sp_View.OpenPaneLength);
                }
            }
        }

        private void Border_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;

            // 仅当 SplitView 处于 Overlay 模式时（窗口宽度最小时）
            if (sp_View.DisplayMode == SplitViewDisplayMode.Overlay && PaneRoot != null)
            {
                // 因为当 IsPaneOpen 为 true 时，会通过 VisualStateManager 把 PaneRoot.Visibility  设置为
                // Visibility.Visible，所以这里把它改为 Visibility.Collapsed，以回到初始状态
                PaneRoot.Visibility = Visibility.Collapsed;

                // 恢复初始状态 
                CompositeTransform ct = PaneRoot.RenderTransform as CompositeTransform;


                // 如果大于 MySplitView.OpenPaneLength 宽度的 1/2 ，则显示，否则隐藏
                if ((sp_View.OpenPaneLength + ct.TranslateX) > sp_View.OpenPaneLength / 2)
                {
                    sp_View.IsPaneOpen = true;

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
                            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                            await statusBar.HideAsync();
                        }
                    }
                    break;
                case ApplicationViewOrientation.Portrait:
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(StatusBar).ToString()))
                    {
                        StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                        await statusBar.ShowAsync();
                    }
                    break;
                default:
                    break;
            }

            if (this.ActualWidth < 1000)
            {
                top_txt_Header.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                top_txt_Header.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }
        //打开搜索框
        private void btn_GoFind_Click(object sender, RoutedEventArgs e)
        {
            if (top_txt_find.Visibility == Visibility.Collapsed)
            {
                btn_GoFind.Visibility = Visibility.Collapsed;
                top_txt_find.Visibility = Visibility.Visible;
            }
            else
            {
                btn_GoFind.Visibility = Visibility.Visible;
                top_txt_find.Visibility = Visibility.Collapsed;
            }

        }
        //子页面后退
        private void MainPage_BackEvent()
        {
            tuic.To = this.ActualWidth;
            storyboardPopOut.Begin();
        }
        //子页面后退动画完成
        private void StoryboardPopOut_Completed(object sender, object e)
        {
            infoFrame.ContentTransitions = null;
            infoFrame.Content = null;
            infoFrame.SetNavigationState(navInfo);
            //infoFrame.CacheSize = 0;
            //int i=  infoFrame.BackStackDepth;
            //string a = string.Empty;
            dh.TranslateX = 0;
        }

        WebClientClass wc = new WebClientClass();

        //用户登录或跳转
        private void btn_UserInfo_Click(object sender, RoutedEventArgs e)
        {
            if (txt_UserName.Text == "请登录")
            {
                infoFrame.Navigate(typeof(LoginPage));

                //jinr.From = this.ActualWidth;
                //storyboardPopIn.Begin();
            }
            else
            {
                infoFrame.Navigate(typeof(UserInfoPage));
            }
            //this.Frame.Navigate(typeof(LoginPage));
        }
        //用户登录成功，读取用户信息
        private void MainPage_LoginEd()
        {
            GetLoadInfo();
        }
        //读取用户信息
        private async void GetLoadInfo()
        {
            UserClass getLogin = new UserClass();
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
                            txt_UserName.Text = model.name;
                            txt_Sign.Visibility = Visibility.Visible;
                            txt_Sign.Text = model.RankStr;
                            img_user.ImageSource = new BitmapImage(new Uri(model.face));
                        }
                        messShow.Show(result, 3000);
                    }
                    else
                    {
                        txt_UserName.Text = "请登录";
                        txt_Sign.Visibility = Visibility.Collapsed;
                        img_user.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/other/NoAvatar.png"));
                    }
                }
                else
                {
                    container.Values["UserName"] = "";
                    container.Values["UserPass"] = "";
                    container.Values["AutoLogin"] = "";
                    txt_UserName.Text = "请登录";
                    txt_Sign.Visibility = Visibility.Collapsed;

                    img_user.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/other/NoAvatar.png"));
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
                    txt_UserName.Text = model.name;
                    txt_Sign.Visibility = Visibility.Visible;
                    txt_Sign.Text = model.RankStr;
                    img_user.ImageSource = new BitmapImage(new Uri(model.face));
                }
            }
        }
        //汉堡菜单的点击
        private void list_Menu_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((e.ClickedItem as StackPanel).Tag == null)
            {
                return;
            }
            bool isLogin = new UserClass().IsLogin();
            switch ((e.ClickedItem as StackPanel).Tag.ToString())
            {
                case "M_Drak_Light":
                    if (RequestedTheme== ElementTheme.Dark)
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
                        infoFrame.Navigate(typeof(FavPage));
                    }
                    else
                    {
                        messShow.Show("请先登录！", 3000);
                    }
                    break;
                case "History":
                    if (isLogin)
                    {
                        infoFrame.Navigate(typeof(HistoryPage));
                    }
                    else
                    {
                        messShow.Show("请先登录！", 3000);
                    }
                    break;
                case "Message":
                    if (isLogin)
                    {
                        infoFrame.Navigate(typeof(MessagePage));
                    }
                    else
                    {
                        messShow.Show("请先登录！", 3000);
                    }
                    break;
                case "Download":
                    infoFrame.Navigate(typeof(DownloadPage));
                    break;
                case "Setting":
                    infoFrame.Navigate(typeof(SettingPage));
                    break;
                case "Feedback":
                    break;
                default:
                    break;
            }

        }
        //触发主题改变
        private void MainPage_ChangeDrak()
        {
            ChangeDrak();
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
            tuic.To = this.ActualWidth;
            storyboardPopOut.Begin();
            ChangeTitbarColor();
        }
        private void ChangeTitbarColor()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                // StatusBar.GetForCurrentView().HideAsync();
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
                statusBar.BackgroundOpacity = 100;
            }
            //电脑标题栏颜色
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
            titleBar.ForegroundColor = Color.FromArgb(255, 254, 254, 254);//Colors.White纯白用不了。。。
            titleBar.ButtonHoverBackgroundColor = ((SolidColorBrush)menu_DarkBack.Background).Color;
            titleBar.ButtonBackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
            titleBar.ButtonForegroundColor = Color.FromArgb(255, 254, 254, 254);
            titleBar.InactiveBackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
            titleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
            infoFrame.Tag = (SolidColorBrush)top_grid.Background;
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

        //打开话题
        private void Find_btn_Topic_Click(object sender, RoutedEventArgs e)
        {
                infoFrame.Navigate(typeof(TopicPage));
        }
        //infoFrame跳转
        private void infoFrame_Navigated(object sender, NavigationEventArgs e)
        {
            switch ((e.Content as Page).Tag.ToString())
            {
                case "视频信息":
                    (infoFrame.Content as VideoInfoPage).BackEvent += MainPage_BackEvent;
                    break;
                case "网页浏览":
                    (infoFrame.Content as WebViewPage).BackEvent += MainPage_BackEvent;
                    break;
                case "登录":
                    (infoFrame.Content as LoginPage).BackEvent += MainPage_BackEvent;
                    (infoFrame.Content as LoginPage).LoginEd += MainPage_LoginEd;
                    break;
                case "话题":
                    (infoFrame.Content as TopicPage).BackEvent += MainPage_BackEvent;
                    break;
                case "排行榜":
                    (infoFrame.Content as RankPage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧信息":
                    (infoFrame.Content as BanInfoPage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧更新时间表":
                    (infoFrame.Content as BanTimelinePage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧索引":
                    (infoFrame.Content as BanTagPage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧Tag":
                    (infoFrame.Content as BanByTagPage).BackEvent += MainPage_BackEvent;
                    break;
                case "全部追番":
                    (infoFrame.Content as UserBangumiPage).BackEvent += MainPage_BackEvent;
                    break;
                case "用户中心":
                    (infoFrame.Content as UserInfoPage).BackEvent += MainPage_BackEvent;
                    (infoFrame.Content as UserInfoPage).ExitEvent += MainPage_ExitEvent;
                    break;
                case "查看评论":
                    (infoFrame.Content as CommentPage).BackEvent += MainPage_BackEvent;
                    break;
                case "搜索结果":
                    (infoFrame.Content as SearchPage).BackEvent += MainPage_BackEvent;
                    break;
                case "收藏夹":
                    (infoFrame.Content as FavPage).BackEvent += MainPage_BackEvent;
                    break;
                case "设置":
                    (infoFrame.Content as SettingPage).BackEvent += MainPage_BackEvent;
                    (infoFrame.Content as SettingPage).ChangeTheme += MainPage_ChangeTheme;
                    (infoFrame.Content as SettingPage).ChangeDrak += MainPage_ChangeDrak;
                    break;
                case "播放器":
                    (infoFrame.Content as PlayerPage).BackEvent += MainPage_BackEvent;
                    break;
                case "历史":
                    (infoFrame.Content as HistoryPage).BackEvent += MainPage_BackEvent;
                    break;
                case "消息中心":
                    (infoFrame.Content as MessagePage).BackEvent += MainPage_BackEvent;
                    break;
                case "全部直播":
                    (infoFrame.Content as AllLivePage).BackEvent += MainPage_BackEvent;
                    break;
                case "直播间":
                    (infoFrame.Content as LiveInfoPage).BackEvent += MainPage_BackEvent;
                    break;
                case "搜索直播":
                    (infoFrame.Content as SearchLivePage).BackEvent += MainPage_BackEvent;
                    break;
                case "离线管理":
                    (infoFrame.Content as DownloadPage).BackEvent += MainPage_BackEvent;
                    break;
                case "编辑资料":
                    (infoFrame.Content as EditPage).BackEvent += MainPage_BackEvent;
                    break;
                default:
                    break;
            }
        }

        private void MainPage_ExitEvent()
        {
            tuic.To = this.ActualWidth;
            storyboardPopOut.Begin();
            GetLoadInfo();
        }

        //主题更换
        private void MainPage_ChangeTheme()
        {
            ChangeTheme();
        }
        //infoFrame跳转动画
        private void infoFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            dh.TranslateX = 0;
            EdgeUIThemeTransition edge = new EdgeUIThemeTransition();
            if (e.NavigationMode == NavigationMode.New)
            {
                edge.Edge = EdgeTransitionLocation.Right;
                TransitionCollection tc = new TransitionCollection();
                tc.Add(edge);
                infoFrame.ContentTransitions = tc;
            }
        }

        public async Task<ObservableCollection<String>> GetSugges(string text)
        {
            try
            {
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://s.search.bilibili.com/main/suggest?suggest_type=accurate&sub_type=tag&main_ver=v1&term=" + text));
                JObject json = JObject.Parse(results);
                // json["result"]["tag"].ToString();
                List<SuggesModel> list = JsonConvert.DeserializeObject<List<SuggesModel>>(json["result"]["tag"].ToString());
                ObservableCollection<String> suggestions = new ObservableCollection<string>();
                foreach (SuggesModel item in list)
                {
                    suggestions.Add(item.value);
                }
                return suggestions;
            }
            catch (Exception)
            {
                return new ObservableCollection<string>();
            }

        }
        public class SuggesModel
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        private async void top_txt_find_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text.Length != 0)
            {
                sender.ItemsSource = await GetSugges(sender.Text);
            }
            else
            {
                sender.ItemsSource = null;
            }
        }

        private void top_txt_find_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender.Text.Length == 0)
            {
                //top_txt_find.Visibility = Visibility.Collapsed;
                //top_btn_find.Visibility = Visibility.Visible;
                //mainFrame.Navigate(typeof(SeasonPage));
            }
            else
            {
                infoFrame.Navigate(typeof(SearchPage), top_txt_find.Text);
            }
        }

        private void top_txt_find_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            top_txt_find.Text = args.SelectedItem as string;
        }
    }
}
