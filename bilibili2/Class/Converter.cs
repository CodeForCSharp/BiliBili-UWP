using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
