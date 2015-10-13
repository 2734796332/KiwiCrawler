using DotNet4.Utilities;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArgsTool.WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            txtUrl.Text = "http://www.shgtj.gov.cn/2011/gcjsxx/xmxx/ghxzyj/";
            txtHtml.Language = Language.HTML;
            txtHtml.ImeMode = System.Windows.Forms.ImeMode.On;
            txtHtml.Font = new System.Drawing.Font("Consolas", 9.75F);
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            string html = "";
            //URL处理
            if (IsUrlable(url))
            {
                html = GetHtml(url);
                //string cookie = result.Cookie;
                txtHtml.Text = "";
                txtHtml.Text = html;
            }
            //正则表达式处理
            string regStr = txtChecked.Text.Trim();
            if (!String.IsNullOrEmpty(regStr) && !String.IsNullOrEmpty(html) && IsUrlable(url))
            {
                int group = -1;
                if (!Int32.TryParse(txtGroup.Text, out group))
                {
                    group = -1;
                }
                try
                {
                    txtResult.Text = "";
                    txtResult.Text = CustomGetLink(html, url, regStr, group).Trim();
                    if (String.IsNullOrEmpty(txtResult.Text))
                    {
                        MessageBox.Show("没有匹配到内容");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("正则表达式不通过");
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private static string CustomGetLink(string html, string baseUrl, string patternStr, int groupIndex)
        {
            string url = "";
            string returnUrl = "";
            if (html != null && !string.IsNullOrEmpty(html))
            {
                Regex regex = new Regex(patternStr);
                MatchCollection mats = regex.Matches(html);
                foreach (Match mat in mats)
                {
                    if (mat.Success)
                    {
                        url = groupIndex != -1 ? mat.Groups[groupIndex].Value : mat.Groups[0].Value;
                        var baseUri = new Uri(baseUrl);
                        Uri currentUri = url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                             ? new Uri(url)
                                             : new Uri(baseUri, url);//根据指定的基 URI 和相对 URI 字符串，初始化 System.Uri 类的新实例。
                                                                     //如果不包含http，则认为超链接是相对路径，根据baseUrl建立绝对路径
                        returnUrl += currentUri.AbsoluteUri + "\r\n";
                    }
                }

                //Match mat = regex.Match(html);
                //if (mat.Success)
                //{
                //    url = groupIndex != -1 ? mat.Groups[groupIndex].Value : mat.Groups[0].Value;
                //    var baseUri = new Uri(baseUrl);
                //    Uri currentUri = url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                //                         ? new Uri(url)
                //                         : new Uri(baseUri, url);//根据指定的基 URI 和相对 URI 字符串，初始化 System.Uri 类的新实例。
                //                                                 //如果不包含http，则认为超链接是相对路径，根据baseUrl建立绝对路径
                //    url = currentUri.AbsoluteUri;
                //}
            }
            return returnUrl;
        }

        private static string GetHtml(string url)
        {
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,// "http://www.shgtj.gov.cn/2011/gcjsxx/xmxx/ghxzyj/",//URL     必需项
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
            return result.Html;
        }

        private static bool IsUrlable(string str)
        {
            return Regex.IsMatch(str, @"^http://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$");
        }

        private void txtUrl_MouseEnter(object sender, EventArgs e)
        {
            txtUrl.Select();
        }
    }
}