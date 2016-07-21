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
using BLL;

namespace 小U
{
    public partial class Decryption : Form
    {
        List<string> fileList = new List<string>(); //操作文件列表
        static public bool deIng = false;//判断解密是否进行中
        private long num = 0; //所处理文件字节数
        private long num2 = 0; //所处理文件字节数(为了解密时计算速度)
        private FileInfo fileInfo;
        DateTime time = new DateTime();//计时
        ListViewItem item;//ListView控件项
        Thread th, th2;
        float ftime;

        public Decryption()
        {
            InitializeComponent();
        }

        private void JieMi_Mian_Load(object sender, EventArgs e)
        {
            label5.Text = "";
            label6.Text = "";
            label1.Text = "";
            label3.Text = "";
            label9.Text = "";
            progressBar1.Visible = false;
            if (Start.roleName != "Admin") button2.Visible = false;

            if (Start.FolderAuthority == "3" || Start.FolderAuthority == "4")
            {
                button1.Enabled = false;
                button1.Visible = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                string filter = "小U加密文件|";
                if (Start.AFileAuthority == "1" || Start.AFileAuthority == "2")
                    filter += "*.*_A;";
                if (Start.BFileAuthority == "1" || Start.BFileAuthority == "2")
                    filter += "*.*_B;";
                if (Start.CFileAuthority == "1" || Start.CFileAuthority == "2")
                    filter += "*.*_C";
                if ((Start.AFileAuthority == "3" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "3" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "3" || Start.CFileAuthority == "4"))
                {
                    MessageBox.Show("您不具备任何解密权限");
                    return;
                }
                if (deIng == true)
                {
                    MessageBox.Show("正在解密！请稍等！");
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
                open.Filter = filter;
                open.Multiselect = false;//是否可以多选
                if (open.ShowDialog() == DialogResult.Cancel)
                    return;
                if (fileList.Count == 0)
                {
                    fileList.Add(open.FileName);
                    fileInfo = new FileInfo(open.FileName);
                    num += fileInfo.Length;
                    item = new ListViewItem(fileList.Count.ToString());
                    item.SubItems.Add(fileInfo.Name);
                    item.SubItems.Add("未解密");
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
                    item.SubItems.Add("未解密");
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
                if((num / 1024 / 1024) > 1024)
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
                if (Start.FolderAuthority == "3" || Start.FolderAuthority == "4")
                {
                    MessageBox.Show("您无权进行此操作，请勿越权操作！");
                    return;
                }
                if ((Start.AFileAuthority == "3" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "3" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "3" || Start.CFileAuthority == "4"))
                {
                    MessageBox.Show("您不具备任何解密权限");
                    return;
                }
                if (deIng == true)
                {
                    MessageBox.Show("正在解密！请稍等！");
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
                    if (Start.AFileAuthority == "3" || Start.AFileAuthority == "4")
                        if (s.Substring(s.Length - 2, 2) == "_A") error = true;
                    if (Start.BFileAuthority == "3" || Start.BFileAuthority == "4")
                        if (s.Substring(s.Length - 2, 2) == "_B") error = true;
                    if (Start.CFileAuthority == "3" || Start.CFileAuthority == "4")
                        if (s.Substring(s.Length - 2, 2) == "_C") error = true;
                    if (s.Substring(s.Length - 2, 2) != "_A" && s.Substring(s.Length - 2, 2) != "_B" && s.Substring(s.Length - 2, 2) != "_C")
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
                    item.SubItems.Add("未解密");
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

        private void button6_Click(object sender, EventArgs e)
        {
            if (deIng == true)
            {
                MessageBox.Show("正在解密！请稍等！");
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

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (Encryption.enIng == true)
                {
                    MessageBox.Show("正在进行加密！请等加密完成后再操作！");
                    return;
                }
                if (deIng == true)
                {
                    MessageBox.Show("正在解密！请稍等！");
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
                if (fileList.Count != 0)
                {
                    //开始解密
                    deIng = true;
                    num2 = num;
                    label6.Text = "正在解密第一个文件";
                    label1.Text = "";
                    progressBar1.Visible = true;
                    th = new Thread(this.Decrypt);
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
                        if (num != 0) progressBar1.Value = System.Math.Abs((int)(SM4.DeSum * 100 / num));
                        if (num != 0) label1.Text = System.Math.Abs((SM4.DeSum * 100 / (num + 1))).ToString() + "%";
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

        private void Decrypt()
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
                    MessageBox.Show("解密失败！");
                    return;
                }
                odr.Close(); //关闭数据流

                foreach (string ss in fileList)
                {
                    listView1.Items[count].SubItems[2].Text = "正在解密";
                    if (ss.Substring(ss.Length - 2, 2) == "_A")
                    {
                        SM4.File_Decrypt(ss, secA, ss.Substring(0, ss.Length - 2));
                        try
                        {
                            File.Delete(ss);
                        }
                        catch (Exception) { } 
                    }
                    if (ss.Substring(ss.Length - 2, 2) == "_B")
                    {
                        SM4.File_Decrypt(ss, secB, ss.Substring(0, ss.Length - 2));
                        try
                        {
                            File.Delete(ss);
                        }
                        catch (Exception) { } 
                    }
                    if (ss.Substring(ss.Length - 2, 2) == "_C")
                    {
                        SM4.File_Decrypt(ss, secC, ss.Substring(0, ss.Length - 2));
                        try
                        {
                            File.Delete(ss);
                        }
                        catch (Exception) { } 
                    }
                    listView1.Items[count].SubItems[2].Text = "解密完成";// "s";   
                    count++;
                    label6.Text = "已解密" + count.ToString() + "个文件";
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
            fileList.Clear();
            deIng = false;
            num2 = 0;
            SM4.DeSum = 0;
            num = 0;
            for (int i = 0; i < listView1.Items.Count; i++)
                listView1.Items[i].SubItems[2].Text = "解密完成";
            progressBar1.Value = 100;
            label1.Text = "100%";
            label6.Text = "解密完成";
            MessageBox.Show("解密完成！");
            if (fileList.Count == 0)
                listView1.Items.Clear();
            progressBar1.Visible = false;
            label1.Text = "";
            label6.Text = "";
            label3.Text = "";
            label9.Text = "";
            label5.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Start.roleName != "Admin")
            {
                MessageBox.Show("您无权进行此操作，请勿越权操作！");
                return;
            }
            if (Encryption.enIng == true)
            {
                MessageBox.Show("正在进行加密！请等加密完成后再操作！");
                return;
            }
            if (deIng == true)
            {
                MessageBox.Show("正在解密！请稍等！");
                return;
            }
            label1.Text = "";
            label6.Text = "";
            label5.Text = "";
            label3.Text = "";
            label9.Text = "";
            fileList.Clear();
            listView1.Items.Clear();
            num = 0;
            progressBar1.Visible = false;
            try
            {
                FindFile(Start.panFu);
                if (fileList.Count == 0)
                {
                    MessageBox.Show("无已加密文件！");
                    return;
                }
                if (fileList.Count != 0)
                {
                    label5.Text = "共" + fileList.Count.ToString() + "个文件";
                    if (num < 1024)
                        label5.Text += "  数据量：" + num.ToString() + " B";
                    if ((num / 1024) >= 1 && (num / 1024) < 1024)
                        label5.Text += "  数据量：" + ((float)num / 1024).ToString() + " KB";
                    if ((num / 1024 / 1024) >= 1 && (num / 1024 / 1024) < 1024)
                        label5.Text += "  数据量：" + ((float)num / 1024 / 1024).ToString() + " MB";
                    if ((num / 1024 / 1024) > 1024)
                        label5.Text += "  数据量：" + ((float)num / 1024 / 1024 / 1024).ToString() + " GB";
                    //开始解密
                    DialogResult bu = MessageBox.Show("一键解密会解密所有已加密的文件。\r\n\r\n是否进行一键解密？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (bu.ToString() == "Cancel")
                    {
                        fileList.Clear();
                        listView1.Items.Clear();
                        label5.Text = "";
                        num = 0;
                        return;
                    }
                    deIng = true;
                    num2 = num;
                    label6.Text = "正在解密第一个文件";
                    label1.Text = "";
                    progressBar1.Visible = true;
                    th = new Thread(this.Decrypt);
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

        private void JieMi_Mian_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (deIng == true)
            {
                e.Cancel = true;//不关闭窗口
                MessageBox.Show("正在解密！请等解密完成再关闭！");
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
                if ((Start.AFileAuthority == "3" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "3" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "3" || Start.CFileAuthority == "4"))
                {
                    MessageBox.Show("您不具备任何解密权限");
                    return;
                }
                if (deIng == true)
                {
                    MessageBox.Show("正在解密！请稍等！");
                    return;
                }
                for (int i = 0; i < paths.Length; i++)
                {
                    if (File.Exists(paths[i]))//是文件
                    {                                   
                        if (paths[i].Substring(paths[i].Length - 2, 2) != "_A" && paths[i].Substring(paths[i].Length - 2, 2) != "_B" && paths[i].Substring(paths[i].Length - 2, 2) != "_C")
                            continue;
                        if (fileList.Count == 0)
                            listView1.Items.Clear();
                        progressBar1.Visible = false;
                        label1.Text = "";
                        label6.Text = "";
                        label3.Text = "";
                        label9.Text = "";

                        if (fileList.Count == 0)
                        {
                            fileList.Add(paths[i]);
                            fileInfo = new FileInfo(paths[i]);
                            num += fileInfo.Length;
                            item = new ListViewItem(fileList.Count.ToString());
                            item.SubItems.Add(fileInfo.Name);
                            item.SubItems.Add("未解密");
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
                            item.SubItems.Add("未解密");
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
                        if (Start.FolderAuthority == "3" || Start.FolderAuthority == "4")
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

