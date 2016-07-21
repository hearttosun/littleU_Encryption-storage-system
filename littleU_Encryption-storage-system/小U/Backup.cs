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
    public partial class Backup : Form
    {
        private Start start;
        private Welcome welcome;
        public Backup()
        {
            InitializeComponent();
        }

        public Backup(Welcome w)
        {
            InitializeComponent();
            welcome = w;
        }

        public Backup(Start s)
        {
            InitializeComponent();
            start = s;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("路径不能为空！");
                return;
            }
            try
            {
                if (!Directory.Exists(textBox1.Text))
                    Directory.CreateDirectory(textBox1.Text);
                string file1 = textBox1.Text + "DB.mdb";
                string file2 = textBox1.Text + "Check.xus";
                string file3 = textBox1.Text + "ReadMe.txt";
                File.Copy(Start.DBFile, file1, true);
                File.Copy(Start.CheckFile, file2, true);
                File.Copy(Start.ReadMeFile, file3, true);
                MessageBox.Show("备份完成！");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            if (f.ShowDialog() == DialogResult.Cancel)
                return;
            textBox1.Text = f.SelectedPath;
            if (textBox1.Text.Substring(textBox1.Text.Length - 1, 1) != @"\")
                textBox1.Text += @"\";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("路径不能为空！");
                return;
            }
            try
            {
                if (!Directory.Exists(textBox1.Text))
                {
                    MessageBox.Show("路径不存在！");
                    return;
                }
                string file1 = textBox1.Text + "DB.mdb";
                string file2 = textBox1.Text + "Check.xus";
                string file3 = textBox1.Text + "ReadMe.txt";
                if (!File.Exists(file1) || !File.Exists(file2) || !File.Exists(file3))
                {
                    MessageBox.Show("配置文件不存在！");
                    return;
                }
                if (!Directory.Exists(Start.path))//创建路径
                {
                    //创建目录
                    Directory.CreateDirectory(Start.path);
                    //隐藏文件夹
                    DirectoryInfo dir = new DirectoryInfo(Start.panFu + @"\小U安全");
                    dir.Attributes = FileAttributes.Hidden;
                }
                File.Copy(file1, Start.DBFile, true);
                File.Copy(file2, Start.CheckFile, true);
                File.Copy(file3, Start.ReadMeFile, true);
                MessageBox.Show("恢复完成！");
                if (start != null)
                    start.ifFirst();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Backup_Load(object sender, EventArgs e)
        {
            if (start != null)
            {
                button2.Enabled = false;
            }
            if (welcome != null)
            {
                button2.Enabled = false;
            }
        }
    }
}
