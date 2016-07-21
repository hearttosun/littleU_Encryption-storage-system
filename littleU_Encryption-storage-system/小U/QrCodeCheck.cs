using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//添加类库
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data.OleDb;
using BLL;

namespace 小U
{
    public partial class QrCodeCheck : Form
    {
        //添加私有成员
        private Socket rsock = null;
        public Thread th1;
        private string name = "";
        private string pwd = "";
        public static bool check = false;

        //添加方法
        private void Receive()
        {
            byte[] data;
            string stringData = "";
            int recv = 0;
            Control.CheckForIllegalCrossThreadCalls = false;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 7915);
            rsock = new Socket(ipep.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            rsock.Bind(ipep);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint ep = (EndPoint)sender;
            while (true)
            {
                data = new byte[1024];
                try
                {
                    recv = rsock.ReceiveFrom(data, ref ep);
                    stringData = Encoding.UTF8.GetString(data, 0, recv);
                    if (stringData.Contains(","))
                    {
                        name = stringData.Substring(0, stringData.IndexOf(","));
                        pwd = stringData.Substring(stringData.IndexOf(",") + 1, stringData.Length - stringData.IndexOf(",") - 1);
                        string conn = Start.DBConn + Start.access_key;
                        OleDbConnection AconnStr = new OleDbConnection(conn);  //设定连接数据库
                        AconnStr.Open(); //打开连接
                        OleDbCommand command = new OleDbCommand();//设定数据库操作指令，利用数据库语言
                        command.Connection = AconnStr;
                        command.CommandText = string.Format("select * from UserList where UserName=@UserName and UserPwd=@UserPwd");
                        command.Parameters.Clear();
                        command.Parameters.Add(new OleDbParameter("@UserName", SM4.EnString(name)));
                        command.Parameters.Add(new OleDbParameter("@UserPwd", SM4.EnString(pwd)));
                        command.ExecuteNonQuery();
                        OleDbDataReader odr = null; //设定一个数据流对象
                        odr = command.ExecuteReader();//执行命令获取数据
                        odr.Read();
                        if (odr.HasRows)//存在记录
                        {
                            odr.Close(); //关闭数据流
                            AconnStr.Close();
                            label3.Text = "  验证成功！请点击登录按钮或按回车键";
                            label3.ForeColor = Color.Blue;
                            button1.Visible = true;
                            check = true;
                            label1.Image = imageList1.Images[0];
                            break;
                        }
                        else
                        {
                            MessageBox.Show("用户名或密码错误！请在小U Android端重新设置用户信息后重试");
                        }
                    }
                }
                catch
                {
                    break;
                }              
            }
        }

        private Start start;

        public QrCodeCheck(Start s)
        {
            InitializeComponent();
            IPAddress ipAddr = Dns.Resolve(Dns.GetHostName()).AddressList[0];//获得局域网P地址
            string ip = ipAddr.ToString();
            label1.Image = QrCode.GenByZXingNet(ip);
            start = s;
            th1 = new Thread(new ThreadStart(Receive));
            th1.Start();
        }

        //发送消息
        private void BroadCast()
        {
            byte[] data = new byte[1024];
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep1 = new IPEndPoint(IPAddress.Broadcast, 7916);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            data = Encoding.UTF8.GetBytes("hello");
            sock.SendTo(data, ipep1);
            sock.Close();
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Visible = false;
        }

        private void QrCodeCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;//不关闭窗口
            this.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((name != "") && (pwd != ""))
            {
                start.ifFirst();
                start.setin(name, pwd);
                start.Visible = true;
                this.Visible = false;
                start.button1_Click(sender, e);//直接登录 
            }
        }
    }
}
