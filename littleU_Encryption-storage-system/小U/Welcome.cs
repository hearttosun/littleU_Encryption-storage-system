using BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace 小U
{
    public partial class Welcome : Form
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private void Welcome_Load(object sender, EventArgs e)
        {
            this.Opacity = 0;//透明度为0
            this.ClientSize = this.BackgroundImage.Size;//界面图像大小一致      
            this.timer1.Interval = 50;
            this.timer1.Enabled = true;
            this.timer1.Start();
        }

        private bool isFade = true;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isFade)
            {
                this.Opacity += 0.035;
                if (this.Opacity >= 1)
                {
                    isFade = false;
                }
            }
            else
            {
                this.timer1.Stop();
                label1.Text = "正在检查SM3DLL.dll模块是否存在";
                if (!File.Exists("SM3DLL.dll"))
                {
                    MessageBox.Show("未找到SM3DLL.dll模块,应用程序关闭！", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                label1.Text = "正在检查SM4DLL.dll模块是否存在";
                if (!File.Exists("SM4DLL.dll"))
                {
                    MessageBox.Show("未找到SM4DLL.dll模块,应用程序关闭！", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                label1.Text = "                               检查完成";
                Start start = new Start();
                start.Show();
                this.Visible = false;
            }
        }
    }
}
