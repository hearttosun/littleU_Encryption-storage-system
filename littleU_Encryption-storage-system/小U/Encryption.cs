using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ADOX;// 引用COM：Microsoft ADO Ext. 2.8 for DDL and Security 
using BLL;
using System.Diagnostics;


namespace 小U
{
    public partial class Encryption : Form
    {
        List<string> fileList = new List<string>(); //操作文件列表
        static public bool enIng = false;//判断加密是否进行中
        private long num = 0; //所处理文件字节数
        private long num2 = 0; //所处理文件字节数(为了加密时计算速度)
        private FileInfo fileInfo;
        DateTime time = new DateTime();//计时
        ListViewItem item;//ListView控件项
        float ftime;
        Thread th, th2;

        public Encryption()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (enIng == true)
                {
                    MessageBox.Show("正在加密！请稍等！");
                    return;
                }
                if ((Start.AFileAuthority == "2" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "2" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "2" || Start.CFileAuthority == "4"))
                {
                    MessageBox.Show("您不具备任何加密权限");
                    return;
                }
                if (fileList.Count == 0)
                    listView1.Items.Clear();
                progressBar1.Visible = false;
                label1.Text = "";
                label6.Text = "";
                label3.Text = "";
                label9.Text = "";
                OpenFileDialog open = new OpenFileDialog();
                open.Title = "选择操作文件";
                open.InitialDirectory = Start.panFu;//初始目录
                open.Filter = "所有文件|*.*";//过滤器
                open.Multiselect = false;//是否可以多选
                if (open.ShowDialog() == DialogResult.Cancel)
                    return;
                if (open.FileName == Start.CheckFile || open.FileName == Start.DBFile || open.FileName == Start.ReadMeFile || open.FileName == Process.GetCurrentProcess().MainModule.FileName ||
                    open.FileName == System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "SM3DLL.dll" || open.FileName == System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "SM4DLL.dll") 
                {
                    MessageBox.Show("不能对该文件进行加密！");
                    return;
                }
                if (open.FileName.Substring(open.FileName.Length - 2, 2) == "_A" || open.FileName.Substring(open.FileName.Length - 2, 2) == "_B" || open.FileName.Substring(open.FileName.Length - 2, 2) == "_C")
                {
                    MessageBox.Show("该文件是加密文件,请勿再次加密！");
                    return;
                }
                if (fileList.Count == 0)
                {
                    fileList.Add(open.FileName);
                    fileInfo = new FileInfo(open.FileName);
                    num += fileInfo.Length;
                    item = new ListViewItem(fileList.Count.ToString());
                    item.SubItems.Add(fileInfo.Name);
                    item.SubItems.Add("未加密");
                    listView1.Items.Add(item);
                }
                else
                {
                    //防止重选
                    foreach (string ss in fileList)
                    {
                        if (ss == open.FileName)
                            return;
                    }
                    fileList.Add(open.FileName);
                    fileInfo = new FileInfo(open.FileName);
                    item = new ListViewItem(fileList.Count.ToString());
                    item.SubItems.Add(fileInfo.Name);
                    item.SubItems.Add("未加密");
                    listView1.Items.Add(item);                
                    num += fileInfo.Length;
                }
                label5.Text = "共" + fileList.Count.ToString() + "个文件";
                if (num < 1024)
                    label5.Text += "  数据量：" + num.ToString() + " B";
                if ((num / 1024) >= 1 && (num / 1024) < 1024)
                    label5.Text += "  数据量：" + ((float)num / 1024).ToString() + " KB";
                if ((num / 1024 / 1024) >= 1 && (num / 1024 / 1024) < 1024)
                    label5.Text += "  数据量：" + ((float)num / 1024 / 1024).ToString() + " MB";
                if ((num / 1024 / 1024) > 1024)
                    label5.Text += "  数据量：" + ((float)num / 1024 / 1024 / 1024).ToString() + " GB";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Start.FolderAuthority == "2" || Start.FolderAuthority == "4")
                {
                    MessageBox.Show("您无权进行此操作，请勿越权操作！");
                    return;
                }
                if (enIng == true)
                {
                    MessageBox.Show("正在加密！请稍等！");
                    return;
                }
                if ((Start.AFileAuthority == "2" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "2" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "2" || Start.CFileAuthority == "4"))
                {
                    MessageBox.Show("您不具备任何加密权限");
                    return;
                }
                if (fileList.Count == 0)
                    listView1.Items.Clear();
                progressBar1.Visible = false;
                label1.Text = "";
                label6.Text = "";
                label3.Text = "";
                label9.Text = "";
                //选择文件夹中所有文件
                FolderBrowserDialog f = new FolderBrowserDialog();
                f.SelectedPath = Start.panFu;
                if (f.ShowDialog() == DialogResult.Cancel)
                    return;
                if (f.SelectedPath == Start.path)
                {
                    MessageBox.Show("不能对该目录下文件进行加密！");
                    return;
                }
                //遍历当前文件夹下文件
                FindFile(f.SelectedPath);
                label5.Text = "共" + fileList.Count.ToString() + "个文件";
                if (num < 1024)
                    label5.Text += "  数据量：" + num.ToString() + " B";
                if ((num / 1024) >= 1 && (num / 1024) < 1024)
                    label5.Text += "  数据量：" + ((float)num / 1024).ToString() + " KB";
                if ((num / 1024 / 1024) >= 1 && (num / 1024 / 1024) < 1024)
                    label5.Text += "  数据量：" + ((float)num / 1024 / 1024).ToString() + " MB";
                if ((num / 1024 / 1024) > 1024)
                    label5.Text += "  数据量：" + ((float)num / 1024 / 1024 / 1024).ToString() + " GB";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void FindFile(string dirPath) //参数dirPath为指定的目录
        {
            bool error;
            string s;
            //在指定目录及子目录下查找文件,在listBox1中列出子目录及文件
            DirectoryInfo Dir = new DirectoryInfo(dirPath);
            try
            {
                foreach (FileInfo f in Dir.GetFiles()) //查找文件
                {
                    s = Dir + @"\" + f.ToString();
                    error = false;
                    if (s.Substring(s.Length - 2, 2) == "_A" || s.Substring(s.Length - 2, 2) == "_B" || s.Substring(s.Length - 2, 2) == "_C")
                        error = true;
                    if (s == Start.CheckFile || s == Start.DBFile || s == Start.ReadMeFile || s == Process.GetCurrentProcess().MainModule.FileName ||
                    s == System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "SM3DLL.dll" || s == System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "SM4DLL.dll") 
                        error = true;
                    //防止重选
                    foreach (string ss in fileList)
                    {
                        if (ss == s)
                            error = true;
                    }

                    if (error == true) continue;
                    if (s == (Start.panFu + System.AppDomain.CurrentDomain.SetupInformation.ApplicationName)) continue;
                    fileList.Add(s);
                    item = new ListViewItem(fileList.Count.ToString());
                    item.SubItems.Add(f.ToString());
                    item.SubItems.Add("未加密");
                    listView1.Items.Add(item);
                    fileInfo = new FileInfo(s);
                    num += fileInfo.Length;
                }
                foreach (DirectoryInfo d in Dir.GetDirectories())//查找子目录 
                {
                    FindFile(Dir + @"\" + d.ToString());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void RoleControl()
        {
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
            radioButton3.Enabled = false;
            if (Start.AFileAuthority == "1" || Start.AFileAuthority == "3")
                radioButton1.Enabled = true;
            if (Start.BFileAuthority == "1" || Start.BFileAuthority == "3")
                radioButton2.Enabled = true;
            if (Start.CFileAuthority == "1" || Start.CFileAuthority == "3")
                radioButton3.Enabled = true;
            if ((Start.AFileAuthority == "2" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "2" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "2" || Start.CFileAuthority == "4"))
            {
                MessageBox.Show("您不具备任何加密权限");
                return;
            }
            if (radioButton1.Enabled == false) radioButton1.Visible = false;
            if (radioButton2.Enabled == false) radioButton2.Visible = false;
            if (radioButton3.Enabled == false) radioButton3.Visible = false;
            if (radioButton1.Enabled == false && radioButton2.Enabled == false && radioButton3.Enabled == true)
                radioButton3.Checked = true;
            if (radioButton1.Enabled == false && radioButton3.Enabled == false && radioButton2.Enabled == true)
                radioButton2.Checked = true;
            if (radioButton2.Enabled == false && radioButton3.Enabled == false && radioButton1.Enabled == true)
                radioButton1.Checked = true;

            if (Start.FolderAuthority == "2" || Start.FolderAuthority == "4")
            {
                button1.Enabled = false;
                button1.Visible = false;
            }
        }

        private void JiaMi_Mian_Load(object sender, EventArgs e)
        {
            label5.Text = "";
            label6.Text = "";
            label1.Text = "";
            label3.Text = "";
            label9.Text = "";
            progressBar1.Visible = false;
            RoleControl();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (enIng == true)
            {
                MessageBox.Show("正在加密！请稍等！");
                return;
            }
            progressBar1.Visible = false;
            label1.Text = "";
            label6.Text = "";
            label5.Text = "";
            label3.Text = "";
            label9.Text = "";
            fileList.Clear();
            listView1.Items.Clear();
            num = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (Decryption.deIng == true)
                {
                    MessageBox.Show("正在进行解密！请等解密完成后再操作！");
                    return;
                }
                if (enIng == true)
                {
                    MessageBox.Show("正在加密！请稍等！");
                    return;
                }
                label1.Text = "";
                label3.Text = "";
                label6.Text = "";
                label9.Text = "";
                progressBar1.Visible = false;
                if (fileList.Count == 0)
                {
                    label5.Text = "";
                    listView1.Items.Clear();
                    MessageBox.Show("未选择文件");
                    return;
                }
                if (radioButton1.Checked == false && radioButton2.Checked == false && radioButton3.Checked == false)
                {
                    MessageBox.Show("未标记敏感等级！");
                    return;
                }
                if (fileList.Count != 0)
                {
                    //开始加密
                    enIng = true;
                    num2 = num;
                    label6.Text = "正在加密第一个文件";
                    label1.Text = "";
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    radioButton3.Enabled = false;
                    progressBar1.Visible = true;
                    th = new Thread(this.Encrypt);
                    Control.CheckForIllegalCrossThreadCalls = false;
                    th.Start();
                    th2 = new Thread(this.count);
                    Control.CheckForIllegalCrossThreadCalls = false;
                    th2.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void count()
        {
            try
            {
                string stime;
                while (true)
                {
                    stime = (DateTime.Now - time).TotalSeconds.ToString();
                    ftime = float.Parse(stime);
                    label3.Text = ftime.ToString() + " s";//计时
                    label3.Refresh();
                    if (num != 0)
                    {
                        if (num != 0) progressBar1.Value = System.Math.Abs((int)((SM4.EnSum * 100) / num));
                        if (num != 0) label1.Text = System.Math.Abs(((SM4.EnSum * 100) / num)).ToString() + "%";
                        progressBar1.Refresh();
                    }
                    if (progressBar1.Value == 100)
                    {
                        return;
                    }
                }
            }
            catch (Exception) { }
        }

        private void Encrypt()
        {
            OleDbConnection AconnStr = new OleDbConnection();
            try
            {
                int count = 0;
                time = DateTime.Now;//开始计时
                //获取密钥
                string conn = Start.DBConn + Start.access_key;
                AconnStr = new OleDbConnection(conn);  //设定连接数据库
                AconnStr.Open(); //打开连接
                OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                command.Connection = AconnStr;
                OleDbDataReader odr = null; //设定一个数据流对象
                command.CommandText = string.Format("select * from UserList where UserRole=@UserRole");
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@UserRole", SM4.EnString("Admin")));
                command.ExecuteNonQuery();
                odr = command.ExecuteReader();//执行命令获取数据
                odr.Read();
                string Key = "";
                if (odr.HasRows)//存在记录
                {
                    Key = SM4.DeString(odr["UserPwd"].ToString());
                }
                odr.Close();
                command.CommandText = string.Format("select * from KeyList");
                command.ExecuteNonQuery();
                odr = command.ExecuteReader();//执行命令获取数据
                odr.Read();
                string secA = "";
                string secB = "";
                string secC = "";
                if (odr.HasRows)//存在记录
                {
                    secA = SM4.DeString(odr["AKey"].ToString(), Key);
                    secB = SM4.DeString(odr["BKey"].ToString(), Key);
                    secC = SM4.DeString(odr["CKey"].ToString(), Key);
                }
                else
                {
                    odr.Close(); //关闭数据流
                    AconnStr.Close();
                    MessageBox.Show("加密失败！");
                    return;
                }
                odr.Close(); //关闭数据流

                foreach (string ss in fileList)
                {
                    listView1.Items[count].SubItems[2].Text = "正在加密";
                    if (radioButton1.Checked == true)//绝密级加密
                    {
                        SM4.File_Encrypt(ss, secA, ss + "_A");
                        try
                        {
                            File.Delete(ss);
                        }
                        catch (Exception) { }                       
                    }
                    if (radioButton2.Checked == true)//机密级加密
                    {
                        SM4.File_Encrypt(ss, secB, ss + "_B");
                        try
                        {
                            File.Delete(ss);
                        }
                        catch (Exception) { }                     
                    }
                    if (radioButton3.Checked == true)//秘密级加密
                    {
                        SM4.File_Encrypt(ss, secC, ss + "_C");
                        try
                        {
                            File.Delete(ss);
                        }
                        catch (Exception) { };                       
                    }
                    command.ExecuteNonQuery();
                    listView1.Items[count].SubItems[2].Text = "加密完成";// "s";   
                    count++;
                    label6.Text = "已加密" + count.ToString() + "个文件";
                    label6.Refresh(); //如果控件的状态实时更新  则实时重绘
                }
                AconnStr.Close();
                ok();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ok()
        {
            if (num2 < 1024)
                label9.Text = "平均速度：" + ((float)num2 / ftime).ToString() + " B/s";
            if ((num2 / 1024) > 1 && (num / 1024) < 1024)
                label9.Text = "平均速度：" + ((float)num2 / 1024 / ftime).ToString() + " KB/s";
            if ((num2 / 1024) > 1024)
                label9.Text = "平均速度：" + ((float)num2 / 1024 / 1024 / ftime).ToString() + " MB/s";
            num = 0;
            fileList.Clear();
            enIng = false;
            SM4.EnSum = 0;
            num = 0;
            for (int i = 0; i < listView1.Items.Count; i++)
                listView1.Items[i].SubItems[2].Text = "加密完成";
            progressBar1.Value = 100;
            label1.Text = "100%";
            label6.Text = "加密完成";
            RoleControl();
            MessageBox.Show("加密完成！");
            if (fileList.Count == 0)
                listView1.Items.Clear();
            progressBar1.Visible = false;
            label1.Text = "";
            label6.Text = "";
            label3.Text = "";
            label9.Text = "";
            label5.Text = "";
        }

        private void JiaMi_Mian_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (enIng == true)
            {
                e.Cancel = true;//不关闭窗口
                MessageBox.Show("正在加密！请等加密完成再关闭！");
                return;
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                this.listView1.Cursor = System.Windows.Forms.Cursors.Arrow;  //指定鼠标形状（更好看）  
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }  
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {      
            try
            {
                String[] paths = (String[])e.Data.GetData(DataFormats.FileDrop);
                if (enIng == true)
                {
                    MessageBox.Show("正在加密！请稍等！");
                    return;
                }
                if ((Start.AFileAuthority == "2" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "2" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "2" || Start.CFileAuthority == "4"))
                {
                    MessageBox.Show("您不具备任何加密权限");
                    return;
                }
                for (int i = 0; i < paths.Length; i++)
                {
                    if (File.Exists(paths[i]))//是文件
                    {               
                        if (fileList.Count == 0)
                            listView1.Items.Clear();
                        progressBar1.Visible = false;
                        label1.Text = "";
                        label6.Text = "";
                        label3.Text = "";
                        label9.Text = "";

                        if (paths[i] == Start.CheckFile || paths[i] == Start.DBFile || paths[i] == Start.ReadMeFile || paths[i] == Process.GetCurrentProcess().MainModule.FileName ||
                           paths[i] == System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "SM3DLL.dll" || paths[i] == System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "SM4DLL.dll")
                            continue;
                        if (paths[i].Substring(paths[i].Length - 2, 2) == "_A" || paths[i].Substring(paths[i].Length - 2, 2) == "_B" || paths[i].Substring(paths[i].Length - 2, 2) == "_C")
                            continue;
                        if (fileList.Count == 0)
                        {
                            fileList.Add(paths[i]);
                            fileInfo = new FileInfo(paths[i]);
                            num += fileInfo.Length;
                            item = new ListViewItem(fileList.Count.ToString());
                            item.SubItems.Add(fileInfo.Name);
                            item.SubItems.Add("未加密");
                            listView1.Items.Add(item);
                        }
                        else
                        {
                            //防止重选
                            bool repeat = false;
                            foreach (string ss in fileList)
                            {
                                if (ss == paths[i])
                                {
                                    repeat = true;
                                    break;
                                }
                            }
                            if (repeat == true) continue;
                            fileList.Add(paths[i]);
                            fileInfo = new FileInfo(paths[i]);
                            item = new ListViewItem(fileList.Count.ToString());
                            item.SubItems.Add(fileInfo.Name);
                            item.SubItems.Add("未加密");
                            listView1.Items.Add(item);
                            num += fileInfo.Length;
                        }
                        label5.Text = "共" + fileList.Count.ToString() + "个文件";
                        if (num < 1024)
                            label5.Text += "  数据量：" + num.ToString() + " B";
                        if ((num / 1024) >= 1 && (num / 1024) < 1024)
                            label5.Text += "  数据量：" + ((float)num / 1024).ToString() + " KB";
                        if ((num / 1024 / 1024) >= 1 && (num / 1024 / 1024) < 1024)
                            label5.Text += "  数据量：" + ((float)num / 1024 / 1024).ToString() + " MB";
                        if ((num / 1024 / 1024) > 1024)
                            label5.Text += "  数据量：" + ((float)num / 1024 / 1024 / 1024).ToString() + " GB";
                    }
                    if (Directory.Exists(paths[i])) // 是文件夹
                    {
                        if (Start.FolderAuthority == "2" || Start.FolderAuthority == "4")
                            continue;
                        if (fileList.Count == 0)
                            listView1.Items.Clear();
                        progressBar1.Visible = false;
                        label1.Text = "";
                        label6.Text = "";
                        label3.Text = "";
                        label9.Text = "";

                        FindFile(paths[i]);
                        label5.Text = "共" + fileList.Count.ToString() + "个文件";
                        if (num < 1024)
                            label5.Text += "  数据量：" + num.ToString() + " B";
                        if ((num / 1024) >= 1 && (num / 1024) < 1024)
                            label5.Text += "  数据量：" + ((float)num / 1024).ToString() + " KB";
                        if ((num / 1024 / 1024) >= 1 && (num / 1024 / 1024) < 1024)
                            label5.Text += "  数据量：" + ((float)num / 1024 / 1024).ToString() + " MB";
                        if ((num / 1024 / 1024) > 1024)
                            label5.Text += "  数据量：" + ((float)num / 1024 / 1024 / 1024).ToString() + " GB";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.listView1.Cursor = System.Windows.Forms.Cursors.IBeam; //还原鼠标形状 
        }
    }
}
