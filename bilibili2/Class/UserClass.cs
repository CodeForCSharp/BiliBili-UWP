﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace bilibili2.Class
{
    class UserClass
    {
        //public static string Uid = string.Empty;
        private static string _uid;
        public static List<string> AttentionList = new List<string>();
        public static string Uid
        {
            get
            {
                HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
                HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
                //hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
                foreach (HttpCookie item in cookieCollection)
                {
                    if (item.Name == "DedeUserID")
                    {
                        _uid = item.Value;
                    }
                }
                return _uid;
            }
        }
        /// <summary>
        /// 用户信息
        /// </summary>
        /// <returns></returns>
        public async Task<GetLoginInfoModel> GetUserInfo()
        {
            if (IsLogin())
            {
                try
                {
                    using (HttpClient hc = new HttpClient())
                    {
                        HttpResponseMessage hr = await hc.GetAsync(new Uri("http://api.bilibili.com/userinfo?mid=" + Uid + "&rd=" + new Random().Next(1, 1000)));
                        hr.EnsureSuccessStatusCode();
                        string results = await hr.Content.ReadAsStringAsync();
                        GetLoginInfoModel model = new GetLoginInfoModel();
                        model = JsonConvert.DeserializeObject<GetLoginInfoModel>(results);
                        AttentionList = JsonConvert.DeserializeObject<List<string>>(model.attentions.ToString());
                        JObject json = JObject.Parse(model.level_info.ToString());
                        model.current_level = (int)json["current_level"];
                        //model.current_level = "LV" + json["current_level"].ToString();
                        return model;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public async Task<GetLoginInfoModel> GetUserInfo(string uid)
        {
            try
            {
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://api.bilibili.com/userinfo?mid=" + uid + "&rd=" + new Random().Next(1, 1000)));
                GetLoginInfoModel model = new GetLoginInfoModel();
                model = JsonConvert.DeserializeObject<GetLoginInfoModel>(results);
                JObject json = JObject.Parse(model.level_info.ToString());
                model.current_level = (int)json["current_level"];
                //model.current_level = "LV" + json["current_level"].ToString();
                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 追番
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetUserBangumi>> GetUserBangumi()
        {
            if (IsLogin())
            {
                try
                {
                    using (HttpClient hc = new HttpClient())
                    {
                        HttpResponseMessage hr = await hc.GetAsync(new Uri("http://space.bilibili.com/ajax/Bangumi/getList?mid=" + Uid + "&pagesize=20"));
                        hr.EnsureSuccessStatusCode();
                        string results = await hr.Content.ReadAsStringAsync();
                        //一层
                        GetUserBangumi model1 = JsonConvert.DeserializeObject<GetUserBangumi>(results);
                        if (model1.status)
                        {
                            //二层
                            GetUserBangumi model2 = JsonConvert.DeserializeObject<GetUserBangumi>(model1.data.ToString());
                            //三层
                            List<GetUserBangumi> lsModel = JsonConvert.DeserializeObject<List<GetUserBangumi>>(model2.result.ToString());
                            return lsModel;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public async Task<List<GetUserBangumi>> GetUserBangumi(string uid)
        {
            try
            {
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://space.bilibili.com/ajax/Bangumi/getList?mid=" + uid + "&pagesize=20"));
                //一层
                GetUserBangumi model1 = JsonConvert.DeserializeObject<GetUserBangumi>(results);
                if (model1.status)
                {
                    //二层
                    GetUserBangumi model2 = JsonConvert.DeserializeObject<GetUserBangumi>(model1.data.ToString());
                    //三层
                    List<GetUserBangumi> lsModel = JsonConvert.DeserializeObject<List<GetUserBangumi>>(model2.result.ToString());
                    return lsModel;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 关注动态
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetAttentionUpdate>> GetUserAttentionUpdate(int PageNum)
        {
            if (IsLogin())
            {
                try
                {
                    using (HttpClient hc = new HttpClient())
                    {
                        HttpResponseMessage hr = await hc.GetAsync(new Uri("http://api.bilibili.com/x/feed/pull?jsonp=jsonp&ps=20&type=1&pn=" + PageNum));
                        hr.EnsureSuccessStatusCode();
                        string results = await hr.Content.ReadAsStringAsync();
                        //一层
                        GetAttentionUpdate model1 = JsonConvert.DeserializeObject<GetAttentionUpdate>(results);
                        if (model1.code == 0)
                        {
                            //二层
                            GetAttentionUpdate model2 = JsonConvert.DeserializeObject<GetAttentionUpdate>(model1.data.ToString());
                            //三层
                            List<GetAttentionUpdate> ls = JsonConvert.DeserializeObject<List<GetAttentionUpdate>>(model2.feeds.ToString());
                            //四层
                            List<GetAttentionUpdate> lsModel = new List<GetAttentionUpdate>();
                            foreach (GetAttentionUpdate item in ls)
                            {
                                GetAttentionUpdate m = JsonConvert.DeserializeObject<GetAttentionUpdate>(item.addition.ToString());
                                m.page = model2.page;
                                lsModel.Add(m);
                            }
                            return lsModel;
                        }
                        else
                        {
                            return null;
                        }
                    }

                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 收藏夹
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetUserFovBox>> GetUserFovBox()
        {
            if (IsLogin())
            {
                try
                {
                    WebClientClass wc = new WebClientClass();
                    string results = await wc.GetResults(new Uri("http://space.bilibili.com/ajax/fav/getBoxList?mid=" + Uid));
                    //一层
                    GetUserFovBox model1 = JsonConvert.DeserializeObject<GetUserFovBox>(results);
                    if (model1.status)
                    {
                        //二层
                        GetUserFovBox model2 = JsonConvert.DeserializeObject<GetUserFovBox>(model1.data.ToString());
                        //三层
                        List<GetUserFovBox> lsModel = JsonConvert.DeserializeObject<List<GetUserFovBox>>(model2.list.ToString());
                        return lsModel;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 观看历史
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetHistoryModel>> GetHistory(int PageNum)
        {
            if (IsLogin())
            {
                try
                {
                    string results = await new WebClientClass().GetResults(new Uri("http://api.bilibili.com/x/history?jsonp=jsonp&ps=20&pn=" + PageNum+"&rnd="+new Random().Next(1000,9999)));
                    //一层
                    GetHistoryModel model = JsonConvert.DeserializeObject<GetHistoryModel>(results);
                    if (model.data == null)
                    {
                        return null;
                    }
                    else
                    {
                        List<GetHistoryModel> lsModel = JsonConvert.DeserializeObject<List<GetHistoryModel>>(model.data.ToString());
                        return lsModel;
                    }

                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 关注直播
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetAttentionLive>> GetAttentionLive()
        {
            List<GetAttentionLive> list = new List<GetAttentionLive>();
            try
            {
                string url = string.Format("http://live.bilibili.com/AppFeed/index?_device=wp&_ulv=10000&access_key={0}&appkey=1d8b6e7d45233436&build=434000&page=1&pagesize=20&platform=android",ApiHelper.access_key);
                url += "&sign="+ApiHelper.GetSign(url);
                string result =await  new WebClientClass().GetResults(new Uri(url));
                GetAttentionLive mode = JsonConvert.DeserializeObject<GetAttentionLive>(result);
                if (mode.code==0)
                {
                    GetAttentionLive model = JsonConvert.DeserializeObject<GetAttentionLive>(mode.data.ToString());
                    list = JsonConvert.DeserializeObject<List<GetAttentionLive>>(model.list.ToString());
                    return list.OrderByDescending(s => s.live_status).ToList();
                }
                else
                {
                    return list;
                }
            }
            catch (Exception)
            {
                return list;
            }
        }
        //是否登录
        public bool IsLogin()
        {
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            List<string> ls = new List<string>();
            foreach (HttpCookie item in cookieCollection)
            {
                ls.Add(item.Name);
            }
            if (!ls.Contains("DedeUserID") || !ls.Contains("DedeUserID__ckMd5"))
            {
                return false;
            }
            else
            {
                //HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
                hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
                //foreach (HttpCookie item in cookieCollection)
                //{
                //    if (item.Name == "DedeUserID")
                //    {
                //        Uid = item.Value;
                //    }
                //}
                return true;

            }
        }

        public async Task<List<GetFavouriteBoxsVideoModel>> GetFavouriteBoxVideo(string fid, int PageNum)
        {
            //啊啊啊啊，没心情啊，下面代码都是乱写的，啊啊啊啊啊 啊啊啊啊啊啊
            if (IsLogin())
            {
                try
                {

                        string results = await new WebClientClass().GetResults(new Uri("http://space.bilibili.com/ajax/fav/getList?mid=" + Uid + "&pagesize=20&fid=" + fid + "&pid=" + PageNum));
                        //一层
                        GetFavouriteBoxsVideoModel model = JsonConvert.DeserializeObject<GetFavouriteBoxsVideoModel>(results);
                        //二层
                        if (model.status)
                        {
                            GetFavouriteBoxsVideoModel model2 = JsonConvert.DeserializeObject<GetFavouriteBoxsVideoModel>(model.data.ToString());
                            //三层
                            List<GetFavouriteBoxsVideoModel> lsModel = JsonConvert.DeserializeObject<List<GetFavouriteBoxsVideoModel>>(model2.vlist.ToString());
                            List<GetFavouriteBoxsVideoModel> RelsModel = new List<GetFavouriteBoxsVideoModel>();
                            foreach (GetFavouriteBoxsVideoModel item in lsModel)
                            {
                                item.pages = model2.pages;
                                RelsModel.Add(item);
                            }
                            return RelsModel;
                        }
                        else
                        {
                            return null;
                        }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

    }
}
