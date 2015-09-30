using DotNet4.Utilities;
using Kw.Combinatorics;
using SimHashBusiness.Analysers;
using SimHashBusiness.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Test
{
    internal class Program
    {
        private static string logFilePath = AppDomain.CurrentDomain.BaseDirectory + @"\logFile\logclear.txt";

        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine("Start");
            UrlAndHtml urlAndHtml = new UrlAndHtml();
            urlAndHtml.Url = "http://news.sdau.edu.cn/list.php?pid=3";
            #region MyRegion
            /*
      //AutoClickPageBar(str);

      WebBrowser wb = new WebBrowser();
      IEBrowser ie = new IEBrowser(wb);
      ie.Navigate(urlAndHtml.Url);
      ie.IEFlow.Wait(new UrlCondition("wait", urlAndHtml.Url, StringCompareMode.StartWith));
      #region
      WebRequest request = WebRequest.Create(urlAndHtml.Url); //请求url
      WebResponse response = request.GetResponse(); //获取url数据

      StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("gb2312"));
      string tempStr = reader.ReadToEnd();
      #endregion MyRegion
      HtmlDocument doc = ie.Document;

      if (doc != null)
      {
          foreach (HtmlElement item in doc.Links)
          {
              urlAndHtml.Html += item.OuterHtml;
          }
          //urlAndHtml.Html = doc.ToString();
          Console.WriteLine(urlAndHtml.Html);
          Console.WriteLine(AutoNextPage(urlAndHtml));
      }
      */
            #endregion

            #region HttpHelper
            string html = GetHtml(urlAndHtml.Url);

            #endregion
            urlAndHtml.Html = html;

            //Console.WriteLine(urlAndHtml.Html);
            string strReg = "<a .+ href='(.+)'>下一页</a>";
            Console.WriteLine(AutoNextPage(urlAndHtml, strReg, 1));
            //AutoNextPage(GetHtml(AutoNextPage(urlAndHtml)));

            Console.WriteLine("End");
            System.Console.ReadKey();
            #region 测试数据

            //测试排列组合
            List<string> list = new List<string>();
            list.Add("http://news.sdau.edu.cn/web_manage/index.php");
            list.Add("http://news.sdau.edu.cn/tongji.php");
            list.Add("http://news.sdau.edu.cn/");
            list.Add("http://news.sdau.edu.cn/list.php?pid=1");
            list.Add("http://news.sdau.edu.cn/list.php?pid=2");
            list.Add("http://news.sdau.edu.cn/list.php?pid=3");
            list.Add("http://news.sdau.edu.cn/list.php?pid=4");
            list.Add("http://news.sdau.edu.cn/yzrd.php");
            list.Add("http://news.sdau.edu.cn/list.php?pid=6");
            list.Add("http://www4.sdau.edu.cn/xcb/tushuoxiaoyuan");
            list.Add("http://news.sdau.edu.cn/list.php?pid=8");
            list.Add("http://xiaobao.sdau.edu.cn/");
            list.Add("http://news.sdau.edu.cn/list.php?pid=11");
            list.Add("http://news.sdau.edu.cn/list.php?pid=7");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66429");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66441");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66431");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66444");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66397");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66454");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66419");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66428");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66445");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66409");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66424");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66404");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66458");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66427");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66416");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66430");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66442");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66455");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66421");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66452");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66499");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66476");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66448");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66467");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66460");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66443");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66423");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66398");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66392");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66390");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66389");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66366");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66377");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66369");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66347");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66341");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66340");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66334");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66333");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66331");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66325");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66292");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66286");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66223");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66218");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66199");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66192");
            list.Add("http://news.sdau.edu.cn/view.php?Id=66189");
            list.Add("http://www.sdau.edu.cn/xcb/tushuoxiaoyuan");
            list.Add("http://weekly.sdau.edu.cn/");
            list.Add("http://www.sdaunet.com/");
            list.Add("http://bbs.sdaunet.com/");
            list.Add("http://news.sdau.edu.cn/web_manage/index.php");
            list.Add("http://news.sdau.edu.cn/tongji.php");
            list.Add("http://news.sdau.edu.cn/");
            list.Add("http://news.sdau.edu.cn/list.php?pid=1");
            list.Add("http://news.sdau.edu.cn/list.php?pid=2");
            list.Add("http://news.sdau.edu.cn/list.php?pid=3");
            list.Add("http://news.sdau.edu.cn/list.php?pid=4");
            list.Add("http://news.sdau.edu.cn/yzrd.php");
            list.Add("http://news.sdau.edu.cn/list.php?pid=6");
            list.Add("http://www4.sdau.edu.cn/xcb/tushuoxiaoyuan");
            list.Add("http://news.sdau.edu.cn/list.php?pid=8");
            list.Add("http://xiaobao.sdau.edu.cn/");

            list.Add("今天天气好晴朗");
            list.Add("今天天气好晴朗，又是刮风又是下雨");
            list.Add("娃哈哈");
            list.Add("小明，你妈叫你回家吃饭");
            #endregion
            #region 换个思路
            /*
            List<UrlCombination> urlList = GetCombinatorics(list);
            //urlList.OrderBy(c => c.SimHash);//排序,不用排序
            //List<UrlCombination> clrList = urlList.Where(c => c.SimHash >= 87.5f).ToList();//筛选
            //
            foreach (var item in urlList)
            {
                Console.WriteLine(item.Url1 + "与" + item.Url2 + "的相似度是：" + item.SimHash + "%");
                File.AppendAllText(logFilePath, item.Url1 + "与" + item.Url2 + "的相似度是：" + item.SimHash + "%" + "\r\n");
            }
            */
            #endregion

            //新思路
            //"http://news.sdau.edu.cn/view.php?Id=66340"
            strReg = "(view).+?([0-9]{5})";// @"<a href='(.+)'.+</a>";//<a href="view.php?Id=66533" title="【专题】金秋硕果日 扬帆追梦时" target="_blank">【专题】金秋硕果日 扬帆追梦时</a>
            //List<string> patList = new List<string>();
            //patList.Add(strReg);

            List<string> urlList = GetUsefulUrl(urlAndHtml, strReg, 0);
            foreach (var item in urlList)
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();
        }

        private static string AutoNextPage(UrlAndHtml args, string patternStr, int groupIndex)
        {
            string url = "";
            string html = args.Html;
            //string strReg = "<a .+ href='(.+)'>下一页</a>";
            Regex regex = new Regex(patternStr);
            Match mat = regex.Match(html);
            if (mat.Success)
            {
                if (IsUrlable(mat.Groups[groupIndex].Value))
                {
                    url = mat.Groups[groupIndex].Value;
                }
                else
                {
                    //url=args.Url.en
                    //abcd e f
                    //0123 4 5
                    //http://news.sdau.edu.cn/list.php?pid=3
                    //http://news.sdau.edu.cn /?pid=3&page=2
                    Int32 index = args.Url.LastIndexOf("/");
                    url = args.Url.Substring(0, index) + "/list.php" + mat.Groups[groupIndex].Value;
                }
            }
            return IsUrlable(url) ? url : null;
        }

        private static List<string> GetUsefulUrl(UrlAndHtml args, string patternStr, int groupIndex)
        {
            string url = "";
            string html = args.Html;
            string strReg = patternStr;// "<a .+ href='(view.php?Id=\\d+)' .*</a>";//<a href="view.php?Id=66529" target="_blank">植保学院邀请专家为青年教师授课</a>

            List<string> list = new List<string>();

            #region 废弃

            MatchCollection mats = Regex.Matches(html, strReg);
            foreach (Match item in mats)
            {
                if (item.Success)
                {
                    if (IsUrlable(item.Groups[groupIndex].Value))
                    {
                        url = item.Groups[groupIndex].Value;
                    }
                    else
                    {
                        //这个地方其实也是写死的，可以有高级配置存在
                        //如果正则表达式得到的url是相对路径，需要配置为绝对路径
                        //参数名为相对路径前缀，且以“/”结尾
                        Int32 index = args.Url.LastIndexOf("/");
                        url = args.Url.Substring(0, index) + "/" + item.Groups[groupIndex].Value;
                    }
                }
                if (IsUrlable(url))
                {
                    list.Add(url);
                }
            }
            #endregion
            return list;
        }

        private static string GetHtml(string url)
        {
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项
                Method = "get",//URL     可选项 默认为Get
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
                Cookie = "",//字符串Cookie     可选项
                Referer = "",//来源URL     可选项
                Postdata = "",//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",//用户的浏览器类型，版本，操作系统     可选项有默认值
                ContentType = "text/html",//返回类型    可选项有默认值
                Allowautoredirect = false,//是否根据301跳转     可选项
                //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数
                //Connectionlimit = 1024,//最大连接数     可选项 默认为1024
                ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数
                //ProxyPwd = "123456",//代理服务器密码     可选项
                //ProxyUserName = "administrator",//代理服务器账户名     可选项
                ResultType = ResultType.String
            };
            HttpResult result = http.GetHtml(item);
            string html = result.Html;
            string cookie = result.Cookie;
            return html;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>

        private static string AutoNextPage(UrlAndHtml args)
        {
            string url = "";
            string html = args.Html;
            string strReg = "<a .+ href='(.+)'>下一页</a>";
            Regex regex = new Regex(strReg);
            Match mat = regex.Match(html);
            if (mat.Success)
            {
                if (IsUrlable(mat.Groups[1].Value))
                {
                    url = mat.Groups[1].Value;
                }
                else
                {
                    //url=args.Url.en
                    //abcd e f
                    //0123 4 5
                    //http://news.sdau.edu.cn/list.php?pid=3
                    //http://news.sdau.edu.cn /?pid=3&page=2
                    Int32 index = args.Url.LastIndexOf("/");
                    url = args.Url.Substring(0, index) + "/list.php" + mat.Groups[1].Value;
                }
            }
            return IsUrlable(url) ? url : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <param name="patternStr">传入的正则表达式，且分组后Groups[0]代表所需内容</param>
        /// <returns></returns>
        private static List<string> GetUsefulUrl(UrlAndHtml args, string patternStr)//直接匹配
        {
            string url = "";
            string html = args.Html;
            string strReg = patternStr;
            List<string> list = new List<string>();

            #region 废弃

            MatchCollection mats = Regex.Matches(html, strReg);
            foreach (Match item in mats)
            {
                if (item.Success)
                {
                    if (IsUrlable(item.Groups[0].Value))
                    {
                        url = item.Groups[0].Value;
                    }
                    else
                    {
                        //url=args.Url.en
                        //abcd e f
                        //0123 4 5
                        //http://news.sdau.edu.cn/list.php?pid=3
                        //http://news.sdau.edu.cn /?pid=3&page=2
                        Int32 index = args.Url.LastIndexOf("/");
                        url = args.Url.Substring(0, index) + "/" + item.Groups[0].Value;
                    }
                }
                if (IsUrlable(url))
                {
                    list.Add(url);
                }
            }
            #endregion
            return list;
        }

        private static List<string> GetUsefulUrl(UrlAndHtml args, List<string> patternStrList)//直接匹配
        {
            #region sdau
            string url = "";
            string html = args.Html;
            List<string> UseFulList = null;

            if (patternStrList != null && patternStrList.Count > 0)
            {
                UseFulList = new List<string>();

                foreach (var item in patternStrList)
                {
                    Regex regex = new Regex(item);
                    Match mat = regex.Match(html);//Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase)
                    if (mat.Success)
                    {
                        if (IsUrlable(mat.Groups[0].Value))
                        {
                            //UseFulList.Add(mat.Groups[0].Value);
                            url = mat.Groups[0].Value;
                        }
                        else
                        {
                            url = "http://news.sdau.edu.cn/" + mat.Groups[0].Value;
                        }
                    }
                    if (IsUrlable(url))
                    {
                        UseFulList.Add(url);
                    }
                }
            }
            return UseFulList;
            #endregion
        }

        private static bool IsUrlable(string str)
        {
            return Regex.IsMatch(str, @"^http://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$");
        }

        #region 换思路前

        //字符串两两组合。
        //需要一个新的类型
        private static List<UrlCombination> GetCombinatorics(List<string> list)
        {
            List<UrlCombination> comList = new List<UrlCombination>();
            IAnalyser analyser = new SimHashAnalyser();
            foreach (var row in new Combination(list.Count, 2).GetRows())//row里存了，m中选出n，和结果数。
            {
                UrlCombination urlCom = new UrlCombination();
                List<string> com = Combination.Permute(row, list);//Combination.Permute(row, list)返回一个组合
                urlCom.Url1 = com[0];
                urlCom.Url2 = com[1];
                //SimHash运算
                urlCom.SimHash = analyser.GetLikenessValue(com[0], com[1]) * 100;
                comList.Add(urlCom);
            }

            return comList;
        }

        private static float GetSimHash(string str1, string str2)
        {
            IAnalyser analyser = new SimHashAnalyser();
            return analyser.GetLikenessValue(str1, str2) * 100;
        }

        /// <summary>
        /// 移除相似值低的数据，即分类
        /// </summary>
        /// <param name="list"></param>
        /// <param name="gateValue"></param>
        private static List<UrlCombination> ClearUrlComList(List<UrlCombination> list, float gateValue)
        {
            List<UrlCombination> temp = new List<UrlCombination>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].SimHash >= gateValue)
                {
                    temp.Add(list[i]);
                }
            }
            return temp;
        }

        #endregion
    }
}