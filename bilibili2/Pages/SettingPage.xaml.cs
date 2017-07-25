﻿using bilibili2.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Display;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public event GoBackHandler ChangeTheme;
        public event GoBackHandler ChangeDrak;
        public event GoBackHandler Feedback;
        public SettingPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        private SettingHelper settings = new SettingHelper();
        private DisplayRequest dispRequest = null;
        private void btn_back_Click(object sender, RoutedEventArgs e)
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetSetting();
        }
        bool Geting = true;
        private void GetSetting()
        {
            txt_Ver.Text = settings.GetVersion();
            string device = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
            if (!settings.SettingContains("Theme"))
            {
                settings.SetSettingValue("Theme", "Pink");
            }
            switch (settings.GetSettingValue("Theme").ToString())
            {
                case "Red":
                    cb_Theme.SelectedIndex = 1;
                    break;
                case "Blue":
                    cb_Theme.SelectedIndex = 4;
                    break;
                case "Green":
                    cb_Theme.SelectedIndex = 3;
                    break;
                case "Pink":
                    cb_Theme.SelectedIndex = 0;
                    break;
                case "Purple":
                    cb_Theme.SelectedIndex = 5;
                    break;
                case "Yellow":
                    cb_Theme.SelectedIndex = 2;
                    break;
                default:
                    break;
            }
            Geting = false;
            if (settings.SettingContains("UpdateCT"))
            {
                tw_CT.IsOn =(bool)settings.GetSettingValue("UpdateCT");
            }
            else
            {
                tw_CT.IsOn = true;
            }

            if (settings.SettingContains("Drak"))
            {
                tw_Drak.IsOn = (bool)settings.GetSettingValue("Drak");
            }
            else
            {
                tw_Drak.IsOn = false;
            }

            if (settings.SettingContains("HideTitleBar"))
            {
                tw_HideStatusBar.IsOn = (bool)settings.GetSettingValue("HideTitleBar");
            }
            else
            {
                tw_HideStatusBar.IsOn = true;
            }

            if (settings.SettingContains("PlayLocal"))
            {
                tw_PlayLocal.IsOn = (bool)settings.GetSettingValue("PlayLocal");
            }
            else
            {
                tw_PlayLocal.IsOn = true;
            }


            if (settings.SettingContains("UseWifi"))
            {
                sw_UseWifi.IsOn = (bool)settings.GetSettingValue("UseWifi");
            }
            else
            {
                sw_UseWifi.IsOn = false;
            }

            if (settings.SettingContains("HoldLight"))
            {
                sw_Light.IsOn = (bool)settings.GetSettingValue("HoldLight");
            }
            else
            {
                sw_Light.IsOn = false;
            }


            cb_Quality.SelectedIndex = settings.SettingContains("Quality") ? int.Parse( settings.GetSettingValue("Quality").ToString()) : 1;

            if (settings.SettingContains("AutoPlay"))
            {
                tw_AutoPlay.IsOn = (bool)settings.GetSettingValue("AutoPlay");
            }
            else
            {
                settings.SetSettingValue("AutoPlay",false);
                tw_AutoPlay.IsOn = false;
            }

            //弹幕字体
            if (settings.SettingContains("FontFamily"))
            {
                switch ((string)settings.GetSettingValue("FontFamily"))
                {
                    case "默认":
                        cb_Font.SelectedIndex = 0;
                        break;
                    case "雅黑":
                        cb_Font.SelectedIndex = 1;
                        break;
                    case "黑体":
                        cb_Font.SelectedIndex = 2;
                        break;
                    case "楷体":
                        cb_Font.SelectedIndex = 3;
                        break;
                    case "宋体":
                        cb_Font.SelectedIndex = 4;
                        break;
                    case "等线":
                        cb_Font.SelectedIndex = 5;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                settings.SetSettingValue("FontFamily", "默认");
                cb_Font.SelectedIndex = 0;
            }

            if (settings.SettingContains("Full"))
            {
                 tw_AutoFull.IsOn = bool.Parse( settings.GetSettingValue("Full").ToString());
            }
            else
            {
                tw_AutoFull.IsOn = device == "Windows.Mobile";
            }

            slider_DanmuJianju.Value = settings.SettingContains("DanmuJianju") ? double.Parse( settings.GetSettingValue("DanmuJianju").ToString()) : 0;

            slider_DanmuTran.Value = settings.SettingContains("DanmuTran") ? double.Parse(settings.GetSettingValue("DanmuTran").ToString()) : 100;

            slider_DanmuSpeed.Value = settings.SettingContains("DanmuSpeed") ? double.Parse(settings.GetSettingValue("DanmuSpeed").ToString()) : 12;

            if (settings.SettingContains("DanmuSize"))
            {
                slider_DanmuSize.Value = double.Parse( settings.GetSettingValue("DanmuSize").ToString());
            }
            else
            {
                slider_DanmuSize.Value = device == "Windows.Mobile" ? 16 : 22;
            }
        }

        private void cb_Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Theme.SelectedItem!=null&&!Geting)
            {
                switch (cb_Theme.SelectedIndex)
                {
                    case 0:
                        settings.SetSettingValue("Theme", "Pink");
                        break;
                    case 1:
                        settings.SetSettingValue("Theme", "Red");
                        break;
                    case 2:
                        settings.SetSettingValue("Theme", "Yellow");
                        break;
                    case 3:
                        settings.SetSettingValue("Theme", "Green");
                        break;
                    case 4:
                        settings.SetSettingValue("Theme", "Blue");
                        break;
                    case 5:
                        settings.SetSettingValue("Theme", "Purple");
                        break;
                    default:
                        break;
                }
                ChangeTheme?.Invoke();
            }

        }

        private void tw_CT_Toggled(object sender, RoutedEventArgs e)
        {
            if (tw_CT.IsOn)
            {
                settings.SetSettingValue("UpdateCT",true);
            }
            else
            {
                settings.SetSettingValue("UpdateCT", false);
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
            }

        }

        private void tw_HideStatusBar_Toggled(object sender, RoutedEventArgs e)
        {
            settings.SetSettingValue("HideTitleBar", tw_HideStatusBar.IsOn);
        }

        private void cb_Quality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.SetSettingValue("Quality", cb_Quality.SelectedIndex);
            
        }

        private void tw_AutoFull_Toggled(object sender, RoutedEventArgs e)
        {
            settings.SetSettingValue("Full", tw_AutoFull.IsOn);
        }

        private void tw_AutoPlay_Toggled(object sender, RoutedEventArgs e)
        {
            settings.SetSettingValue("AutoPlay", tw_AutoPlay.IsOn);
        }

        private void slider_DanmuSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            settings.SetSettingValue("DanmuSize", slider_DanmuSize.Value);
        }

        private void slider_DanmuSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            settings.SetSettingValue("DanmuSpeed", slider_DanmuSpeed.Value);
        }

        private void slider_DanmuJianju_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            settings.SetSettingValue("DanmuJianju", slider_DanmuJianju.Value);
        }

        private void slider_DanmuTran_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            settings.SetSettingValue("DanmuTran", slider_DanmuTran.Value);
        }

        private void slider_DanmuFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string a = (cb_Font.SelectedItem as ComboBoxItem).Content.ToString();
            settings.SetSettingValue("FontFamily", a);

        }

        private void tw_Drak_Toggled(object sender, RoutedEventArgs e)
        {
            settings.SetSettingValue("Drak", tw_Drak.IsOn);
            ChangeDrak();
        }

        private void btn_DeleteGuanjianzi_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list_Guanjianzi.SelectedItems)
            {
                string b = (string)settings.GetSettingValue("Guanjianzi");
                list_Guanjianzi.Items.Remove(item);
                settings.SetSettingValue("Guanjianzi", b.Replace("|" + item, string.Empty));
            }
        }

        private void btn_GetGuanjianzi_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_OpenGuanjianzi_Click(object sender, RoutedEventArgs e)
        {
            dan_Sp.IsPaneOpen = true;
            grid_Guanjianzi.Visibility = Visibility.Visible;
            grid_Yonghu.Visibility = Visibility.Collapsed;
            if (settings.SettingContains("Guanjianzi") )
            {
                string a = (string)settings.GetSettingValue("Guanjianzi");
                if (a.Length != 0)
                {
                    list_Guanjianzi.Items.Clear();
                    foreach (var item in a.Split('|').ToList())
                    {
                        list_Guanjianzi.Items.Add(item);
                    }
                    list_Guanjianzi.Items.Remove(string.Empty);
                }
            }
            else
            {
                settings.SetSettingValue("Guanjianzi", string.Empty);
            }
        }

        private void btn_OpenYonghu_Click(object sender, RoutedEventArgs e)
        {
            dan_Sp.IsPaneOpen = true;
            grid_Guanjianzi.Visibility = Visibility.Collapsed;
            grid_Yonghu.Visibility = Visibility.Visible;
            if (settings.SettingContains("Yonghu"))
            {
                string a = (string)settings.GetSettingValue("Yonghu");
                if (a.Length != 0)
                {
                    list_Yonghu.Items.Clear();
                    foreach (var item in a.Split('|').ToList())
                    {
                        list_Yonghu.Items.Add(item);
                    }
                    list_Yonghu.Items.Remove(string.Empty);
                }
            }
            else
            {
                settings.SetSettingValue("Yonghu", string.Empty);
            }
        }

        private void btn_DeleteYonghu_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list_Yonghu.SelectedItems)
            {
                string b = (string)settings.GetSettingValue("Yonghu") ;
                list_Yonghu.Items.Remove(item);
                settings.SetSettingValue("Yonghu", b.Replace("|" + item, string.Empty));
            }
        }

        private void btn_GetYonghu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_AddYonghu_Click(object sender, RoutedEventArgs e)
        { 
              string b = (string)settings.GetSettingValue("Yonghu") + "|" + txt_Yonghu.Text;
               settings.SetSettingValue("Yonghu", b);
               list_Yonghu.Items.Add(txt_Yonghu.Text);
               txt_Yonghu.Text = string.Empty;
        }

        private void btn_AddGuanjianzi_Click(object sender, RoutedEventArgs e)
        {
            string b = (string)settings.GetSettingValue("Guanjianzi") + "|" + txt_Guanjianzi.Text;
            settings.SetSettingValue("Guanjianzi", b);
            list_Yonghu.Items.Add(txt_Guanjianzi.Text);
            txt_Guanjianzi.Text = string.Empty;
        }

        private void btn_feedback_Click(object sender, RoutedEventArgs e)
        {
            Feedback();
        }

        private void tw_Drak_Toggled_1(object sender, RoutedEventArgs e)
        {

        }

        private void sw_Light_Toggled(object sender, RoutedEventArgs e)
        {
            if (sw_Light.IsOn)
            {
                if (dispRequest == null)
                {
                    dispRequest = new DisplayRequest();
                    dispRequest.RequestActive(); 
                }
            }
            else
            {
                if (dispRequest != null)
                {
                    dispRequest = null;
                }
            }
            settings.SetSettingValue("HoldLight", sw_Light.IsOn);
        }

        private void sw_UseWifi_Toggled(object sender, RoutedEventArgs e)
        {
            settings.SetSettingValue("UseWifi", sw_UseWifi.IsOn);
        }

        private void tw_PlayLocal_Toggled(object sender, RoutedEventArgs e)
        {
            settings.SetSettingValue("PlayLocal", tw_PlayLocal.IsOn);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dan_Sp.OpenPaneLength = this.ActualWidth<=500 ? this.ActualWidth : 350;
        }
    }
}
