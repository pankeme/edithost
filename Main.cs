using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Net;


namespace EditHost
{
    public partial class Main : Form
    {
        //host数据分隔符
        private string fg = "*";

        //本地hosts文件路径
        private string hostFilePath = "C:\\Windows\\System32\\drivers\\etc\\hosts";

        //hosts文件中可用(非注释)数据列表
        private ArrayList hostFileData = new ArrayList();

        //UI基本数据列表
        private ArrayList dataList = new ArrayList();

        //要移除数据行列表  
        private Dictionary<string, string> deleteList = new Dictionary<string, string>();

        //要添加数据行列表
        private ArrayList addList = new ArrayList();

        //远程配置文件数据临时保存变量 
        private string configData;

        //本地配置文件(win)
        private string configPath; 

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string osInfo = System.Environment.OSVersion.ToString();
            //是XP系统
            if (osInfo.IndexOf("5.1") != -1)  
            {
                this.configPath = "C:\\Documents and Settings\\Administrator\\AppData\\LocalLow\\edithost.config";
            }
            //非XP系统（win7、win10等）
            else
            {
                this.configPath = "C:\\Users\\Administrator\\AppData\\LocalLow\\edithost.config";
            }

            this.readHostsFile();
            this.setDataList();
            this.updateUIlist();
        }

        private void btnFormMax_Click(object sender, EventArgs e)
        {

        }

        /**
         * 手动修改按钮点击事件
         */
        private void edit_Click(object sender, EventArgs e)
        {
            //使用记事本打开hosts文件
            System.Diagnostics.Process.Start("notepad.exe", this.hostFilePath);
        }

        /**
         * 确定按钮点击事件
         */
        private void confirm_Click(object sender, EventArgs e)
        {
            //重新读取hosts文件内容
            this.readHostsFile();
            //清空addList
            this.addList.Clear();
            //清空deleteList
            this.deleteList.Clear();

            foreach (CheckBox chk in panel1.Controls)
            {
                if (chk.CheckState == CheckState.Checked) {
                    if (!this.hostFileData.Contains(chk.Tag.ToString().Replace(" ", "")))
                    {
                        this.addList.Add(chk.Tag);
                    }

                } else if (chk.CheckState == CheckState.Unchecked){
                    if (this.hostFileData.Contains(chk.Tag.ToString().Replace(" ", "")))
                    {
                        this.deleteList.Add(chk.Tag.ToString().Replace(" ", ""),chk.Tag.ToString());
                    }

                } else {
                    MessageBox.Show("checkbox控件处于不确定状态");
                }
            }

            //删除要删除的行
            //添加要添加的行
            if (this.deleteHostLines() && this.addHostLines())
            {
                MessageBox.Show("修改成功");
            }
            
        }


        /**
         * 关闭按钮点击事件
         */
        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /**
         * 关于按钮点击事件
         */
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        /**
         * 刷新按钮点击事件
         */
        private void update_Click(object sender, EventArgs e)
        {
            this.pictureBox.Hide();
            //第一步 更新配置文件
            this.getConfigFile();
            //第二步 设置dataList
            this.setDataList();
            //第三步 更新UI
            this.updateUIlist();
            this.pictureBox.Show();
        }

        /**
         * 刷新按钮提示文字
         */
         private void update_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.pictureBox, "刷新");
        }

        /**
         * 将hosts文件内容读到数组变量中
         */
        private void readHostsFile()
        {
            //清空hostFileData
            this.hostFileData.Clear();

            //读取hosts文件
            ArrayList hostsDatasTmp = this.readFile(this.hostFilePath);

            foreach (string str in hostsDatasTmp)
            {
                string lineTemp = str.Replace(" ", "").Replace("\t", "");
                //去掉空行和注释行
                if (lineTemp == "" || lineTemp.Substring(0, 1) == "#")
                {
                    continue;
                }
                this.hostFileData.Add(lineTemp);
            }

        }

        /**
         * 将要删除的行删除
         */
        private Boolean deleteHostLines()
        {
            ArrayList hostDataTmp = this.readFile(this.hostFilePath);
            ArrayList tmp = new ArrayList();

            foreach (string l in hostDataTmp)
            {
                if (!this.deleteList.ContainsKey(l.Replace("\t", "").Replace(" ", "")))
                {
                    tmp.Add(l);
                }
            }

            //重新写入
            return this.writeFile(this.hostFilePath, tmp, FileMode.Create);
        }

        /**
         * 将要添加的行添加
         */
        private Boolean addHostLines()
        {
            return this.writeFile(this.hostFilePath, this.addList, FileMode.Append);
        }

        /**
         * 读取远程配置文件并保存到本地
         */
        private void getConfigFile()
        {
            string url = "http://xxx.com/edithost.config";

            try
            {
                HttpWebRequest request;
                // 创建一个HTTP请求
                request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                this.configData = myreader.ReadToEnd();
                myreader.Close();
            } catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            ArrayList tmp = new ArrayList();
            tmp.Add(this.configData);
            //将获取的配置文件保存到本地
            if (!this.writeFile(this.configPath, tmp, FileMode.Create))
            {
                MessageBox.Show("配置文件写入失败");
            }
        }

        /**
         * 设置dataList数据内容
         * 如果EditHost.config文件存在且有内容，则将其内容更新到dataList中
         * 如果EditHost.config文件不存在或内容为空，则使用默认配置设置dataList
         */
         private void setDataList()
        {
            //先清空dataList
            this.dataList.Clear();

            if (File.Exists(this.configPath))  //文件存在
            {
                //读取文件内容
                ArrayList lineTmp = this.readFile(this.configPath);

                ArrayList tmp = new ArrayList();

                for (int i = 0; i < lineTmp.Count; i++)
                {
                    //去掉空行和注释行
                    if ((string)lineTmp[i] == "" || lineTmp[i].ToString().Substring(0, 1) == "#")
                    {
                        continue;
                    } else
                    {
                        string[] t = lineTmp[i].ToString().Split(new string[] { this.fg }, StringSplitOptions.None);
                        if (t.Length != 3)
                        {
                            continue;
                        }
                        tmp.Add(lineTmp[i]);
                    }
                }
              
                //如果文件不为空
                if (tmp.Count != 0)
                {
                    this.dataList = tmp;//引用赋值
                    return;
                }
            }

            //初始化dataList数据
            this.dataList.Add("33.66.99.88" + this.fg + "www.taobao.com" + this.fg + "淘宝指向88服务器");
            
        }

        /**
         * 更新UI列表
         */
         private void updateUIlist()
        {
            //先清空UI列表数据
            panel1.Controls.Clear();
            //重新读取hosts文件内容
            this.readHostsFile();

            for (int i = 0; i < this.dataList.Count; i++)
            {
                int len = 22;
                CheckBox chk = new CheckBox();
                chk.AutoSize = true;
                chk.Location = new System.Drawing.Point(15, len * i + 10);//位置 
                //chk.Size = new System.Drawing.Size(78, 16);//大小 
                string str = this.dataList[i].ToString();
                string[] strArr = str.Split(new string[] { this.fg }, StringSplitOptions.None);

                chk.Checked = false;
                if (this.hostFileData.Contains(strArr[0] + strArr[1]))
                {
                    chk.Checked = true;
                }
                
                chk.Text = strArr[2];//内容 
                chk.Tag = strArr[0] + " " + strArr[1];
                panel1.Controls.Add(chk);
            }
        }


        /**
         * 读取文件
         * @file : 文件名（包括路径）
         * @return : 数据ArrayList
         */
        private ArrayList readFile(string file)
        {
            //定义临时ArrayList
            ArrayList dataTmp = new ArrayList();
            StreamReader sr = new StreamReader(file, System.Text.Encoding.UTF8);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                dataTmp.Add(line);
            }
            sr.Close();
            return dataTmp;
        }

        /**
         * 写入文件
         * @file : 文件名
         * @data : 数据
         * @mode : 写入模式（覆写FileMode.OpenOrCreate、追加FileMode.Append）
         * @return : Boolean
         */
        private Boolean writeFile(string file,ArrayList data, FileMode mode)
        {
            try
            {
                FileStream fs = new FileStream(file, mode, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs); // 创建写入流
                foreach (string l in data)
                {
                    sw.WriteLine(l); // 写入
                }
                sw.Close(); //关闭文件
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            return true;
        }


    }
}
