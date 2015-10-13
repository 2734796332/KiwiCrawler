using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zoyobar.shared.panzer.web.ib;

namespace Demo.Browser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private IEBrowser ie;

        private void btnStart_Click(object sender, EventArgs e)
        {
            // ie = new IEBrowser(this.webBrowser);
            webBrowser.Navigate("http://www.shgtj.gov.cn/2011/gcjsxx/xmxx/ghxzyj/index.html");
            string a = webBrowser.DocumentText;
            Console.WriteLine(a);
        }
    }
}