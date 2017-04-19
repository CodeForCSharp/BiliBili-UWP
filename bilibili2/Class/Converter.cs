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
            var spanDays = (today-today.TimeOfDay-date).TotalDays;
            if (spanDays<0&&spanDays>=-1)
            {
                return "今天";
            }
            else if(spanDays<-1&&spanDays>=-2)
            {
                return "明天";
            }
            else if(spanDays<1&&spanDays>=0)
            {
                return "昨天";
            }
            else if(spanDays<2&&spanDays>=1)
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
                var a = colorName.Length <= 8 ? Convert.ToByte(255) : Convert.ToByte(v >> 24 & 0XFF);
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

        public static string TimeToSimplifyTime(string time)
        {
            var date = DateTime.Parse(time);
            var timeCalled = "";
            var now = DateTime.Now;
            var pivot = now - now.TimeOfDay;
            var spanDays = (pivot-date).TotalDays;
            if(spanDays<0&&spanDays>=-1)
            {
                if (date.Hour > 0 && date.Hour <= 4)
                {
                    timeCalled = "凌晨";
                }
            }
            else if(spanDays>=0&spanDays<1)
            {
                timeCalled = "昨天";
            }
            return $"{timeCalled}{date.Hour}:{date.Minute:D2}";
        }

        public static string SimplifyTimes(double times)
        {
            return times >= 10000 ? $"{times / 10000:.00}万" : $"{times}";
        }

        public static string TickToDate(long tick)
        {
            var pivot = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var pubdate = pivot + TimeSpan.FromSeconds(tick);
            return pubdate.ToString("yyyy-MM-dd hh:mm:ss");
        }
    }
}
