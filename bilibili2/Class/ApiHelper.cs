using BILIBILI_UWP.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace bilibili2
{
    class ApiHelper
    {
        public const string _appSecret = "94aba54af9065f71de72f5508f1cd42e";
        public const string _appKey = "84956560bc028eb7";
        public const string _appSecret1 = "1c15888dc316e05a15fdd0a02ed6584f";
        public const string _appKey1 = "f3bb208b3d081dc8";
        public static string access_key = string.Empty;
        public static string _buvid = "B3CC4714-C1D3-4010-918B-8E5253E123C16133infoc";
        public static string _hwid = "03008c8c0300d6d1";
        public static string GetSign(string url)
        {
            string str = url.Substring(url.IndexOf("?", 4, StringComparison.Ordinal) + 1);
            var list = str.Split('&').ToList();
            list.Sort();
            var stringBuilder = new StringBuilder();
            foreach (var str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret);
            var result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }

        public static string GetSign1(string url)
        {
            string str = url.Substring(url.IndexOf("?", 4, StringComparison.Ordinal) + 1);
            var list = str.Split('&').ToList();
            list.Sort();
            var stringBuilder = new StringBuilder();
            foreach (var str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret1);
            var result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }

        public static long GetTimeSpen => Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);

        public static async Task<string> GetEncryptedPassword(string passWord)
        {
            //https://secure.bilibili.com/login?act=getkey&rnd=4928
            //https://passport.bilibili.com/login?act=getkey&rnd=4928
            HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
            httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
            httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient(httpBaseProtocolFilter);
            //WebClientClass wc = new WebClientClass();
            string stringAsync =  await httpClient.GetStringAsync((new Uri("https://secure.bilibili.com/login?act=getkey&rnd=" + new Random().Next(1000,9999), UriKind.Absolute)));
            JObject jObjects = JObject.Parse(stringAsync);
            string str = jObjects["hash"].ToString();
            string str1 = jObjects["key"].ToString();
            string str2 = string.Concat(str, passWord);
            string str3 = Regex.Match(str1, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim();
            byte[] numArray = Convert.FromBase64String(str3);
            AsymmetricKeyAlgorithmProvider asymmetricKeyAlgorithmProvider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
            CryptographicKey cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(numArray.AsBuffer(), 0);
            IBuffer buffer = CryptographicEngine.Encrypt(cryptographicKey, Encoding.UTF8.GetBytes(str2).AsBuffer(), null);
            var base64String = Convert.ToBase64String(buffer.ToArray());
            return base64String;
        }

        public static async Task<string> LoginBilibili(string UserName, string Password)
        {
            try
            {
                //发送第一次请求，得到access_key
                WebClientClass wc = new WebClientClass();
                string results =await wc.GetResults(new Uri("https://api.bilibili.com/login?appkey=84956560bc028eb7&platform=wp&pwd=" +WebUtility.UrlEncode(Password) + "&type=json&userid=" + WebUtility.UrlEncode(UserName)));
                //Json解析及数据判断
                LoginModel model = new LoginModel();
                model = JsonConvert.DeserializeObject<LoginModel>(results);
                switch (model.code)
                {
                    case -627:
                        return "登录失败，密码错误！";
                    case -626:
                        return "登录失败，账号不存在！";
                    case -625:
                        return "密码错误多次";
                    case -628:
                        return "未知错误";
                    case -1:
                        return "登录失败，程序注册失败！请联系作者！";
                }
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                if (model.code == 0)
                {
                    access_key = model.access_key;
                    var hr2 = await hc.GetAsync(new Uri("http://api.bilibili.com/login/sso?&access_key=" + model.access_key + "&appkey=84956560bc028eb7&platform=wp"));
                    hr2.EnsureSuccessStatusCode();
                    var folder = ApplicationData.Current.LocalFolder;
                    var file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
                    await FileIO.WriteTextAsync(file, model.access_key);
                }
                //看看存不存在Cookie
                var hb = new HttpBaseProtocolFilter();
                var cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
                if (cookieCollection.Select(item => item.Name).Contains("DedeUserID"))
                {
                    return "登录成功";
                }
                return "登录失败";
            }
            catch (Exception)
            {
                return "登录发生错误";
            }
        }
    }
}
