// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="pzcast">
//   (C) 2015 pzcast. All rights reserved.
// </copyright>
// <summary>
//   The program.1777
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SimpleCrawler.Demo
{
    using SimHashBusiness;
    using SimHashBusiness.Analysers;
    using SimHashBusiness.Interfaces;
    using StanSoft;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using zoyobar.shared.panzer.web.ib;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        #region Static Fields

        /// <summary>
        /// The settings.
        /// </summary>
        private static readonly CrawlSettings Settings = new CrawlSettings();

        /// <summary>
        /// The filter.
        /// 关于使用 Bloom 算法去除重复 URL：http://www.cnblogs.com/heaad/archive/2011/01/02/1924195.html
        /// </summary>
        private static BloomFilter<string> filter;

        private static Int32 fileId = 0;
        private static string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\Files\";
        private static string articlePath = AppDomain.CurrentDomain.BaseDirectory + @"\Articles\";
        private static string urlFilePath = AppDomain.CurrentDomain.BaseDirectory + @"\UrlFile\url.txt";
        private static Queue<String> fileQueue = new Queue<String>();

        #endregion Static Fields

        #region Methods

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            filter = new BloomFilter<string>(200000);
            //const string CityName = "beijing";

            // 设置种子地址
            //Settings.SeedsAddress.Add(string.Format("http://jobs.zhaopin.com/{0}", CityName));//
            //Settings.SeedsAddress.Add("http://www.shgtj.gov.cn/2011/gcjsxx/xmxx/ghxzyj/");
            Settings.SeedsAddress.Add("http://news.sdau.edu.cn/list.php?pid=3");
            //Settings.SeedsAddress.Add("   ");
            // 设置 URL 关键字
            //Settings.HrefKeywords.Add(string.Format("/{0}/bj", CityName));
            //Settings.HrefKeywords.Add(string.Format("/{0}/sj", CityName));

            // 设置爬取线程个数
            Settings.ThreadCount = 1;

            // 设置爬取深度
            Settings.Depth = 60;

            // 设置爬取时忽略的 Link，通过后缀名的方式，可以添加多个
            Settings.EscapeLinks.Add(".jpg");

            // 设置自动限速，1~5 秒随机间隔的自动限速
            Settings.AutoSpeedLimit = false;

            // 设置都是锁定域名,去除二级域名后，判断域名是否相等，相等则认为是同一个站点
            // 例如：mail.pzcast.com 和 www.pzcast.com
            Settings.LockHost = false;

            // 设置请求的 User-Agent HTTP 标头的值
            // settings.UserAgent 已提供默认值，如有特殊需求则自行设置

            // 设置请求页面的超时时间，默认值 15000 毫秒
            // settings.Timeout 按照自己的要求确定超时时间

            // 设置用于过滤的正则表达式
            //Settings.RegularFilterExpressions.Add("<a .+ href='(.+)'>下一页</a>");//  string strReg = "<a .+ href='(.+)'>下一页</a>";

            var master = new CrawlMaster(Settings);
            master.AddUrlEvent += MasterAddUrlEvent;
            master.DataReceivedEvent += MasterDataReceivedEvent;
            //master.CustomParseLinkEvent += CustomParseLinkEvent_Next;
            //master.CustomParseLinkEvent += CustomParseLinkEvent_MainList;
            master.CustomParseLinkEvent2 += Master_CustomParseLinkEvent2;
            master.Crawl();

            Console.ReadKey();
        }

        private static Dictionary<string, string> Master_CustomParseLinkEvent2(CustomParseLinkEvent2Args args)//用ref或out改写该方法
        {
            args.UrlDictionary = CustomParseLinkE_MainList(args, "(view).+?([0-9]{5})");//去除
            return CustomParseLinkE_NextPageSdau(args, "<a .+ href='(.+)'>下一页</a>", 1);//添加
        }

        /// <summary>
        /// The master add url event.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool MasterAddUrlEvent(AddUrlEventArgs args)
        {
            if (!filter.Contains(args.Url))//不包含就添加
            {
                filter.Add(args.Url);
                Console.WriteLine(args.Url);
                File.AppendAllText(urlFilePath, args.Url + "\r\n");
                return true;
            }

            return false; // 返回 false 代表：不添加到队列中
        }

        /// <summary>
        /// The master data received event.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>

        private static void MasterDataReceivedEvent(DataReceivedEventArgs args)
        {
            // 在此处解析页面，可以用类似于 HtmlAgilityPack（页面解析组件）的东东、也可以用正则表达式、还可以自己进行字符串分析
            //NSoup.Nodes.Document doc = NSoup.NSoupClient.Parse(args.Html);

            #region 接收数据处理

            fileQueue.Enqueue(args.Html);
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    try
                    {
                        if (fileQueue.Count > 0)
                        {
                            fileId++;
                            string fileContent = fileQueue.Dequeue();
                            if (fileContent.Trim() != "")
                            {
                                WriteToFiles(fileContent);
                            }
                            //AddUrlEventArgs urlArgs = new AddUrlEventArgs();
                            //urlArgs.Depth = 1;
                            //urlArgs.Title = "下一页";
                            //urlArgs.Url= AutoNextPage(args);
                            //MasterAddUrlEvent(urlArgs);
                            //string url = AutoNextPage(args);
                            //Settings.SeedsAddress.Add(url);
                        }
                        else
                        {
                            Thread.Sleep(2000);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            });

            #endregion 接收数据处理
        }

        private static void WriteToFiles(string fileContent)
        {
            NSoup.Nodes.Document doc = NSoup.NSoupClient.Parse(fileContent);
            using (FileStream fsWriter = new FileStream(filePath + fileId + doc.Title + ".html", FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = System.Text.Encoding.Default.GetBytes(fileContent);
                fsWriter.Write(buffer, 0, buffer.Length);
            }

            Article article = Html2Article.GetArticle(fileContent);
            //Console.WriteLine(article.Content);
            using (FileStream fsWriter = new FileStream(articlePath + fileId + doc.Title + ".txt", FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = System.Text.Encoding.Default.GetBytes(article.Content);
                fsWriter.Write(buffer, 0, buffer.Length);
            }
        }

        #endregion Methods

        #region 自定义方法

        //翻页的方法
        /// <summary>
        /// 模式<a href="?pid=3&amp;page=2">下一页</a>
        /// </summary>
        /// <param name="srcUrl">html页面的URL</param>
        /// <returns>下一页的URL</returns>
        private string AutoClickPageBar(string srcUrl)
        {
            //WebBrowser iBrowser = new WebBrowser();
            IEBrowser ie = new IEBrowser(new WebBrowser());
            ie.Navigate(srcUrl);
            //思路一：查找根据文本确定文本所在标签
            //思路二：在所有<a>标签中查找文本是“下一页”的<a>标签
            //思路三：正则表达式

            string url = "";
            return url;
        }

        /// <summary>
        /// 自动翻页
        /// </summary>
        /// <param name="args">DataReceivedEventArgs</param>
        /// <returns>URL</returns>
        private static void CustomParseLinkEvent_Next(CustomParseLinkEvent2Args args)
        {
            #region 20150930之前的代码

            /*20150930之前的代码
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
                        Int32 index = args.Url.LastIndexOf("/");
                        //url = args.Url.Substring(0, index) + "/" + mat.Groups[1].Value;
                        url = args.Url.Substring(0, index) + "/list.php" + mat.Groups[1].Value;
                        Console.WriteLine("************************");
                        Console.WriteLine(url);
                        Console.WriteLine("************************");

                        File.AppendAllText(urlFilePath, "************************" + "\r\n");
                        File.AppendAllText(urlFilePath, args.Url + "\r\n");
                        File.AppendAllText(urlFilePath, "************************" + "\r\n");
                    }
                }
                return IsUrlable(url) ? new UrlInfo(url) { Depth = args.Depth + 1 } : null;
                */

            #endregion 20150930之前的代码

            //urlAndHtml.Html = args.Html;
            //urlAndHtml.Url = args.Url;
            //string url = AutoNextPage(urlAndHtml, "<a .+ href='(.+)'>下一页</a>", 1);
            //return IsUrlable(url) ? new UrlInfo(url) { Depth = args.Depth + 1 } : null;
        }

        private static bool IsUrlable(string str)
        {
            return Regex.IsMatch(str, @"^http://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$");
        }

        private static Dictionary<string, string> CustomParseLinkE_NextPageSdau(CustomParseLinkEvent2Args args, string patternStr, int groupIndex)
        {
            string url = "";
            if (args != null && !string.IsNullOrEmpty(args.Html))
            {
                Regex regex = new Regex(patternStr);
                Match mat = regex.Match(args.Html);
                if (mat.Success)
                {
                    url = mat.Groups[groupIndex].Value;
                    var baseUri = new Uri(args.UrlInfo.UrlString);
                    Uri currentUri = url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                         ? new Uri(url)
                                         : new Uri(baseUri, url);//根据指定的基 URI 和相对 URI 字符串，初始化 System.Uri 类的新实例。
                                                                 //如果不包含http，则认为超链接是相对路径，根据baseUrl建立绝对路径
                    url = currentUri.AbsoluteUri;
                    //Console.WriteLine("######" + url + "######");
                    args.UrlDictionary.Add(url, Guid.NewGuid().ToString());
                }
            }
            return args.UrlDictionary;
        }

        private static Dictionary<string, string> CustomParseLinkE_MainList(CustomParseLinkEvent2Args args, string patternStr)
        {
            Dictionary<string, string> temp = new Dictionary<string, string>();
            foreach (var item in args.UrlDictionary)
            {
                string href = item.Key;
                string text = item.Value;

                if (!string.IsNullOrEmpty(href))
                {
                    Regex regex = new Regex(patternStr);
                    Match mat = regex.Match(href);
                    if (mat.Success)
                    {
                        temp.Add(href, text);
                    }
                }
            }
            return temp;
        }

        private static List<string> GetUsefulUrl(UrlAndHtml args, string patternStr, int groupIndex)
        {
            string url = "";
            string html = args.Html;
            string strReg = patternStr;
            List<string> list = new List<string>();

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
            return list;
        }

        #endregion 自定义方法
    }
}