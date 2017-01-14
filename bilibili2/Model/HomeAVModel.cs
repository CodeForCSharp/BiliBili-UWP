using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili2.Model
{
    public class AVModel
    {
        [JsonProperty("aid")]
        public string Aid { get; set; }
        [JsonProperty("copyright")]
        public string Copyright { get; set; }
        [JsonProperty("typeid")]
        public int Typeid { get; set; }
        [JsonProperty("typename")]
        public string Typename { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }
        [JsonProperty("play")]
        public string Play { get; set; }
        [JsonProperty("review")]
        public int Review { get; set; }
        [JsonProperty("video_review")]
        public int VideoReview { get; set; }
        [JsonProperty("favorites")]
        public int Favorites { get; set; }
        [JsonProperty("mid")]
        public int Mid { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("create")]
        public string Create { get; set; }
        [JsonProperty("pic")]
        public string Pic { get; set; }
        [JsonProperty("credit")]
        public int Credit { get; set; }
        [JsonProperty("coins")]
        public int Coins { get; set; }
        [JsonProperty("duration")]
        public string Duration { get; set; }
        [JsonProperty("comment")]
        public int Comment { get; set; }
        [JsonProperty("badgepay")]
        public bool Badgepay { get; set; }
    }
    public class HomeAVModel
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("pages")]
        public int Pages { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("num")]
        public int Num { get; set; }
        [JsonProperty("results")]
        public int Results { get; set; }
        [JsonProperty("list")]
        public IEnumerable<AVModel> List { get; set; }
    }
}
