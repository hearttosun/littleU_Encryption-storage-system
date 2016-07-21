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
    public partial class NineLock : Form
    {
        private Start start;
        private string jieSuo = "";
        private bool ok = false;

        public NineLock(Start s)
        {
            InitializeComponent();
            start = s;
        }

        private void NineLock_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width - 330, Screen.PrimaryScreen.Bounds.Height - this.Height - 263);
            button10_Click(sender, e);
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            if (visible == true)
            {
                bmp = new Bitmap(imageList1.Images[1]);
            }
            button1.Image = bmp;
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

        private void button10_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(imageList1.Images[0]);
            button1.Image = bmp;
            button2.Image = bmp;
            button3.Image = bmp;
            button4.Image = bmp;
            button5.Image = bmp;
            button6.Image = bmp;
            button7.Image = bmp;
            button8.Image = bmp;
            button9.Image = bmp;
            jieSuo = "";
            ok = false;
        }

        private void NineLock_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (check == true) return;//准备重设            
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
                string nineLock = "";
                string mima = "";
                if (odr.HasRows)//存在记录
                {
                    nineLock = SM4.DeString(odr["NineLock"].ToString());
                    nineLock = nineLock.Replace("\0", "");//去掉结束符
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
                if (ok == false && reset == false)//正常验证手势
                {
                    if (jieSuo == nineLock)
                    {
                        ok = true;
                        start.xiuGai(mima);
                        //直接完成登录  先将 Start类的button1_Click函数改为public      
                        start.button1_Click(sender, (EventArgs)e); //事件类型e不同 要强制转换        
                        this.Close();
                    }
                }

                if (reset == true)//重设手势前验证原手势
                {
                    if (jieSuo == nineLock)
                    {
                        check = true;
                        MessageBox.Show("验证通过！请输入新的手势！");
                        visible = true;//轨迹可见
                        linkLabel1.Text = "隐藏轨迹";
                        linkLabel2.Text = "确认重设";
                        button10_Click(sender, e);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        bool visible = false;
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            button10_Click(sender, e);
            if (visible == true)
            {
                visible = false;
                linkLabel1.Text = "显示轨迹";
                return;
            }
            if (visible == false)
            {
                visible = true;
                linkLabel1.Text = "隐藏轨迹";
                return;
            }
        }

        private bool reset = false;//判断重设
        private bool check = false;//第一步是否验证通过
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (reset == false)
                {
                    if (MessageBox.Show("重设手势需要验证原手势，是否重设？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        reset = true;
                        linkLabel2.Text = "验证原手势";
                    }
                }
                if (check == true)
                {

                    string conn = Start.DBConn + Start.access_key;
                    OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                    AconnStr.Open(); //打开连接
                    OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                    command.Connection = AconnStr;
                    command.CommandText = string.Format("update UserList set NineLock=@NineLock where UserName=@UserName");
                    command.Parameters.Clear();
                    command.Parameters.Add(new OleDbParameter("@NineLock", SM4.EnString(jieSuo)));
                    command.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(Start.UserName)));
                    command.ExecuteNonQuery();
                    AconnStr.Close();
                    check = false;
                    reset = false;
                    SM3.DBCheck(Start.panFu + @"\小U安全\\Config\\DB.mdb", Start.panFu + @"\小U安全\\Config\\Check.xus");
                    MessageBox.Show("设置成功！");
                    linkLabel2.Text = "重设";
                    button10_Click(sender, e);
                    visible = false;//轨迹可见
                    linkLabel1.Text = "显示轨迹";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
