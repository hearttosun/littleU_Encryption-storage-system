using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace 小U
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //如果要实现单实例运行 不能再属性中选择优化代码 否则无法实现
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, "SINGLE_INSTANCE_MUTEX");
            if (!mutex.WaitOne(0, false))//请求互斥体的所属权
            {
                mutex.Close();
                mutex = null;
            }
            if (mutex != null)
            {
                Application.Run(new Welcome());
            }
            else
            {
                MessageBox.Show("程序已经启动");
            }
        }
    }
}
