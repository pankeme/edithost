using System;
using System.Windows.Forms;

namespace EditHost
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        /**
         * 关闭按钮点击事件
         */
        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /**
         * panke名称点击事件
         */
        private void panke_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //调用系统默认的浏览器   
            System.Diagnostics.Process.Start("http://panke.me");
        }
    }
}
