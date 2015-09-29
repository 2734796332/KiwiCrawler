﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrawlMaster.cs" company="pzcast">
//   (C) 2015 pzcast. All rights reserved.
// </copyright>
// <summary>
//   The crawl master.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SimpleCrawler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// The crawl master.
    /// </summary>
    public class CrawlMaster
    {
        #region Constants

        /// <summary>
        /// The web url regular expressions.
        /// url正则表达式。
        /// </summary>
        private const string WebUrlRegularExpressions = @"^(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The cookie container.
        /// 1. cookie容器
        /// 2. System.Net.CookieCollection 对象的集合提供容器
        /// </summary>
        private readonly CookieContainer cookieContainer;

        /// <summary>
        /// The random.
        /// 随机数生成器。
        /// </summary>
        private readonly Random random;

        /// <summary>
        /// The threads.
        /// 线程数组。
        /// </summary>
        private readonly Thread[] threads;

        /// <summary>
        /// The thread status.
        /// 线程状态数组。
        /// </summary>
        private readonly bool[] threadStatus;

        #endregion Fields

        #region Constructors and Destructors

        /// <summary>
        /// 初始化 +CrawlMaster(CrawlSettings settings)
        /// 1.cookieContainer
        /// 2.random
        /// 3.Settings
        /// 4.threads
        /// 5.threadStatus
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public CrawlMaster(CrawlSettings settings)
        {
            this.cookieContainer = new CookieContainer();
            this.random = new Random();
            this.Settings = settings;
            this.threads = new Thread[settings.ThreadCount];
            this.threadStatus = new bool[settings.ThreadCount];
        }

        #endregion Constructors and Destructors

        #region Public Events

        /// <summary>
        /// The add url event.
        /// </summary>
        public event AddUrlEventHandler AddUrlEvent;

        /// <summary>
        /// The crawl error event.
        /// </summary>
        public event CrawlErrorEventHandler CrawlErrorEvent;

        /// <summary>
        /// The data received event.
        /// </summary>
        public event DataReceivedEventHandler DataReceivedEvent;

        /// <summary>
        /// 自定义超链接处理事件
        /// </summary>
        public event CustomParseLinkEventHandler CustomParseLinkEvent;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the settings.
        /// </summary>
        public CrawlSettings Settings { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        /// <summary>
        /// The crawl.
        /// </summary>
        public void Crawl()
        {
            this.Initialize();

            for (int i = 0; i < this.threads.Length; i++)
            {
                this.threads[i].Start(i);
                this.threadStatus[i] = false;
            }
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            foreach (Thread thread in this.threads)
            {
                thread.Abort();
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        /// <summary>
        /// The config request.
        /// 配置request对象。
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        private void ConfigRequest(HttpWebRequest request)
        {
            request.UserAgent = this.Settings.UserAgent;
            request.CookieContainer = this.cookieContainer;
            request.AllowAutoRedirect = true;
            request.MediaType = "text/html";
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8";

            if (this.Settings.Timeout > 0)
            {
                request.Timeout = this.Settings.Timeout;
            }
        }

        /// <summary>
        /// The crawl process.
        /// </summary>
        /// <param name="threadIndex">
        /// The thread index.
        /// </param>
        private void CrawlProcess(object threadIndex)
        {
            var currentThreadIndex = (int)threadIndex;
            while (true)
            {
                // 根据队列中的 Url 数量和空闲线程的数量，判断线程是睡眠还是退出
                if (UrlQueue.Instance.Count == 0)
                {
                    this.threadStatus[currentThreadIndex] = true;
                    if (!this.threadStatus.Any(t => t == false))
                    {
                        break;
                    }

                    Thread.Sleep(2000);
                    continue;
                }

                this.threadStatus[currentThreadIndex] = false;

                if (UrlQueue.Instance.Count == 0)
                {
                    continue;
                }

                UrlInfo urlInfo = UrlQueue.Instance.DeQueue();

                HttpWebRequest request = null;
                HttpWebResponse response = null;

                try
                {
                    if (urlInfo == null)
                    {
                        continue;
                    }

                    // 1~5 秒随机间隔的自动限速
                    if (this.Settings.AutoSpeedLimit)
                    {
                        int span = this.random.Next(1000, 5000);
                        Thread.Sleep(span);
                    }

                    // 创建并配置Web请求
                    request = WebRequest.Create(urlInfo.UrlString) as HttpWebRequest;
                    this.ConfigRequest(request);//方法：引用参数的变身房。

                    if (request != null)
                    {
                        response = request.GetResponse() as HttpWebResponse;
                    }

                    if (response != null)//保证有数据返回，获得了响应
                    {
                        this.PersistenceCookie(response);//维持cookie

                        Stream stream = null;

                        // 如果页面压缩，则解压数据流
                        if (response.ContentEncoding == "gzip")
                        {
                            Stream responseStream = response.GetResponseStream();
                            if (responseStream != null)
                            {
                                stream = new GZipStream(responseStream, CompressionMode.Decompress);
                            }
                        }
                        else
                        {
                            stream = response.GetResponseStream();
                        }

                        using (stream)
                        {
                            string html = this.ParseContent(stream, response.CharacterSet); /// 将流转化为字符串，包含了编码情况处理（如果页面有乱码，在这里处理）

                            this.ParseLinks(urlInfo, html);//根据页面html和urlInfo，开启将链接添加到url爬行队列

                            if (this.DataReceivedEvent != null)
                            {
                                //在这里得到了数据。
                                this.DataReceivedEvent(
                                    new DataReceivedEventArgs
                                    {
                                        Url = urlInfo.UrlString,
                                        Depth = urlInfo.Depth,
                                        Html = html
                                    });
                                if (this.CustomParseLinkEvent != null)
                                {
                                    //自定义事件，将DataReceivedEventArgs传入，将处理过程暴露给外部，将自定义方法处理获得的UrlInfo（主要是url）加入队列
                                    var customUrlInfo = this.CustomParseLinkEvent(new DataReceivedEventArgs
                                    {
                                        Url = urlInfo.UrlString,
                                        Depth = urlInfo.Depth,
                                        Html = html
                                    });
                                    if (customUrlInfo != null)
                                    {
                                        UrlQueue.Instance.EnQueue(customUrlInfo);
                                    }
                                }
                            }

                            if (stream != null)
                            {
                                stream.Close();
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (this.CrawlErrorEvent != null)
                    {
                        if (urlInfo != null)
                        {
                            this.CrawlErrorEvent(
                                new CrawlErrorEventArgs { Url = urlInfo.UrlString, Exception = exception });
                        }
                    }
                }
                finally
                {
                    if (request != null)
                    {
                        request.Abort();
                    }

                    if (response != null)
                    {
                        response.Close();
                    }
                }
            }
        }

        /// <summary>
        /// The parse links.
        /// 超链接解析
        /// 通用的超链接解析；没有针对特定的超链接解析暴露接口
        /// </summary>
        /// <param name="urlInfo">
        /// The url info.
        /// </param>
        /// <param name="html">
        /// The html.
        /// </param>
        private void ParseLinks(UrlInfo urlInfo, string html)
        {
            //设置参数大于0，并且 url深度 大于等于 设置深度==》同时满足==》退出
            //设置参数小于等于0，或者url深度 小于 设置深度（2）==》执行下面处理
            if (this.Settings.Depth > 0 && urlInfo.Depth >= this.Settings.Depth)
            {
                return;
            }

            var urlDictionary = new Dictionary<string, string>();//<href,text>

            Match match = Regex.Match(html, "(?i)<a .*?href=\"([^\"]+)\"[^>]*>(.*?)</a>");//"(?i)<a .*?href=\"([^\"]+)\"[^>]*>(.*?)</a>"超链接正则表达式
            while (match.Success)
            {
                // 以 href 作为 key
                string urlKey = match.Groups[1].Value;

                // 以 text 作为 value
                string urlValue = Regex.Replace(match.Groups[2].Value, "(?i)<.*?>", string.Empty);

                urlDictionary[urlKey] = urlValue;
                match = match.NextMatch();//从上一个匹配结束的位置（即在上一个匹配字符之后的字符）开始返回一个包含下一个匹配结果的新
            }

            foreach (var item in urlDictionary)
            {
                string href = item.Key;
                string text = item.Value;

                if (!string.IsNullOrEmpty(href))
                {
                    bool canBeAdd = true;

                    if (this.Settings.EscapeLinks != null && this.Settings.EscapeLinks.Count > 0)//有忽略链接
                    {                                                             //（suffix：后缀；StringComparison.OrdinalIgnoreCase：忽略大小写，进行比较）
                        if (this.Settings.EscapeLinks.Any(suffix => href.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))// 确定序列中的任何元素是否都满足条件。
                                                                                                                               //满足条件，不添加；即有满足忽略后缀的超链接，不添加
                        {
                            canBeAdd = false;
                        }
                    }

                    if (this.Settings.HrefKeywords != null && this.Settings.HrefKeywords.Count > 0)//有连接关键词
                    {
                        if (!this.Settings.HrefKeywords.Any(href.Contains))//不包含关键字，不添加。原来关键字这么重要。
                        {
                            canBeAdd = false;
                        }
                    }

                    if (canBeAdd)
                    {
                        string url = href.Replace("%3f", "?")
                            .Replace("%3d", "=")
                            .Replace("%2f", "/")
                            .Replace("&amp;", "&");

                        if (string.IsNullOrEmpty(url) || url.StartsWith("#")
                            || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                            || url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var baseUri = new Uri(urlInfo.UrlString);
                        Uri currentUri = url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                             ? new Uri(url)
                                             : new Uri(baseUri, url);//根据指定的基 URI 和相对 URI 字符串，初始化 System.Uri 类的新实例。
                                                                     //如果不包含http，则认为超链接是相对路径，根据baseUrl建立绝对路径

                        url = currentUri.AbsoluteUri;

                        if (this.Settings.LockHost)
                        {
                            // 去除二级域名后，判断域名是否相等，相等则认为是同一个站点
                            // 例如：mail.pzcast.com 和 www.pzcast.com
                            if (baseUri.Host.Split('.').Skip(1).Aggregate((a, b) => a + "." + b)
                                != currentUri.Host.Split('.').Skip(1).Aggregate((a, b) => a + "." + b))
                            {
                                continue;
                            }
                        }

                        if (!this.IsMatchRegular(url))//如果不匹配
                        {
                            continue;
                        }

                        #region addUrlEventArgs

                        var addUrlEventArgs = new AddUrlEventArgs { Title = text, Depth = urlInfo.Depth + 1, Url = url };
                        if (this.AddUrlEvent != null && !this.AddUrlEvent(addUrlEventArgs))
                        {
                            continue;
                        }

                        #endregion addUrlEventArgs

                        //经过上面的一系列处理，决定将url加入队列
                        UrlQueue.Instance.EnQueue(new UrlInfo(url) { Depth = urlInfo.Depth + 1 });
                    }
                }
            }
        }

        /// <summary>
        /// The initialize.
        /// </summary>
        private void Initialize()
        {
            if (this.Settings.SeedsAddress != null && this.Settings.SeedsAddress.Count > 0)
            {
                foreach (string seed in this.Settings.SeedsAddress)
                {
                    if (Regex.IsMatch(seed, WebUrlRegularExpressions, RegexOptions.IgnoreCase))
                    {
                        UrlQueue.Instance.EnQueue(new UrlInfo(seed) { Depth = 1 });
                    }
                }
            }

            for (int i = 0; i < this.Settings.ThreadCount; i++)
            {
                var threadStart = new ParameterizedThreadStart(this.CrawlProcess);

                this.threads[i] = new Thread(threadStart);
            }

            ServicePointManager.DefaultConnectionLimit = 256;
        }

        /// <summary>
        /// The is match regular.
        /// 根据Settings中的过滤规则验证url
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsMatchRegular(string url)
        {
            bool result = false;

            if (this.Settings.RegularFilterExpressions != null && this.Settings.RegularFilterExpressions.Count > 0)//过滤规则存在
            {
                if (this.Settings.RegularFilterExpressions.Any(pattern => Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase)))
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// The parse content.
        /// 将流转化为字符串
        /// 注意了编码情况（如果页面有乱码，在这里处理）
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="characterSet">
        /// The character set.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ParseContent(Stream stream, string characterSet)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            byte[] buffer = memoryStream.ToArray();

            Encoding encode = Encoding.ASCII;
            string html = encode.GetString(buffer);

            string localCharacterSet = characterSet;

            Match match = Regex.Match(html, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                localCharacterSet = match.Groups[2].Value;

                var stringBuilder = new StringBuilder();
                foreach (char item in localCharacterSet)
                {
                    if (item == ' ')
                    {
                        break;
                    }

                    if (item != '\"')
                    {
                        stringBuilder.Append(item);
                    }
                }

                localCharacterSet = stringBuilder.ToString();
            }

            if (string.IsNullOrEmpty(localCharacterSet))
            {
                localCharacterSet = characterSet;
            }

            if (!string.IsNullOrEmpty(localCharacterSet))
            {
                encode = Encoding.GetEncoding(localCharacterSet);
            }

            memoryStream.Close();

            return encode.GetString(buffer);
        }

        /// <summary>
        /// The persistence（维持） cookie.
        /// 维持cookie
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        private void PersistenceCookie(HttpWebResponse response)
        {
            if (!this.Settings.KeepCookie)
            {
                return;
            }

            string cookies = response.Headers["Set-Cookie"];
            if (!string.IsNullOrEmpty(cookies))
            {
                var cookieUri =
                    new Uri(
                        string.Format(
                            "{0}://{1}:{2}/",
                            response.ResponseUri.Scheme,
                            response.ResponseUri.Host,
                            response.ResponseUri.Port));

                this.cookieContainer.SetCookies(cookieUri, cookies);
            }
        }

        #endregion Methods
    }
}