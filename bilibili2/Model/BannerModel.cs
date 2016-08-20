using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili2.Model
{
    public class CodeModel
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("data")]
        public List<BannerModel> Data { get; set; }
    }
    //Banner
    public class BannerModel
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("weight")]
        public int Weight { get; set; }
        [JsonProperty("remark")]
        public string Remark { get; set; }
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
