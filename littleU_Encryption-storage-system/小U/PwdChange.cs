using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;//连接access数据库
using ADOX;
using System.Text.RegularExpressions;// 引用COM：Microsoft ADO Ext. 2.8 for DDL and Security 
//添加引用：Microsoft ActioveX Data Objects 2.8 Library
using BLL;

namespace 小U
{
    public partial class PwdChange : Form
    {
        private Start start;

        public PwdChange(Start s)
        {
            InitializeComponent();
            start = s;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
                {
                    MessageBox.Show("请输入完善！");
                    return;
                }
                if (textBox2.Text != textBox3.Text)
                {
                    MessageBox.Show("两次新密码不一致！");
                    return;
                }
                if (textBox2.Text.Length < 8)
                {
                    MessageBox.Show("新密码长度不够！");
                    return;
                }
                string pattern1 = @"[A-Za-z]";
                string pattern2 = @"[0-9]";
                Regex regex = new Regex(pattern1);
                Regex regex2 = new Regex(pattern2);
                if (!regex.IsMatch(textBox2.Text) || !regex2.IsMatch(textBox2.Text))
                {
                    MessageBox.Show("新密码必须为字母和数字的组合！");
                    return;
                }

                string conn = Start.DBConn + Start.access_key;
                OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                AconnStr.Open(); //打开连接
                OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                command.Connection = AconnStr;
                command.CommandText = string.Format("select * from UserList where UserName=@UserName");
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(Start.UserName)));
                command.ExecuteNonQuery();
                OleDbDataReader odr = null; //设定一个数据流对象
                odr = command.ExecuteReader();//执行命令获取数据
                odr.Read();
                string old_pwd = "";
                string role = "";
                if (odr.HasRows)//存在记录
                {
                    old_pwd = SM4.DeString(odr["UserPwd"].ToString());
                    old_pwd = old_pwd.Replace("\0", "");//去掉结束符
                    role = SM4.DeString(odr["UserRole"].ToString()).Replace("\0", "");
                }
                odr.Close(); //关闭数据流
                if (textBox1.Text != old_pwd)
                {
                    MessageBox.Show("原密码错误！");
                    return;
                }
                if (textBox2.Text == textBox1.Text)
                {
                    MessageBox.Show("新密码不能和原密码一致！");
                    return;
                }
                command.CommandText = string.Format("update UserList set UserPwd=@UserPwd where UserName=@UserName");
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@UserPwd", SM4.EnString(textBox2.Text)));
                command.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(Start.UserName)));
                command.ExecuteNonQuery();

                if (role == "Admin")
                {
                    //重新加密等级密钥
                    command.CommandText = string.Format("select * from KeyList");
                    command.ExecuteNonQuery();
                    odr = command.ExecuteReader();//执行命令获取数据
                    odr.Read();
                    string secA = "";
                    string secB = "";
                    string secC = "";
                    if (odr.HasRows)//存在记录
                    {
                        secA = SM4.DeString(odr["AKey"].ToString(), old_pwd);
                        secB = SM4.DeString(odr["BKey"].ToString(), old_pwd);
                        secC = SM4.DeString(odr["CKey"].ToString(), old_pwd);
                    }
                    odr.Close();
                    command.CommandText = string.Format("update KeyList set AKey=@AKey");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@AKey", SM4.EnString(secA, textBox2.Text)));
                    command.ExecuteNonQuery();
                    command.CommandText = string.Format("update KeyList set BKey=@BKey");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@BKey", SM4.EnString(secB, textBox2.Text)));
                    command.ExecuteNonQuery();
                    command.CommandText = string.Format("update KeyList set CKey=@CKey");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@CKey", SM4.EnString(secC, textBox2.Text)));
                    command.ExecuteNonQuery();
                }

                //重新加密校验值文件
                AconnStr.Close();
                SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");
                MessageBox.Show("修改成功！");
                start.xiuGai(textBox2.Text);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void XiuGai_Load(object sender, EventArgs e)
        {

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

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 16)
            {
                MessageBox.Show("输入长度超限，密码不超过16位");
                textBox2.Text = textBox2.Text.Substring(0, 16);
                return;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text.Length > 16)
            {
                MessageBox.Show("输入长度超限，密码不超过16位");
                textBox3.Text = textBox3.Text.Substring(0, 16);
                return;
            }
        }
    }
}
