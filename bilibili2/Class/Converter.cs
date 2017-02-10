using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace bilibili2.Class
{
    public static class Converter
    {
        public static string WeekToWeekIcon(string dateString)
        {
            var date = DateTime.Parse(dateString);
            return $"ms-appx:///Assets/Icon/bangumi_timeline_weekday_{(int)date.DayOfWeek}.png";
        } 

        public static string DateToWeek(string dateString)
        {
            var date = DateTime.Parse(dateString);
            switch(date.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    return "周五";
                case DayOfWeek.Monday:
                    return "周一";
                case DayOfWeek.Saturday:
                    return "周六";
                case DayOfWeek.Sunday:
                    return "周日";
                case DayOfWeek.Thursday:
                    return "周四";
                case DayOfWeek.Tuesday:
                    return "周二";
                case DayOfWeek.Wednesday:
                    return "周三";
                default:
                    return "";
            }
        }

        public static string DateToSimplifyDate(string dateString)
        {
            var date = DateTime.Parse(dateString);
            var today = DateTime.Now;
            var spanDays = (today-date).TotalDays;
            if (spanDays<1&&spanDays>=0)
            {
                return "今天";
            }
            else if(spanDays<0&&spanDays>=-1)
            {
                return "明天";
            }
            else if(spanDays<2&&spanDays>=1)
            {
                return "昨天";
            }
            else if(spanDays<3&&spanDays>=2)
            {
                return "前天";
            }
            else
            {
                return $"{date.Month}月{date.Day}日";
            }
        }

        public static string DurationToLongDuration(string duration)
        {
            var part = duration.Split(':');
            var minutes = int.Parse(part[0]);
            var seconds = int.Parse(part[1]);
            return $"{minutes/60:D2}:{minutes%60:D2}:{seconds:D2}";
        }

        public static int RankIndexToFontSize(int index)
        {
            switch (index)
            {
                case 1: return 18;
                case 2: return 17;
                case 3: return 16;
                default:
                    return 15;
            }
        }

        public static Color StringToColor(string colorName)
        {
            try
            {
                if (colorName.StartsWith("#"))
                    colorName = colorName.Replace("#", string.Empty);
                ulong v = ulong.Parse(colorName);
                var a = Convert.ToByte(v >> 24 & 0XFF);
                var r = Convert.ToByte(v >> 16 & 0XFF);
                var g = Convert.ToByte(v >> 8 & 0XFF);
                var b = Convert.ToByte(v >> 0 & 0XFF);
                Color color = Color.FromArgb(a, r, g, b);
                return color;
            }
            catch
            {
                return new Color
                {
                    A = 255,
                    R = 255,
                    G = 255,
                    B = 255,
                };
            }
        }
    }
}
