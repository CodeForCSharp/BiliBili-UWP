using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili2.Model
{
    public class EntranceIconModel
    {
        [JsonProperty("src")]
        public string Src { get; set; }
        [JsonProperty("height")]
        public string Height { get; set; }
        [JsonProperty("width")]
        public string Width { get; set; }
    }
    public class EntranceIconsModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("entrance_icon")]
        public EntranceIconModel EntranceIcon { get; set; }
    }
    public class PartitionModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("area")]
        public string Area { get; set; }
        [JsonProperty("sub_icon")]
        public SubIconModel SubIcon { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
    }
    public class OwnerModel
    {
        [JsonProperty("face")]
        public string Face { get; set; }
        [JsonProperty("mid")]
        public int Mid { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class CoverModel
    {
        [JsonProperty("src")]
        public string Src { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
    }
    public class LivesModel
    {
        [JsonProperty("owner")]
        public OwnerModel Owner { get; set; }
        [JsonProperty("cover")]
        public CoverModel Cover { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("room_id")]
        public int RoomId { get; set; }
        [JsonProperty("check_version")]
        public int CheckVersion { get; set; }
        [JsonProperty("online")]
        public int Online { get; set; }
        [JsonProperty("area")]
        public string Area { get; set; }
        [JsonProperty("area_id")]
        public int AreaId { get; set; }
        [JsonProperty("playurl")]
        public string Playurl { get; set; }
        [JsonProperty("accept_quality")]
        public string AcceptQuality { get; set; }
        [JsonProperty("broadcast_type")]
        public int BroadcastType { get; set; }
        [JsonProperty("is_tv")]
        public int IsTv { get; set; }
        [JsonProperty("corner")]
        public string Corner { get; set; }
    }
    public class PartitionsModel
    {
        [JsonProperty("partition")]
        public PartitionModel Partition { get; set; }
        [JsonProperty("lives")]
        public List<LivesModel> Lives { get; set; }
    }
    public class RecommendDataModel
    {
        [JsonProperty("lives")]
        public List<LivesModel> Lives { get; set; }
        [JsonProperty("partition")]
        public PartitionModel Partition { get; set; }
        [JsonProperty("banner_data")]
        public List<LivesModel> BannerData { get; set; }
    }
    public class HomeLiveDataModel
    {
        [JsonProperty("banner")]
        public List<LiveBannerModel> Banner { get; set; }
        [JsonProperty("entranceIcons")]
        public List<EntranceIconsModel> EntranceIcons { get; set; }
        [JsonProperty("partitions")]
        public List<PartitionsModel> Partitions { get; set; }
        [JsonProperty("recommend_data")]
        public RecommendDataModel RecommendData { get; set; }
    }
    public class HomeLiveModel
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public HomeLiveDataModel Data { get; set; }
    }
    public class SubIconModel
    {
        [JsonProperty("src")]
        public string Src { get; set; }
        [JsonProperty("height")]
        public string Height { get; set; }
        [JsonProperty("width")]
        public string Width { get; set; }
    }

    public class LiveBannerModel
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("img")]
        public string Img { get; set; }
        [JsonProperty("remark")]
        public string Remark { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
