using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Data.OleDb;//连接access数据库
using ADOX;
using System.Text.RegularExpressions;// 引用COM：Microsoft ADO Ext. 2.8 for DDL and Security 
//添加引用：Microsoft ActioveX Data Objects 2.8 Library
using BLL;
using System.Data.SqlClient;

namespace 小U
{
    public partial class Register : Form
    {
        private Start start;
        private List<string> roles = new List<string>();

        public Register(Start s)
        {
            InitializeComponent();
            start = s;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "" || textBox4.Text == "" || textBox5.Text == "")
            {
                MessageBox.Show("请输入完整！");
                return;
            }
            if (textBox5.Text.Contains(","))
            {
                MessageBox.Show("用户名中不能包含 , 字符");
                return;
            }
            if (textBox1.Text != textBox4.Text)
            {
                MessageBox.Show("两次密码输入不一致！");
                return;
            }
            if (jieSuo == "")
            {
                MessageBox.Show("未设置解锁手势！");
                return;
            }
            if (textBox1.Text.Length < 8)
            {
                MessageBox.Show("密码长度不够！");
                return;
            }

            string pattern1 = @"[A-Za-z]";
            string pattern2 = @"[0-9]";
            Regex regex = new Regex(pattern1);
            Regex regex2 = new Regex(pattern2);
            if (!regex.IsMatch(textBox1.Text) || !regex2.IsMatch(textBox1.Text))
            {
                MessageBox.Show("密码必须为字母和数字的组合！");
                return;
            }

            if (Start.First == true)//首次加密为管理用户
            {
                try
                {
                    if (!Directory.Exists(Start.path))//第一次加密
                    {
                        //创建目录
                        Directory.CreateDirectory(Start.path);
                        //隐藏文件夹
                        DirectoryInfo dir = new DirectoryInfo(Start.panFu + @"\小U安全");
                        dir.Attributes = FileAttributes.Hidden;
                    }
                    //防止多次写入
                    if (File.Exists(Start.DBFile))
                        File.Delete(Start.DBFile);
                    if (File.Exists(Start.CheckFile))
                        File.Delete(Start.CheckFile);
                    //新建用户等级access数据库  带密码                 
                    string conn = Start.DBConn + Start.access_key + ";Jet OLEDB:Engine Type=5";
                    ADOX.Catalog catalog = new Catalog();
                    try
                    {
                        catalog.Create(conn);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        return;
                    }
                    //连接数据库
                    ADODB.Connection cn = new ADODB.Connection();
                    cn.Open(conn, null, null, -1);
                    catalog.ActiveConnection = cn;
                    //新建表
                    ADOX.Table table = new ADOX.Table();
                    table.Name = "UserList";//用户数据表
                    ADOX.Column column = new ADOX.Column();
                    column.ParentCatalog = catalog;
                    column.Type = ADOX.DataTypeEnum.adInteger; // 必须先设置字段类型
                    column.Name = "ID";
                    column.DefinedSize = 9;
                    column.Properties["AutoIncrement"].Value = true;
                    table.Columns.Append(column, DataTypeEnum.adInteger, 0);
                    //设置主键
                    table.Keys.Append("PrimaryKey", ADOX.KeyTypeEnum.adKeyPrimary, "ID", "", "");
                    //设置除主键外其他字段
                    table.Columns.Append("UserName", DataTypeEnum.adLongVarWChar, 20);
                    table.Columns.Append("UserPwd", DataTypeEnum.adLongVarWChar, 20);
                    table.Columns.Append("Question", DataTypeEnum.adLongVarWChar, 50);
                    table.Columns.Append("Answer", DataTypeEnum.adLongVarWChar, 50);
                    table.Columns.Append("NineLock", DataTypeEnum.adLongVarWChar, 9);
                    table.Columns.Append("UserRole", DataTypeEnum.adLongVarWChar, 20);//角色
                    table.Columns.Append("IdentityInformation",DataTypeEnum.adLongVarWChar, 20);;//身份信息
                    table.Columns.Append("LastLoginTime", DataTypeEnum.adLongVarWChar, 20);
                    table.Columns.Append("UserState", DataTypeEnum.adLongVarWChar, 1);//用户状态 0表示审核通过，1表示未审核
                    try
                    {
                        catalog.Tables.Append(table);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        return;
                    }

                    table = new ADOX.Table();
                    table.Name = "KeyList";//密钥数据表
                    column = new ADOX.Column();
                    column.ParentCatalog = catalog;
                    column.Type = ADOX.DataTypeEnum.adInteger; // 必须先设置字段类型
                    column.Name = "ID";
                    column.DefinedSize = 9;
                    column.Properties["AutoIncrement"].Value = true;
                    table.Columns.Append(column, DataTypeEnum.adInteger, 0);
                    //设置主键
                    table.Keys.Append("PrimaryKey", ADOX.KeyTypeEnum.adKeyPrimary, "ID", "", "");
                    //设置除主键外其他字段
                    table.Columns.Append("AKey", DataTypeEnum.adLongVarWChar, 50);
                    table.Columns.Append("BKey", DataTypeEnum.adLongVarWChar, 50);
                    table.Columns.Append("CKey", DataTypeEnum.adLongVarWChar, 50);
                    try
                    {
                        catalog.Tables.Append(table);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        return;
                    }                 

                    table = new ADOX.Table();
                    table.Name = "RoleAccessControl";//RBAC数据表
                    column = new ADOX.Column();
                    column.ParentCatalog = catalog;
                    column.Type = ADOX.DataTypeEnum.adInteger; // 必须先设置字段类型
                    column.Name = "ID";
                    column.DefinedSize = 9;
                    column.Properties["AutoIncrement"].Value = true;
                    table.Columns.Append(column, DataTypeEnum.adInteger, 0);
                    //设置主键
                    table.Keys.Append("PrimaryKey", ADOX.KeyTypeEnum.adKeyPrimary, "ID", "", "");
                    //设置除主键外其他字段
                    table.Columns.Append("RoleName", DataTypeEnum.adLongVarWChar, 20);
                    table.Columns.Append("AFile", DataTypeEnum.adLongVarWChar, 1);
                    table.Columns.Append("BFile", DataTypeEnum.adLongVarWChar, 1);
                    table.Columns.Append("CFile", DataTypeEnum.adLongVarWChar, 1);
                    table.Columns.Append("FolderOperation", DataTypeEnum.adLongVarWChar, 1);
                    try
                    {
                        catalog.Tables.Append(table);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        return;
                    }
                    //此处一定要关闭连接，否则添加数据时候会出错
                    table = null;
                    catalog = null;
                    cn.Close();
                    cn = null;

                    //插入管理员信息
                    OleDbConnection AconnStr;  //添加一个私有的连接对象   
                    string con = Start.DBConn + Start.access_key;
                    AconnStr = new OleDbConnection(con);  //设定连接数据库
                    AconnStr.Open(); //打开连接s
                    //插入管理员角色
                    string ss = "Insert into RoleAccessControl(RoleName,AFile,BFile,CFile,FolderOperation) values(@RoleName,@AFile,@BFile,@CFile,@FolderOperation);";
                    OleDbCommand Icmd = new OleDbCommand(ss, AconnStr);
                    Icmd.Parameters.Clear();
                    Icmd.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString("Admin")));
                    Icmd.Parameters.Add(new OleDbParameter("@AFile", SM4.EnString("1")));
                    Icmd.Parameters.Add(new OleDbParameter("@BFile", SM4.EnString("1")));
                    Icmd.Parameters.Add(new OleDbParameter("@CFile", SM4.EnString("1")));
                    Icmd.Parameters.Add(new OleDbParameter("@FolderOperation", SM4.EnString("1")));
                    Icmd.ExecuteNonQuery();

                    //插入管理员用户信息
                    ss = "Insert into UserList(UserName,UserPwd,Question,Answer,NineLock,UserRole,IdentityInformation,LastLoginTime,UserState) values(@UserName,@UserPwd,@Question,@Answer,@NineLock,@UserRole,@IdentityInformation,@LastLoginTime,@UserState)";
                    Icmd = new OleDbCommand(ss, AconnStr);
                    Icmd.Parameters.Clear();
                    Icmd.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(textBox5.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@UserPwd", SM4.EnString(textBox1.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@Question", SM4.EnString(textBox2.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@Answer", SM4.EnString(textBox3.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@NineLock", SM4.EnString(jieSuo)));
                    Icmd.Parameters.Add(new OleDbParameter("@UserRole", SM4.EnString("Admin")));
                    Icmd.Parameters.Add(new OleDbParameter("@IdentityInformation", "null"));
                    Icmd.Parameters.Add(new OleDbParameter("@LastLoginTime", DateTime.Now.ToString()));
                    Icmd.Parameters.Add(new OleDbParameter("@UserState", SM4.EnString("0")));
                    Icmd.ExecuteNonQuery();

                    //生成随机密钥并插入               
                    byte[] Key = SM4.GenerateRandomBytes();
                    char[] chr = new char[16];
                    for (int i = 0; i < Key.Length; i++)
                    {
                        chr[i] = (char)Key[i];
                    }
                    string aKey = new string(chr);
                    Thread.Sleep(10);//阻塞0.01秒，防止随机相同
                    Key = SM4.GenerateRandomBytes();
                    chr = new char[16];
                    for (int i = 0; i < Key.Length; i++)
                    {
                        chr[i] = (char)Key[i];
                    }
                    string bKey = new string(chr);
                    Thread.Sleep(10);//阻塞0.01秒，防止随机相同
                    Key = SM4.GenerateRandomBytes();//用函数 伪随机发生器安全  自己写的随机不安全
                    chr = new char[16];
                    for (int i = 0; i < Key.Length; i++)
                    {
                        chr[i] = (char)Key[i];
                    }
                    string cKey = new string(chr);
                    ss = "Insert into KeyList(AKey,BKey,CKey) values(@AKey,@BKey,@CKey);";
                    Icmd = new OleDbCommand(ss, AconnStr);
                    Icmd.Parameters.Clear();
                    Icmd.Parameters.Add(new OleDbParameter("@AKey", SM4.EnString(aKey, textBox1.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@BKey", SM4.EnString(bKey, textBox1.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@CKey", SM4.EnString(cKey, textBox1.Text)));
                    Icmd.ExecuteNonQuery();

                    AconnStr.Close();
                    AconnStr = null;
                    Icmd = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("注册失败！\r\n" + ex.Message);
                    return;
                }
                Start.UserName = textBox1.Text;

                //写readme文件
                string str = "管理员注册：首次打开应用-点击“注册”-填入信息-点击“设置”-返回登录界面------此时的用户即为管理员用户。\r\n\r\n";
                str += "切记：管理员用户有且只有一个，默认为第一个注册用户，不可更改！\r\n\r\n";
                str += "加密：登陆-点击“加密”-点击“添加文件”或“添加文件夹”-选择加密等级（管理员功能，非管理员无此步骤）-点击“加密”\r\n\r\n";
                str += "解密：登陆-点击“解密”-点击“添加文件”或“添加文件夹”-点击“加密”\r\n\r\n";
                str += "一键解密：（管理员功能，其他用户无此功能）登陆-点击“一键解密”\r\n\r\n";
                str += "说明：一键解密会解密所有已加密文件，此操作须谨慎！\r\n\r\n";
                str += "用户管理：（管理员功能，其他用户无此功能）登陆-点击“用户管理”-选择用户\r\n\r\n";
                str += "配置文件备份与恢复：（管理员功能，其他用户无此功能）登陆-点击“备份”,将配置文件备份,可以在配置文件出错进行恢复";
                StreamWriter swr = new StreamWriter(Start.ReadMeFile, true);
                swr.WriteLine(str);
                swr.Close();
                //是否备份配置文件
                if (MessageBox.Show("U盘初始化加密完成！\r\n备份配置文件能提高您的安全！\r\n是否进行配置文件的备份？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    Backup b = new Backup();
                    b.Show();
                }
                if (MessageBox.Show("当前无角色设置，其他用户无法注册，是否创建角色？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    UserManage2 u = new UserManage2();
                    u.Show();
                }
                System.Diagnostics.Process.Start(Start.ReadMeFile); //打开此文件
            }

            if (Start.First == false)//普通用户注册
            {
                if (comboBox1.Text == "")
                {
                    MessageBox.Show("未选择角色");
                    return;
                }
                if (textBox6.Text == "")
                    textBox6.Text = "null";
                OleDbConnection AconnStr;  //添加一个私有的连接对象      
                try
                {
                    string conn = Start.DBConn + Start.access_key;
                    AconnStr = new OleDbConnection(conn);  //设定连接数据库
                    AconnStr.Open(); //打开连接
                    string sql = string.Format("select * from UserList where UserName=@UserName");  //要为*
                    OleDbCommand command = new OleDbCommand(sql, AconnStr);//设定数据库操作指令，利用数据库语言
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(textBox5.Text.Trim())));
                    command.ExecuteNonQuery();
                    OleDbDataReader odr = null; //设定一个数据流对象
                    odr = command.ExecuteReader();//执行命令获取数据
                    odr.Read();
                    if (odr.HasRows)//存在记录
                    {
                        AconnStr.Close();
                        odr.Close();
                        MessageBox.Show("用户名已存在!");
                        return;
                    }
                    odr.Close(); //关闭数据流
                    string ss = "Insert into UserList(UserName,UserPwd,Question,Answer,NineLock,UserRole,IdentityInformation,LastLoginTime,UserState) values(@UserName,@UserPwd,@Question,@Answer,@NineLock,@UserRole,@IdentityInformation,@LastLoginTime,@UserState)";
                    OleDbCommand Icmd = new OleDbCommand(ss, AconnStr);
                    Icmd.Parameters.Clear();
                    Icmd.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(textBox5.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@UserPwd", SM4.EnString(textBox1.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@Question", SM4.EnString(textBox2.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@Answer", SM4.EnString(textBox3.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@NineLock", SM4.EnString(jieSuo)));
                    Icmd.Parameters.Add(new OleDbParameter("@UserRole", SM4.EnString(comboBox1.Text)));
                    Icmd.Parameters.Add(new OleDbParameter("@IdentityInformation", textBox6.Text));
                    Icmd.Parameters.Add(new OleDbParameter("@LastLoginTime", DateTime.Now.ToString()));
                    Icmd.Parameters.Add(new OleDbParameter("@UserState", SM4.EnString("1")));//未审核
                    Icmd.ExecuteNonQuery();
                    AconnStr.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("注册失败！\r\n" + ex.Message);
                    return;
                }
                Start.UserName = textBox1.Text;
                MessageBox.Show("注册成功！");
            }

            //写校验文件
            SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");
            start.ifFirst();
            start.setin(textBox5.Text, textBox1.Text);
            start.Visible = true;
            this.Visible = false;
            start.button1_Click(sender, e);//直接登录
            Application.DoEvents();
        }

        private void jiaMi_Fast_FormClosed(object sender, FormClosedEventArgs e)
        {
            start.Visible = true;
        }

        private void jiaMi_Fast_Load(object sender, EventArgs e)
        {
            if (Start.First == true)//管理员用户注册
            {
                label8.Text = "";
                label9.Text = "";
                label10.Text = "";
                comboBox1.Visible = false;
                textBox6.Visible = false;
            }
            if (Start.First == false)//普通用户注册
            {
                roles.Clear();
                comboBox1.Items.Clear();
                string conn = Start.DBConn + Start.access_key;
                OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                AconnStr.Open(); //打开连接
                OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                command.Connection = AconnStr;
                command.CommandText = string.Format("select * from RoleAccessControl");
                command.ExecuteNonQuery();
                OleDbDataReader odr = null; //设定一个数据流对象
                odr = command.ExecuteReader();//执行命令获取数据
                while (odr.Read())
                {
                    if (odr.HasRows)//存在记录
                    {
                        if (SM4.DeString(odr["RoleName"].ToString()).Replace("\0", "") != "Admin")
                            roles.Add(SM4.DeString(odr["RoleName"].ToString()).Replace("\0", ""));
                    }
                }
                odr.Close(); //关闭数据流
                AconnStr.Close();
                for (int i = 0; i < roles.Count; i++)
                {
                    comboBox1.Items.Add(roles[i]);
                }
                comboBox1.SelectedIndex = -1;
            }
        }


        //以下为九宫解锁实现
        bool visible = true;
        private string jieSuo = "";

        private void button11_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button11.Image = bmp;
            if (jieSuo.IndexOf("1") < 0)
                jieSuo += "1";
        }

        private void button2_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button2.Image = bmp;
            if (jieSuo.IndexOf("2") < 0)
                jieSuo += "2";
        }

        private void button3_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button3.Image = bmp;
            if (jieSuo.IndexOf("3") < 0)
                jieSuo += "3";
        }

        private void button4_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button4.Image = bmp;
            if (jieSuo.IndexOf("4") < 0)
                jieSuo += "4";
        }

        private void button5_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button5.Image = bmp;
            if (jieSuo.IndexOf("5") < 0)
                jieSuo += "5";
        }

        private void button6_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button6.Image = bmp;
            if (jieSuo.IndexOf("6") < 0)
                jieSuo += "6";
        }

        private void button7_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button7.Image = bmp;
            if (jieSuo.IndexOf("7") < 0)
                jieSuo += "7";
        }

        private void button8_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button8.Image = bmp;
            if (jieSuo.IndexOf("8") < 0)
                jieSuo += "8";
        }

        private void button9_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button9.Image = bmp;
            if (jieSuo.IndexOf("9") < 0)
                jieSuo += "9";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            button11.Image = bmp;
            button2.Image = bmp;
            button3.Image = bmp;
            button4.Image = bmp;
            button5.Image = bmp;
            button6.Image = bmp;
            button7.Image = bmp;
            button8.Image = bmp;
            button9.Image = bmp;
            jieSuo = "";
        }


        /// <summary> 
        /// C#截取定长字符串函数         
        /// </summary> 
        /// <param name="string1">原字符串</param> 
        /// <param name="Len1">长度（原字符串中全是汉字时的汉字个数）</param>         
        /// <returns>截取后的字符串（ReCutStr）</returns>         
        public static string CutStr(string string1, int Len1)
        {
            string ReCutstr = string.Empty;
            bool Remark = false;//如果不够长度，为假             
            int n = string1.Length;
            int TrueLen = 0;
            int Pcount = 0; //英文字计数             
            int Gcount = 0; //中文字计数            
            int Lcount = 0; //长度计数  
            char[] bytes = string1.ToCharArray();
            foreach (char chrA in bytes)
            {
                if (Convert.ToInt32(chrA) >= 0 && Convert.ToInt32(chrA) <= 255)
                {
                    Pcount = Pcount + 1;
                }
                else //如果是中文                 
                {
                    Gcount = Gcount + 1;
                }
                Lcount = Pcount + Gcount*2;
                TrueLen = Pcount + Gcount;
                if (Lcount > Len1)//如果长度已够                 
                {
                    ReCutstr = string1.Substring(0, TrueLen) ;
                    Remark = true;
                    break;
                }
            }
            if (!Remark)//如果是不够长度           
            {
                ReCutstr = string1;
            }
            return ReCutstr;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.Encoding.Default.GetBytes(textBox5.Text).Length > 16)
            {
                MessageBox.Show("输入长度超限");
                textBox5.Text = CutStr(textBox5.Text, 15);
                return;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.Encoding.Default.GetBytes(textBox2.Text).Length > 16)
            {
                MessageBox.Show("输入长度超限");
                textBox2.Text = CutStr(textBox2.Text, 15);
                return;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.Encoding.Default.GetBytes(textBox3.Text).Length > 16)
            {
                MessageBox.Show("输入长度超限");
                textBox3.Text = CutStr(textBox3.Text, 15);
                return;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 16)
            {
                MessageBox.Show("输入长度超限，密码不超过16位");
                textBox1.Text = textBox1.Text.Substring(0, 16);
                return;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text.Length > 16)
            {
                MessageBox.Show("输入长度超限，密码不超过16位");
                textBox4.Text = textBox4.Text.Substring(0, 16);
                return;
            }
        }
    }
}
