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
        //host���ݷָ���
        private string fg = "*";

        //����hosts�ļ�·��
        private string hostFilePath = "C:\\Windows\\System32\\drivers\\etc\\hosts";

        //hosts�ļ��п���(��ע��)�����б�
        private ArrayList hostFileData = new ArrayList();

        //UI���������б�
        private ArrayList dataList = new ArrayList();

        //Ҫ�Ƴ��������б�  
        private Dictionary<string, string> deleteList = new Dictionary<string, string>();

        //Ҫ����������б�
        private ArrayList addList = new ArrayList();

        //Զ�������ļ�������ʱ������� 
        private string configData;

        //���������ļ�(win)
        private string configPath; 

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string osInfo = System.Environment.OSVersion.ToString();
            //��XPϵͳ
            if (osInfo.IndexOf("5.1") != -1)  
            {
                this.configPath = "C:\\Documents and Settings\\Administrator\\AppData\\LocalLow\\edithost.config";
            }
            //��XPϵͳ��win7��win10�ȣ�
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
         * �ֶ��޸İ�ť����¼�
         */
        private void edit_Click(object sender, EventArgs e)
        {
            //ʹ�ü��±���hosts�ļ�
            System.Diagnostics.Process.Start("notepad.exe", this.hostFilePath);
        }

        /**
         * ȷ����ť����¼�
         */
        private void confirm_Click(object sender, EventArgs e)
        {
            //���¶�ȡhosts�ļ�����
            this.readHostsFile();
            //���addList
            this.addList.Clear();
            //���deleteList
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
                    MessageBox.Show("checkbox�ؼ����ڲ�ȷ��״̬");
                }
            }

            //ɾ��Ҫɾ������
            //���Ҫ��ӵ���
            if (this.deleteHostLines() && this.addHostLines())
            {
                MessageBox.Show("�޸ĳɹ�");
            }
            
        }


        /**
         * �رհ�ť����¼�
         */
        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /**
         * ���ڰ�ť����¼�
         */
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        /**
         * ˢ�°�ť����¼�
         */
        private void update_Click(object sender, EventArgs e)
        {
            this.pictureBox.Hide();
            //��һ�� ���������ļ�
            this.getConfigFile();
            //�ڶ��� ����dataList
            this.setDataList();
            //������ ����UI
            this.updateUIlist();
            this.pictureBox.Show();
        }

        /**
         * ˢ�°�ť��ʾ����
         */
         private void update_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.pictureBox, "ˢ��");
        }

        /**
         * ��hosts�ļ����ݶ������������
         */
        private void readHostsFile()
        {
            //���hostFileData
            this.hostFileData.Clear();

            //��ȡhosts�ļ�
            ArrayList hostsDatasTmp = this.readFile(this.hostFilePath);

            foreach (string str in hostsDatasTmp)
            {
                string lineTemp = str.Replace(" ", "").Replace("\t", "");
                //ȥ�����к�ע����
                if (lineTemp == "" || lineTemp.Substring(0, 1) == "#")
                {
                    continue;
                }
                this.hostFileData.Add(lineTemp);
            }

        }

        /**
         * ��Ҫɾ������ɾ��
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

            //����д��
            return this.writeFile(this.hostFilePath, tmp, FileMode.Create);
        }

        /**
         * ��Ҫ��ӵ������
         */
        private Boolean addHostLines()
        {
            return this.writeFile(this.hostFilePath, this.addList, FileMode.Append);
        }

        /**
         * ��ȡԶ�������ļ������浽����
         */
        private void getConfigFile()
        {
            string url = "http://xxx.com/edithost.config";

            try
            {
                HttpWebRequest request;
                // ����һ��HTTP����
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
            //����ȡ�������ļ����浽����
            if (!this.writeFile(this.configPath, tmp, FileMode.Create))
            {
                MessageBox.Show("�����ļ�д��ʧ��");
            }
        }

        /**
         * ����dataList��������
         * ���EditHost.config�ļ������������ݣ��������ݸ��µ�dataList��
         * ���EditHost.config�ļ������ڻ�����Ϊ�գ���ʹ��Ĭ����������dataList
         */
         private void setDataList()
        {
            //�����dataList
            this.dataList.Clear();

            if (File.Exists(this.configPath))  //�ļ�����
            {
                //��ȡ�ļ�����
                ArrayList lineTmp = this.readFile(this.configPath);

                ArrayList tmp = new ArrayList();

                for (int i = 0; i < lineTmp.Count; i++)
                {
                    //ȥ�����к�ע����
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
              
                //����ļ���Ϊ��
                if (tmp.Count != 0)
                {
                    this.dataList = tmp;//���ø�ֵ
                    return;
                }
            }

            //��ʼ��dataList����
            this.dataList.Add("33.66.99.88" + this.fg + "www.taobao.com" + this.fg + "�Ա�ָ��88������");
            
        }

        /**
         * ����UI�б�
         */
         private void updateUIlist()
        {
            //�����UI�б�����
            panel1.Controls.Clear();
            //���¶�ȡhosts�ļ�����
            this.readHostsFile();

            for (int i = 0; i < this.dataList.Count; i++)
            {
                int len = 22;
                CheckBox chk = new CheckBox();
                chk.AutoSize = true;
                chk.Location = new System.Drawing.Point(15, len * i + 10);//λ�� 
                //chk.Size = new System.Drawing.Size(78, 16);//��С 
                string str = this.dataList[i].ToString();
                string[] strArr = str.Split(new string[] { this.fg }, StringSplitOptions.None);

                chk.Checked = false;
                if (this.hostFileData.Contains(strArr[0] + strArr[1]))
                {
                    chk.Checked = true;
                }
                
                chk.Text = strArr[2];//���� 
                chk.Tag = strArr[0] + " " + strArr[1];
                panel1.Controls.Add(chk);
            }
        }


        /**
         * ��ȡ�ļ�
         * @file : �ļ���������·����
         * @return : ����ArrayList
         */
        private ArrayList readFile(string file)
        {
            //������ʱArrayList
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
         * д���ļ�
         * @file : �ļ���
         * @data : ����
         * @mode : д��ģʽ����дFileMode.OpenOrCreate��׷��FileMode.Append��
         * @return : Boolean
         */
        private Boolean writeFile(string file,ArrayList data, FileMode mode)
        {
            try
            {
                FileStream fs = new FileStream(file, mode, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs); // ����д����
                foreach (string l in data)
                {
                    sw.WriteLine(l); // д��
                }
                sw.Close(); //�ر��ļ�
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
