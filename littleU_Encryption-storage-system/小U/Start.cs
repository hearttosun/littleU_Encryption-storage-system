using ADOX;
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
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace 小U
{
    public partial class Start : Form
    {
        public const int WM_DEVICECHANGE = 0x219;//U盘插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        static public string panFu = "";//盘符
        static public string path = "";//配置文件路径
        static public string DBFile = "";//数据库文件路径
        static public string ReadMeFile = "";//ReadMe文件路径
        static public string DBFileSuo = "";//锁定数据库文件路径
        static public string CheckFile = "";//数据库校验文件路径
        static public string DBConn = "";//数据库连接
        static public bool First = false;//是否是第一次加密
        static public string roleName = "";//用户角色
        static public string AFileAuthority = "";//绝密文件访问权限
        static public string BFileAuthority = "";//机密文件访问权限
        static public string CFileAuthority = "";//秘密文件访问权限
        static public string FolderAuthority = "";//文件夹操作权限
        static public string UserName = "";//登陆成功的用户名
        static public bool xiugai = true;//默认选择修改用户等级
        static public string access_key = "*Da8ba2B0DA546K3I8**";
        private QrCodeCheck q;

        public Start()
        {
            InitializeComponent();
            q = new QrCodeCheck(this);
        }

        //U盘消息
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == WM_DEVICECHANGE)
                    detection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.WndProc(ref m);
        }

        //U盘检测
        private void detection()
        {
            int count = 0;
            DriveInfo[] uin = DriveInfo.GetDrives();
            comboBox1.Text = "";
            comboBox1.Items.Clear();
            foreach (DriveInfo drive in uin)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    count++;
                    panFu = drive.Name;
                    comboBox1.Items.Add(panFu);
                }
            }
            if (count == 0)
            {
                label4.Text = "U盘未插入！";
                label2.Text = "";
                linkLabel5.Visible = false;
                label4.ForeColor = Color.Red;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
            }
            if (count == 1)
            {
                comboBox1.Text = panFu;
                label4.Text = "U盘已插入，盘符为:" + panFu;
                label4.ForeColor = Color.Blue;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
            }
            if (count > 1)
            {
                comboBox1.Enabled = true;
                label4.Text = "U盘数大于1，请选择要操作的U盘";
                linkLabel5.Visible = false;
            }
            ifFirst();
        }

        private void Start_Load(object sender, EventArgs e)
        {
            try
            {
                comboBox1.Enabled = false;
                label4.Text = "";
                label2.Text = "";
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                if (!File.Exists("SM3DLL.dll"))
                {
                    MessageBox.Show("未找到SM3DLL.dll模块,应用程序关闭！", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                if (!File.Exists("SM4DLL.dll"))
                {
                    MessageBox.Show("未找到SM4DLL.dll模块,应用程序关闭！", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                detection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //为了在初次加密完成后自动完成密码输入
        public void setin(string u, string m)
        {
            textBox1.Text = u;
            textBox2.Text = m;
        }

        //为了修改密码后自动完成密码输入
        public void xiuGai(string m)
        {
            textBox2.Text = m;
        }

        public bool ifFirst()
        {
            if (comboBox1.Text != "")
            {
                //给各个静态变量赋值
                path = panFu + @"小U安全\Config";
                DBFile = path + @"\DB.mdb";//用户等级数据库文件路径
                DBFileSuo = path + @"\DB.ldb";//锁定数据库文件路径
                CheckFile = path + @"\Check.xus";//数据库校验文件
                ReadMeFile = path + @"\ReadMe.txt";//ReadMe校验文件
                try
                {
                    if (File.Exists(Start.DBFileSuo))
                        File.Delete(Start.DBFileSuo);
                }
                catch { }

                DBConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + DBFile + ";Jet OLEDB:Database Password=";
                if (File.Exists(DBFile) && File.Exists(CheckFile))
                {
                    textBox2.Enabled = true;
                    textBox1.Enabled = true;
                    label2.Text = "(已加密)";
                    First = false;
                    linkLabel5.Visible = false;
                    return false;//不是第一次操作
                }
                else
                {
                    textBox2.Enabled = false;
                    textBox1.Enabled = false;
                    First = true;
                    label2.Text = "(未加密)";
                    linkLabel5.Visible = true;
                }
            }
            return true;//是第一次操作
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label4.Text = "选择U盘：" + comboBox1.Text;
            panFu = comboBox1.Text;
            ifFirst();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (comboBox1.Text == "")
                {
                    MessageBox.Show("U盘未确定！");
                    return;
                }
                if (ifFirst())
                {
                    MessageBox.Show("U盘未加密！");
                    return;
                }
                if (textBox1.Text == "")
                {
                    MessageBox.Show("请输入用户名！");
                    textBox1.Focus();
                    return;
                }
                if (!SM3.CheckDB(Start.panFu + @"\小U安全\\Config\\Check.xus", Start.panFu + @"\小U安全\\Config\\DB.mdb"))
                {
                    linkLabel5.Visible = true;
                    if (MessageBox.Show("配置文件被破坏！是否恢复配置文件？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                    {
                        Backup b = new Backup(this);
                        b.Show();
                    }
                    else this.Close();
                }
                else
                {
                    string conn = Start.DBConn + Start.access_key;
                    OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                    AconnStr.Open(); //打开连接
                    OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                    command.Connection = AconnStr;
                    command.CommandText = string.Format("select * from UserList where UserName=@UserName");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(textBox1.Text)));
                    command.ExecuteNonQuery();
                    OleDbDataReader odr = null; //设定一个数据流对象
                    odr = command.ExecuteReader();//执行命令获取数据
                    odr.Read();
                    if (!odr.HasRows)//存在记录
                    {
                        MessageBox.Show("用户名不存在！");
                        return;
                    }
                    UserName = textBox1.Text;
                    Find f = new Find();
                    f.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (comboBox1.Text == "")
                {
                    MessageBox.Show("U盘未确定！");
                    return;
                }
                if (ifFirst())
                {
                    MessageBox.Show("U盘未加密！");
                    return;
                }
                if (textBox1.Text == "")
                {
                    MessageBox.Show("请输入用户名");
                    textBox1.Focus();
                    return;
                }
                if (!SM3.CheckDB(Start.panFu + @"\小U安全\\Config\\Check.xus", Start.panFu + @"\小U安全\\Config\\DB.mdb"))
                {
                    linkLabel5.Visible = true;
                    if (MessageBox.Show("配置文件被破坏！是否恢复配置文件？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                    {
                        Backup b = new Backup(this);
                        b.Show();
                    }
                    else this.Close();
                }
                else
                {
                    string conn = Start.DBConn + Start.access_key;
                    OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                    AconnStr.Open(); //打开连接
                    OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                    command.Connection = AconnStr;
                    command.CommandText = string.Format("select * from UserList where UserName=@UserName");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(textBox1.Text)));
                    command.ExecuteNonQuery();
                    OleDbDataReader odr = null; //设定一个数据流对象
                    odr = command.ExecuteReader();//执行命令获取数据
                    odr.Read();
                    if (!odr.HasRows)//存在记录
                    {
                        AconnStr.Close(); //打开连接
                        MessageBox.Show("用户名不存在！");
                        return;
                    }
                    AconnStr.Close(); //打开连接
                    UserName = textBox1.Text;
                    PwdChange x = new PwdChange(this);
                    x.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (comboBox1.Text == "")
                {
                    MessageBox.Show("U盘未确定！");
                    return;
                }
                if (ifFirst())
                {
                    MessageBox.Show("U盘未加密！");
                    return;
                }
                if (textBox1.Text == "")
                {
                    MessageBox.Show("请输入用户名");
                    textBox1.Focus();
                    return;
                }
                if (!SM3.CheckDB(Start.panFu + @"\小U安全\\Config\\Check.xus", Start.panFu + @"\小U安全\\Config\\DB.mdb"))
                {
                    linkLabel5.Visible = true;
                    if (MessageBox.Show("配置文件被破坏！是否恢复配置文件？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                    {
                        Backup b = new Backup(this);
                        b.Show();
                    }
                    else this.Close();
                }
                else
                {
                    string conn = Start.DBConn + Start.access_key;
                    OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                    AconnStr.Open(); //打开连接
                    OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                    command.Connection = AconnStr;
                    command.CommandText = string.Format("select * from UserList where UserName=@UserName");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(textBox1.Text)));
                    command.ExecuteNonQuery();
                    OleDbDataReader odr = null; //设定一个数据流对象
                    odr = command.ExecuteReader();//执行命令获取数据
                    odr.Read();
                    if (!odr.HasRows)//不存在记录
                    {
                        MessageBox.Show("用户名不存在！");
                        return;
                    }
                    UserName = textBox1.Text;
                    NineLock n = new NineLock(this);
                    n.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Start_FormClosed(object sender, FormClosedEventArgs e)
        {
            //强制关闭
            System.Diagnostics.Process tt = System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id);
            tt.Kill();
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("U盘未确定！");
                return;
            }
            if (ifFirst())//第一次加密
            {

                if (MessageBox.Show("是否进行管理员注册？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    Register j = new Register(this);
                    j.Show();
                }
                else return;
            }
            else
            {
                if (!SM3.CheckDB(Start.panFu + @"\小U安全\\Config\\Check.xus", Start.panFu + @"\小U安全\\Config\\DB.mdb"))
                {
                    linkLabel5.Visible = true;
                    if (MessageBox.Show("配置文件被破坏！是否恢复配置文件？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                    {
                        Backup b = new Backup(this);
                        b.Show();
                    }
                    else this.Close();
                }
                else
                {
                    string conn = Start.DBConn + Start.access_key;
                    OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                    AconnStr.Open(); //打开连接
                    OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                    command.Connection = AconnStr;
                    command.CommandText = string.Format("select * from RoleAccessControl where RoleName<>@RoleName");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString("Admin")));
                    command.ExecuteNonQuery();
                    OleDbDataReader odr = null; //设定一个数据流对象
                    odr = command.ExecuteReader();//执行命令获取数据
                    odr.Read();
                    if (odr.HasRows)//存在记录
                    {
                        Register j = new Register(this);
                        j.Show();
                    }
                    else
                    {
                        MessageBox.Show("管理员未设置用户角色，无法注册，请联系管理员");
                    }
                    odr.Close(); //关闭数据流
                    AconnStr.Close();
                }
            }
        }

        public void openMain()
        {
            Main U = new Main();
            U.Show();
        }

        public void button1_Click(object sender, EventArgs e)
        {

            try
            {
                if (comboBox1.Text == "")
                {
                    MessageBox.Show("U盘未确定！");
                    return;
                }
                if (ifFirst())//第一次加密
                {

                    if (MessageBox.Show("是否进行管理员注册？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        Register j = new Register(this);
                        j.Show();
                    }
                    else return;
                }
                else
                {
                    if (!SM3.CheckDB(Start.panFu + @"\小U安全\\Config\\Check.xus", Start.panFu + @"\小U安全\\Config\\DB.mdb"))
                    {
                        linkLabel5.Visible = true;
                        if (MessageBox.Show("配置文件被破坏！是否恢复配置文件？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                        {
                            Backup b = new Backup(this);
                            b.Show();
                        }
                        else this.Close();
                    }
                    else
                    {
                        UserName = "";
                        string conn = Start.DBConn + Start.access_key;
                        OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                        AconnStr.Open(); //打开连接
                        OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                        command.Connection = AconnStr;
                        command.CommandText = string.Format("select * from UserList where UserName=@Name and UserPwd=@Pwd");  //防注入
                        command.Parameters.Clear();
                        command.Parameters.Add(new OleDbParameter("@Name", SM4.EnString(textBox1.Text)));
                        command.Parameters.Add(new OleDbParameter("@Pwd", SM4.EnString(textBox2.Text)));
                        command.ExecuteNonQuery();
                        OleDbDataReader odr = null; //设定一个数据流对象
                        odr = command.ExecuteReader();//执行命令获取数据
                        odr.Read();
                        if (odr.HasRows)//存在记录
                        {
                            Start.UserName = textBox1.Text;
                            string con = Start.DBConn + Start.access_key;
                            OleDbConnection AconnStr1 = new OleDbConnection(con);  //设定连接数据库
                            AconnStr1.Open(); //打开连接s
                            string ss1 = "UPDATE UserList SET LastLoginTime =@LastLoginTime WHERE UserName=@UserName";

                            OleDbCommand Icmd1 = new OleDbCommand(ss1, AconnStr);
                            Icmd1.Parameters.Clear();
                            Icmd1.Parameters.Add(new OleDbParameter("@LastLoginTime", System.DateTime.Now.ToString()));
                            Icmd1.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(Start.UserName)));
                            Icmd1.ExecuteNonQuery();
                            AconnStr1.Close();
                            SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");
                            roleName = SM4.DeString(odr["UserRole"].ToString()).Replace("\0", "");
                            string state = SM4.DeString(odr["UserState"].ToString()).Replace("\0", "");
                            odr.Close(); //关闭数据流

                            command.CommandText = string.Format("select * from RoleAccessControl where RoleName=@RoleName");
                            command.Parameters.Clear();
                            command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(roleName)));
                            command.ExecuteNonQuery();
                            odr = command.ExecuteReader();//执行命令获取数据
                            odr.Read();
                            if (odr.HasRows)//存在记录
                            {
                                AFileAuthority = SM4.DeString(odr["AFile"].ToString()).Replace("\0", "");
                                BFileAuthority = SM4.DeString(odr["BFile"].ToString()).Replace("\0", "");
                                CFileAuthority = SM4.DeString(odr["CFile"].ToString()).Replace("\0", "");
                                FolderAuthority = SM4.DeString(odr["FolderOperation"].ToString()).Replace("\0", "");
                            }
                            else
                            {
                                odr.Close();
                                AconnStr.Close();
                                MessageBox.Show("程序出错");
                                Application.Exit();
                            }
                            odr.Close();

                            UserName = textBox1.Text;
                            if (roleName == "Admin")
                            {
                                command.CommandText = string.Format("select * from UserList where UserState=@UserState");
                                command.Parameters.Clear();
                                command.Parameters.Add(new OleDbParameter("@UserState", SM4.EnString("1")));
                                command.ExecuteNonQuery();
                                odr = command.ExecuteReader();//执行命令获取数据
                                odr.Read();
                                if (odr.HasRows && xiugai == true)//存在记录
                                {
                                    if (MessageBox.Show("有新注册用户，是否审核？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                                    {
                                        UserManage2 u = new UserManage2();
                                        u.Show();
                                        this.Visible = false;
                                    }
                                    else xiugai = false;
                                }
                            }
                            odr.Close();
                            AconnStr.Close();
                            if (state == "1")
                            {
                                MessageBox.Show("管理员尚未审核注册信息！请联系管理员！");
                                this.Close();
                            }
                            UserName = textBox1.Text;
                            Main U = new Main();
                            U.Show();
                            this.Visible = false;
                        }
                        else
                        {
                            MessageBox.Show("用户名或密码错误!");
                            odr.Close(); //关闭数据流
                            AconnStr.Close();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult bu = MessageBox.Show("确认退出？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (bu.ToString() == "OK")
                Application.Exit();
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Backup b = new Backup(this);
            b.Show();
        }     

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("U盘未确定！");
                return;
            }
            if (ifFirst())
            {
                MessageBox.Show("U盘未加密！");
                return;
            }
            if (!SM3.CheckDB(Start.panFu + @"\小U安全\\Config\\Check.xus", Start.panFu + @"\小U安全\\Config\\DB.mdb"))
            {
                linkLabel5.Visible = true;
                if (MessageBox.Show("配置文件被破坏！是否恢复配置文件？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                {
                    Backup b = new Backup(this);
                    b.Show();
                }
                else this.Close();
            }
            else
            {
                if (QrCodeCheck.check == false)
                {
                    q.Show();
                    q.Visible = true;
                }
                else
                    MessageBox.Show("已验证通过！");
            }
        }
    }
}
