using bilibili2.Pages;
using Newtonsoft.Json.Linq;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2.Controls
{
    public sealed partial class FindFragment : UserControl
    {
        Frame frame = Window.Current.Content as Frame;
        WebClientClass wc = new WebClientClass();
        public FindFragment()
        {
            this.InitializeComponent();
            SetHotSearchAsync();
        }

        private async void SetHotSearchAsync()
        {
            string url = $"http://app.bilibili.com/x/v2/search/hot?limit=50&appkey={ApiHelper._appKey}&build=434000&platform=android";
            url += "&sign=" + ApiHelper.GetSign(url);
            var result = await wc.GetResults(new Uri(url));
            var model = JObject.Parse(result);
            if(model["code"].Value<int>()==0)
            {
                var tags = model["data"]["list"].Select(token => new HotSearchViewModel
                {
                     Keyword =token["keyword"].Value<string>(),
                     Status = token["status"].Value<string>()
                });
                TagHot.ItemsSource = tags;
            }
        }
        private void AutoFind_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            frame.Navigate(typeof(SearchPage),args.QueryText);
        }

        private async void AutoFind_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text.Length == 0) return;
            string url = $"http://app.bilibili.com/x/v2/search/suggest?type=accurate&appkey={ApiHelper._appKey}&build=434000&platform=android&keyword={Uri.EscapeDataString(sender.Text)}";
            url += "&sign=" + ApiHelper.GetSign(url);
            var result = await wc.GetResults(new Uri(url));
            var model = JObject.Parse(result);
            if(model["code"].Value<int>()==0)
            {
                List<ISuggestion> suggestions = new List<ISuggestion>();
                if(model["data"]["upuser"]!=null)
                {
                    var ups = model["data"]["upuser"].Select(token => new SuggestionUpViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_hint_search.png",
                        Mid = token["mid"].Value<int>(),
                        Type = "UP主",
                        Value = token["value"].Value<string>()
                    });
                    suggestions.AddRange(ups);
                }
                if(model["data"]["bangumi"]!=null)
                {
                    var bangumis = model["data"]["bangumi"].Select(token => new SuggestionBanViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_hint_search.png",
                        Spid = token["spid"].Value<int>(),
                        Type = "番剧",
                        Value = token["value"].Value<string>()
                    });
                }
                if (model["data"]["suggest"] != null)
                {
                    var tags = model["data"]["suggest"].Select(token => new SuggestionViewModel
                    {
                        Icon = "ms-appx:///Assets/Icon/ic_hint_search.png",
                        Type = "",
                        Value = token.Value<string>()
                    });
                    suggestions.AddRange(tags);
                }
                sender.ItemsSource = suggestions;
            }
        }

        private void AutoFind_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            switch(args.SelectedItem)
            {
                case SuggestionViewModel tag:
                    sender.Text = tag.Value;
                    break;
                case SuggestionUpViewModel up:
                    frame.Navigate(typeof(UserInfoPage),up.Mid.ToString());
                    break;
                case SuggestionBanViewModel ban:
                    frame.Navigate(typeof(BanInfoPage),ban.Spid.ToString());
                    break;
            }
        }

        private void TagHot_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is HotSearchViewModel model)
            {
                frame.Navigate(typeof(SearchPage), model.Keyword);
            }
        }

        private void BtnTopic_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(TopicPage));
        }

        private void BtnRank_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(RankPage));
        }

        private void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(VideoInfoPage), new Random().Next(2000000, 4999999).ToString());
        }

        private void BtnActvity_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnBlackHouse_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
