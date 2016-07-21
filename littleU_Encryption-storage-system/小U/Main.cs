using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace 小U
{
    public partial class Main : Form
    {
        public const int WM_DEVICECHANGE = 0x219;//U盘插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  //一个设备或媒体片已被删除。

        public Main()
        {
            InitializeComponent();
        }

        private void UI_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width - 35, Screen.PrimaryScreen.Bounds.Height - this.Height - 35);
            if (Start.roleName != "Admin")
            {
                button2.Visible = false;
                button6.Visible = false;
            }
            notifyIcon1.Icon = this.Icon;
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "小U";
        }

        #region 实现移动
        private bool isMouseDown = false;
        private Point FormLocation;//form的location
        private Point mouseOffset;//鼠标的按下位置

        private void label2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = true;
                FormLocation = this.Location;
                mouseOffset = Control.MousePosition;
            }
        }

        private void label2_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }

        private void label2_MouseMove(object sender, MouseEventArgs e)
        {
            int _x = 0;
            int _y = 0;
            if (isMouseDown)
            {
                Point pt = Control.MousePosition;
                _x = mouseOffset.X - pt.X;
                _y = mouseOffset.Y - pt.Y;
                this.Location = new Point(FormLocation.X - _x, FormLocation.Y - _y);
            }
        }
        #endregion

        //U盘消息
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                if (m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
                {
                    if (!File.Exists(Start.DBFile))
                    {
                        MessageBox.Show("U盘已拔出，程序关闭");
                        //强制关闭
                        System.Diagnostics.Process tt = System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id);
                        tt.Kill();
                    }
                }
            }
            base.WndProc(ref m);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((Start.AFileAuthority == "2" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "2" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "2" || Start.CFileAuthority == "4"))
            {
                MessageBox.Show("您不具备任何加密权限！");
                return;
            }
            Encryption j = new Encryption();
            j.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((Start.AFileAuthority == "3" || Start.AFileAuthority == "4") && (Start.BFileAuthority == "3" || Start.BFileAuthority == "4") && (Start.CFileAuthority == "3" || Start.CFileAuthority == "4"))
            {
                MessageBox.Show("您不具备任何解密权限！");
                return;
            }
            Decryption j = new Decryption();
            j.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Start.roleName != "Admin")
            {
                MessageBox.Show("您无权进行此操作，请勿越权操作！");
                return;
            }
            UserManage2 u = new UserManage2();
            u.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            About a = new About();
            a.Show();
        }

        private bool control = false;
        private void label2_Click(object sender, EventArgs e)
        {
            if (control == false)
            {
                button1.Visible = false;
                button2.Visible = false;
                button3.Visible = false;
                button4.Visible = false;
                button5.Visible = false;
                button6.Visible = false;
                control = true;
            }
            else
            {
                if (Start.roleName == "Admin")
                {
                    button2.Visible = true;
                    button6.Visible = true;
                }
                button1.Visible = true;             
                button3.Visible = true;
                button4.Visible = true;
                button5.Visible = true;            
                control = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (Start.roleName != "Admin")
            {
                MessageBox.Show("您无权进行此操作，请勿越权操作！");
                return;
            }
            Backup b = new Backup();
            b.Show();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.Focus();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Visible = true;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {     
            if (Encryption.enIng == true)
            {
                MessageBox.Show("正在加密！请等加密完成再关闭！");
                return;
            }
            if (Decryption.deIng == true)
            {
                MessageBox.Show("正在解密！请等解密完成再关闭！");
                return;
            }
            DialogResult bu = MessageBox.Show("确认退出？", "小U提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (bu.ToString() == "OK")
            {
                //强制关闭 Application.Exit();无效
                System.Diagnostics.Process tt = System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id);
                tt.Kill();
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消关闭
            e.Cancel = true;
            this.Visible = false;
        }

    }
}
