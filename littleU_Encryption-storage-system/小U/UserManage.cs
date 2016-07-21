using BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace 小U
{
    public partial class UserManage2 : Form
    {
        private List<string> roles = new List<string>();
        string choose = "";
        private List<string> users = new List<string>();

        public UserManage2()
        {
            InitializeComponent();
        }

        private void UserManage2_Load(object sender, EventArgs e)
        {
            comboBox3.Enabled = false;
            roles.Clear();
            comboBox1.Items.Clear();
            comboBox3.Items.Clear();
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
                comboBox3.Items.Add(roles[i]);
            }
            comboBox1.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                UpdateCheckStatus(e);
            }
        }

        // update check status for parent and child
        private void UpdateCheckStatus(TreeViewEventArgs e)
        {
            CheckAllChildNodes(e.Node);
            UpdateAllParentNodes(e.Node);
        }

        // updates all parent nodes recursively.
        private void UpdateAllParentNodes(TreeNode treeNode)
        {
            TreeNode parent = treeNode.Parent;
            if (parent != null)
            {
                if (parent.Checked && !treeNode.Checked)
                {
                    parent.Checked = false;
                    UpdateAllParentNodes(parent);
                }
                else if (!parent.Checked && treeNode.Checked)
                {
                    bool all = true;
                    foreach (TreeNode node in parent.Nodes)
                    {
                        if (!node.Checked)
                        {
                            all = false;
                            break;
                        }
                    }
                    if (all)
                    {
                        parent.Checked = true;
                        UpdateAllParentNodes(parent);
                    }
                }
            }
        }

        // updates all child tree nodes recursively.
        private void CheckAllChildNodes(TreeNode treeNode)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.Checked = treeNode.Checked;
                if (node.Nodes.Count > 0)
                {
                    // If the current node has child nodes, call the CheckAllChildsNodes method recursively.
                    this.CheckAllChildNodes(node);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("请输入角色名称");
                return;
            }
            try
            {
                string conn = Start.DBConn + Start.access_key;
                OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                AconnStr.Open(); //打开连接
                string sql = string.Format("select * from RoleAccessControl where RoleName=@RoleName");  //要为*
                OleDbCommand command = new OleDbCommand(sql, AconnStr);//设定数据库操作指令，利用数据库语言
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(textBox1.Text.Trim())));
                command.ExecuteNonQuery();
                OleDbDataReader odr = null; //设定一个数据流对象
                odr = command.ExecuteReader();//执行命令获取数据
                odr.Read();
                if (odr.HasRows)//存在记录
                {
                    AconnStr.Close();
                    odr.Close();
                    MessageBox.Show("角色已存在!");
                    return;
                }
                odr.Close(); //关闭数据流
                string ss = "Insert into RoleAccessControl(RoleName,AFile,BFile,CFile,FolderOperation) values(@RoleName,@AFile,@BFile,@CFile,@FolderOperation)";
                OleDbCommand Icmd = new OleDbCommand(ss, AconnStr);
                Icmd.Parameters.Clear();
                Icmd.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(textBox1.Text)));
                string ac = "";
                string bc = "";
                string cc = "";
                string fc = "";
                if (treeView1.Nodes[0].Checked) ac = "1";//加解密
                if (treeView1.Nodes[0].Nodes[0].Checked && (!treeView1.Nodes[0].Nodes[1].Checked)) ac = "2";//解密
                if (treeView1.Nodes[0].Nodes[1].Checked && (!treeView1.Nodes[0].Nodes[0].Checked)) ac = "3";//加密
                if ((!treeView1.Nodes[0].Nodes[0].Checked) && (!treeView1.Nodes[0].Nodes[1].Checked)) ac = "4";//无权

                if (treeView1.Nodes[1].Checked) bc = "1";//加解密
                if (treeView1.Nodes[1].Nodes[0].Checked && (!treeView1.Nodes[1].Nodes[1].Checked)) bc = "2";//解密
                if (treeView1.Nodes[1].Nodes[1].Checked && (!treeView1.Nodes[1].Nodes[0].Checked)) bc = "3";//加密
                if ((!treeView1.Nodes[1].Nodes[0].Checked) && (!treeView1.Nodes[1].Nodes[1].Checked)) bc = "4";//无权

                if (treeView1.Nodes[2].Checked) cc = "1";//加解密
                if (treeView1.Nodes[2].Nodes[0].Checked && (!treeView1.Nodes[2].Nodes[1].Checked)) cc = "2";//解密
                if (treeView1.Nodes[2].Nodes[1].Checked && (!treeView1.Nodes[2].Nodes[0].Checked)) cc = "3";//加密
                if ((!treeView1.Nodes[2].Nodes[0].Checked) && (!treeView1.Nodes[2].Nodes[1].Checked)) cc = "4";//无权

                if (treeView1.Nodes[3].Checked) fc = "1";//加解密
                if (treeView1.Nodes[3].Nodes[0].Checked && (!treeView1.Nodes[3].Nodes[1].Checked)) fc = "2";//解密
                if (treeView1.Nodes[3].Nodes[1].Checked && (!treeView1.Nodes[3].Nodes[0].Checked)) fc = "3";//加密
                if ((!treeView1.Nodes[3].Nodes[0].Checked) && (!treeView1.Nodes[3].Nodes[1].Checked)) fc = "4";//无权
                Icmd.Parameters.Add(new OleDbParameter("@AFile", SM4.EnString(ac)));
                Icmd.Parameters.Add(new OleDbParameter("@BFile", SM4.EnString(bc)));
                Icmd.Parameters.Add(new OleDbParameter("@CFile", SM4.EnString(cc)));
                Icmd.Parameters.Add(new OleDbParameter("@FolderOperation", SM4.EnString(fc)));
                Icmd.ExecuteNonQuery();
                AconnStr.Close();
                SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败！\r\n" + ex.Message);
                return;
            }
            Start.UserName = textBox1.Text;
            treeViewInit();
            MessageBox.Show("添加成功！");
            textBox1.Text = "";
            UserManage2_Load(sender, e);
        }
      
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string ac = "";
                string bc = "";
                string cc = "";
                string fc = "";
                if (treeView1.Nodes[0].Checked) ac = "1";//加解密
                if (treeView1.Nodes[0].Nodes[0].Checked && (!treeView1.Nodes[0].Nodes[1].Checked)) ac = "2";//解密
                if (treeView1.Nodes[0].Nodes[1].Checked && (!treeView1.Nodes[0].Nodes[0].Checked)) ac = "3";//加密
                if ((!treeView1.Nodes[0].Nodes[0].Checked) && (!treeView1.Nodes[0].Nodes[1].Checked)) ac = "4";//无权

                if (treeView1.Nodes[1].Checked) bc = "1";//加解密
                if (treeView1.Nodes[1].Nodes[0].Checked && (!treeView1.Nodes[1].Nodes[1].Checked)) bc = "2";//解密
                if (treeView1.Nodes[1].Nodes[1].Checked && (!treeView1.Nodes[1].Nodes[0].Checked)) bc = "3";//加密
                if ((!treeView1.Nodes[1].Nodes[0].Checked) && (!treeView1.Nodes[1].Nodes[1].Checked)) bc = "4";//无权

                if (treeView1.Nodes[2].Checked) cc = "1";//加解密
                if (treeView1.Nodes[2].Nodes[0].Checked && (!treeView1.Nodes[2].Nodes[1].Checked)) cc = "2";//解密
                if (treeView1.Nodes[2].Nodes[1].Checked && (!treeView1.Nodes[2].Nodes[0].Checked)) cc = "3";//加密
                if ((!treeView1.Nodes[2].Nodes[0].Checked) && (!treeView1.Nodes[2].Nodes[1].Checked)) cc = "4";//无权

                if (treeView1.Nodes[3].Checked) fc = "1";//加解密
                if (treeView1.Nodes[3].Nodes[0].Checked && (!treeView1.Nodes[3].Nodes[1].Checked)) fc = "2";//解密
                if (treeView1.Nodes[3].Nodes[1].Checked && (!treeView1.Nodes[3].Nodes[0].Checked)) fc = "3";//加密
                if ((!treeView1.Nodes[3].Nodes[0].Checked) && (!treeView1.Nodes[3].Nodes[1].Checked)) fc = "4";//无权
                
                string conn = Start.DBConn + Start.access_key;
                OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                AconnStr.Open(); //打开连接
                OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                command.Connection = AconnStr;
                command.CommandText = string.Format("update RoleAccessControl set AFile=@AFile where RoleName=@RoleName");
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@AFile", SM4.EnString(ac)));
                command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(comboBox1.Text)));
                command.ExecuteNonQuery();
                command.CommandText = string.Format("update RoleAccessControl set BFile=@BFile where RoleName=@RoleName");
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@BFile", SM4.EnString(bc)));
                command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(comboBox1.Text)));
                command.ExecuteNonQuery();
                command.CommandText = string.Format("update RoleAccessControl set CFile=@CFile where RoleName=@RoleName");
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@CFile", SM4.EnString(cc)));
                command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(comboBox1.Text)));
                command.ExecuteNonQuery();
                command.CommandText = string.Format("update RoleAccessControl set FolderOperation=@FolderOperation where RoleName=@RoleName");
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@FolderOperation", SM4.EnString(fc)));
                command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(comboBox1.Text)));
                command.ExecuteNonQuery();
                SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show("设置成功");
        }

        private void treeViewInit()
        {
            treeView1.Nodes[0].Checked = false;
            treeView1.Nodes[0].Nodes[0].Checked = false;
            treeView1.Nodes[0].Nodes[1].Checked = false;
            treeView1.Nodes[1].Checked = false;
            treeView1.Nodes[1].Nodes[0].Checked = false;
            treeView1.Nodes[1].Nodes[1].Checked = false;
            treeView1.Nodes[2].Checked = false;
            treeView1.Nodes[2].Nodes[0].Checked = false;
            treeView1.Nodes[2].Nodes[1].Checked = false;
            treeView1.Nodes[3].Checked = false;
            treeView1.Nodes[3].Nodes[0].Checked = false;
            treeView1.Nodes[3].Nodes[1].Checked = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                treeViewInit();
                treeView1.ExpandAll();
                string conn = Start.DBConn + Start.access_key;
                OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                AconnStr.Open(); //打开连接
                OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                command.Connection = AconnStr;
                command.CommandText = string.Format("select * from RoleAccessControl where RoleName=@RoleName");  //防注入
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(comboBox1.Text)));
                command.ExecuteNonQuery();
                OleDbDataReader odr = null; //设定一个数据流对象
                odr = command.ExecuteReader();//执行命令获取数据
                odr.Read();
                if (odr.HasRows)//存在记录
                {
                    string ac = SM4.DeString(odr["AFile"].ToString()).Replace("\0","");
                    string bc = SM4.DeString(odr["BFile"].ToString()).Replace("\0", "");
                    string cc = SM4.DeString(odr["CFile"].ToString()).Replace("\0", "");
                    string fc = SM4.DeString(odr["FolderOperation"].ToString()).Replace("\0", "");

                    if (ac == "1")
                    {
                        treeView1.Nodes[0].Checked = true;
                        treeView1.Nodes[0].Nodes[0].Checked = true;
                        treeView1.Nodes[0].Nodes[1].Checked = true;
                    }

                    if (ac == "2")
                    {
                        treeView1.Nodes[0].Checked = false;
                        treeView1.Nodes[0].Nodes[0].Checked = true;
                        treeView1.Nodes[0].Nodes[1].Checked = false;
                    }

                    if (ac == "3")
                    {
                        treeView1.Nodes[0].Checked = false;
                        treeView1.Nodes[0].Nodes[0].Checked = false;
                        treeView1.Nodes[0].Nodes[1].Checked = true;
                    }

                    if (ac == "4")
                    {
                        treeView1.Nodes[1].Checked = false;
                        treeView1.Nodes[1].Nodes[0].Checked = false;
                        treeView1.Nodes[1].Nodes[1].Checked = false;
                    }

                    if (bc == "1")
                    {
                        treeView1.Nodes[1].Checked = true;
                        treeView1.Nodes[1].Nodes[0].Checked = true;
                        treeView1.Nodes[1].Nodes[1].Checked = true;
                    }

                    if (bc == "2")
                    {
                        treeView1.Nodes[1].Checked = false;
                        treeView1.Nodes[1].Nodes[0].Checked = true;
                        treeView1.Nodes[1].Nodes[1].Checked = false;
                    }

                    if (bc == "3")
                    {
                        treeView1.Nodes[1].Checked = false;
                        treeView1.Nodes[1].Nodes[0].Checked = false;
                        treeView1.Nodes[1].Nodes[1].Checked = true;
                    }

                    if (bc == "4")
                    {
                        treeView1.Nodes[1].Checked = false;
                        treeView1.Nodes[1].Nodes[0].Checked = false;
                        treeView1.Nodes[1].Nodes[1].Checked = false;
                    }

                    if (cc == "1")
                    {
                        treeView1.Nodes[2].Checked = true;
                        treeView1.Nodes[2].Nodes[0].Checked = true;
                        treeView1.Nodes[2].Nodes[1].Checked = true;
                    }

                    if (cc == "2")
                    {
                        treeView1.Nodes[2].Checked = false;
                        treeView1.Nodes[2].Nodes[0].Checked = true;
                        treeView1.Nodes[2].Nodes[1].Checked = false;
                    }

                    if (cc == "3")
                    {
                        treeView1.Nodes[2].Checked = false;
                        treeView1.Nodes[2].Nodes[0].Checked = false;
                        treeView1.Nodes[2].Nodes[1].Checked = true;
                    }

                    if (cc == "4")
                    {
                        treeView1.Nodes[2].Checked = false;
                        treeView1.Nodes[2].Nodes[0].Checked = false;
                        treeView1.Nodes[2].Nodes[1].Checked = false;
                    }
                    if (fc == "1")
                    {
                        treeView1.Nodes[3].Checked = true;
                        treeView1.Nodes[3].Nodes[0].Checked = true;
                        treeView1.Nodes[3].Nodes[1].Checked = true;
                    }

                    if (fc == "2")
                    {
                        treeView1.Nodes[3].Checked = false;
                        treeView1.Nodes[3].Nodes[0].Checked = true;
                        treeView1.Nodes[3].Nodes[1].Checked = false;
                    }

                    if (fc == "3")
                    {
                        treeView1.Nodes[3].Checked = false;
                        treeView1.Nodes[3].Nodes[0].Checked = false;
                        treeView1.Nodes[3].Nodes[1].Checked = true;
                    }

                    if (fc == "4")
                    {
                        treeView1.Nodes[3].Checked = false;
                        treeView1.Nodes[3].Nodes[0].Checked = false;
                        treeView1.Nodes[3].Nodes[1].Checked = false;
                    }
                }
                odr.Close(); //关闭数据流
                AconnStr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("未选择角色！");
                return;
            }
            if (MessageBox.Show("删除角色会删除该角色下的所有用户，确认删除？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                try
                {
                    string conn = Start.DBConn + Start.access_key;
                    OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                    AconnStr.Open(); //打开连接
                    OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                    command.Connection = AconnStr;
                    command.CommandText = string.Format("delete from RoleAccessControl where RoleName=@RoleName");
                    //还得继续删除该角色下的所有用户
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@RoleName", SM4.EnString(comboBox1.Text)));
                    command.ExecuteNonQuery();
                    command.CommandText = string.Format("delete from UserList where UserRole=@UserRole");
                    //还得继续删除该角色下的所有用户
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@UserRole", SM4.EnString(comboBox1.Text)));
                    command.ExecuteNonQuery();
                    AconnStr.Close();
                    UserManage2_Load(sender, e);
                    if (choose == "new") button4_Click(sender, e);
                    if (choose == "old") button5_Click(sender, e);
                    SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");                  
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                MessageBox.Show("删除成功！");             
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            comboBox3.Enabled = false;
            users.Clear();
            comboBox2.Items.Clear();
            string conn = Start.DBConn + Start.access_key;
            OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
            AconnStr.Open(); //打开连接
            OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
            command.Connection = AconnStr;
            command.CommandText = string.Format("select * from UserList where UserState=@UserState");
            command.Parameters.Clear();
            command.Parameters.Add(new OleDbParameter("@UserState", SM4.EnString("1")));
            command.ExecuteNonQuery();
            OleDbDataReader odr = null; //设定一个数据流对象
            odr = command.ExecuteReader();//执行命令获取数据
            while (odr.Read())
            {
                if (odr.HasRows)//存在记录
                {
                    users.Add(SM4.DeString(odr["UserName"].ToString()).Replace("\0", ""));
                }
            }
            odr.Close(); //关闭数据流
            AconnStr.Close();
            for (int i = 0; i < users.Count; i++)
            {
                comboBox2.Items.Add(users[i]);
            }
            Start.xiugai = false;
            comboBox2.SelectedIndex = -1;
            if (comboBox2.Items.Count != 0)
            {
                comboBox2.SelectedIndex = 0;
                comboBox2_SelectedIndexChanged(sender, e);
            }
            label6.Text = "申请角色：";
            label8.Text = "申请时间：";
            groupBox4.Text = "注册信息";
            button6.Text = "审核通过";
            choose = "new";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            comboBox3.Enabled = true;
            users.Clear();
            comboBox2.Items.Clear();
            string conn = Start.DBConn + Start.access_key;
            OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
            AconnStr.Open(); //打开连接
            OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
            command.Connection = AconnStr;
            command.CommandText = string.Format("select * from UserList where UserState=@UserState and UserRole<>@UserRole");
            command.Parameters.Clear();
            command.Parameters.Add(new OleDbParameter("@UserState", SM4.EnString("0")));
            command.Parameters.Add(new OleDbParameter("@UserRole", SM4.EnString("Admin")));
            command.ExecuteNonQuery();
            OleDbDataReader odr = null; //设定一个数据流对象
            odr = command.ExecuteReader();//执行命令获取数据
            while (odr.Read())
            {
                if (odr.HasRows)//存在记录
                {
                    users.Add(SM4.DeString(odr["UserName"].ToString().Replace("\0", "")));
                }
            }
            odr.Close(); //关闭数据流
            AconnStr.Close();
            for (int i = 0; i < users.Count; i++)
            {
                comboBox2.Items.Add(users[i]);
            }
            Start.xiugai = false;
            comboBox2.SelectedIndex = -1;
            if (comboBox2.Items.Count != 0)
            {
                comboBox2.SelectedIndex = 0;
                comboBox2_SelectedIndexChanged(sender, e);
            }
            label6.Text = "所属角色：";
            label8.Text = "上次登录时间：";
            groupBox4.Text = "用户信息";
            button6.Text = "授予角色";
            choose = "old";
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string conn = Start.DBConn + Start.access_key;
            OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
            AconnStr.Open(); //打开连接
            OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
            command.Connection = AconnStr;
            command.CommandText = string.Format("select * from UserList where UserName=@Name");  //防注入
            command.Parameters.Clear();
            command.Parameters.Add(new OleDbParameter("@Name", SM4.EnString(comboBox2.Text)));
            command.ExecuteNonQuery();
            OleDbDataReader odr = null; //设定一个数据流对象
            odr = command.ExecuteReader();//执行命令获取数据
            odr.Read();
            if (odr.HasRows)//存在记录
            {
                comboBox3.Text = SM4.DeString(odr["UserRole"].ToString()).Replace("\0", "");
                comboBox1.Text = SM4.DeString(odr["UserRole"].ToString()).Replace("\0", "");
                comboBox1_SelectedIndexChanged(sender, e);
                textBox4.Text = SM4.DeString(odr["UserRole"].ToString()).Replace("\0", "");
                textBox5.Text = odr["IdentityInformation"].ToString().Replace("\0", "");
                textBox6.Text = odr["LastLoginTime"].ToString();
            }
            odr.Close(); //关闭数据流
            AconnStr.Close();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Text = comboBox3.Text;
            comboBox1_SelectedIndexChanged(sender, e);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox2.Text == "")
                {
                    MessageBox.Show("未选择用户！");
                    return;
                }
                if (MessageBox.Show("是否删除该用户？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {

                    string conn = Start.DBConn + Start.access_key;
                    OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                    AconnStr.Open(); //打开连接
                    OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                    command.Connection = AconnStr;
                    command.CommandText = string.Format("delete from UserList where UserName=@Name");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@Name", SM4.EnString(comboBox2.Text)));
                    command.ExecuteNonQuery();
                    AconnStr.Close();
                    SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");
                    MessageBox.Show("删除成功！");
                    if (choose == "old") button5_Click(sender, e);
                    if (choose == "new") button4_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox2.Text == "")
            {
                MessageBox.Show("未选择用户！");
                return;
            }
            if (choose == "old")//授予角色
            {
                if (comboBox3.Text=="")
                {
                    MessageBox.Show("未选择要授予用户的角色！");
                    return;
                }
                string conn = Start.DBConn + Start.access_key;
                OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                AconnStr.Open(); //打开连接
                OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                command.Connection = AconnStr;
                command.CommandText = string.Format("update UserList set UserRole=@UserRole where UserName=@Name");
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@UserRole", SM4.EnString(comboBox3.Text)));
                command.Parameters.Add(new OleDbParameter("@Name", SM4.EnString(comboBox2.Text)));
                command.ExecuteNonQuery();              
                MessageBox.Show("设置成功！");
                button5_Click(sender, e);
            }

            if (choose == "new")//通过审核
            {
                string conn = Start.DBConn + Start.access_key;
                OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                AconnStr.Open(); //打开连接
                OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                command = new OleDbCommand("update UserList set UserState=@UserState where UserName=@Name", AconnStr);
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@UserState", SM4.EnString("0")));
                command.Parameters.Add(new OleDbParameter("@Name", SM4.EnString(comboBox2.Text)));
                command.ExecuteReader();//执行命令获取数据
                AconnStr.Close();
                MessageBox.Show("审核成功！");
                button4_Click(sender, e);
            }
            SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");
            comboBox1_SelectedIndexChanged(sender, e);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.Encoding.Default.GetBytes(textBox1.Text).Length > 16)
            {
                MessageBox.Show("输入长度超限");
                textBox1.Text =Register.CutStr(textBox1.Text, 15);
                return;
            }
        }
    }
}
