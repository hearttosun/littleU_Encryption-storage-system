using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using BLL;

namespace 小U
{
    public partial class Find : Form
    {
        public Find()
        {
            InitializeComponent();
        }

        private void Find_Load(object sender, EventArgs e)
        {
            label4.Visible = false;
            textBox3.Visible = false;
            try
            {
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
                if (odr.HasRows)//存在记录
                {
                    label3.Text = SM4.DeString(odr["Question"].ToString());
                    odr.Close(); //关闭数据流
                    AconnStr.Close();
                }
                else
                {
                    odr.Close(); //关闭数据流
                    AconnStr.Close();
                    return;
                }
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
                string answer = "";
                string mima = "";
                if (odr.HasRows)//存在记录
                {
                    answer = SM4.DeString(odr["Answer"].ToString());
                    answer = answer.Replace("\0", "");//去掉结束符
                    mima = SM4.DeString(odr["UserPwd"].ToString());
                    mima = mima.Replace("\0", "");//去掉结束符
                    odr.Close(); //关闭数据流
                    AconnStr.Close();
                }
                else
                {
                    odr.Close(); //关闭数据流
                    AconnStr.Close();
                    return;
                }
                label4.Visible = true;
                textBox3.Visible = true;
                if (textBox2.Text != answer)
                {
                    textBox3.Text = "答案错误！";
                    return;
                }
                else
                {
                    textBox3.Text = mima;
                }
                textBox2.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.Encoding.Default.GetBytes(textBox2.Text).Length > 16)
            {
                MessageBox.Show("输入长度超限");
                textBox2.Text = Register.CutStr(textBox2.Text, 15);
                return;
            }
        }
    }
}
