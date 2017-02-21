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
        public FindFragment()
        {
            this.InitializeComponent();
        }

        private void AutoFind_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void AutoFind_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void AutoFind_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private void TagHot_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void BtnTopic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRank_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRandom_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnActvity_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnBlackHouse_Click(object sender, RoutedEventArgs e)
        {

        }

        public class SuggestionTemplateSelector:DataTemplateSelector
        {
            protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
            {
                switch(item)
                {
                    case 
                }
                return base.SelectTemplateCore(item, container);
            }
        }
    }
}
