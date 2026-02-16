using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace WackeClient
{
    internal static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;
        private static Main MainForm;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            if (!IsRunAsAdministrator())
            {
                // 重新以管理员身份启动
                var processInfo = new ProcessStartInfo
                {
                    FileName = System.Windows.Forms.Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas" // 关键：以管理员身份运行
                };

                try
                {
                    Process.Start(processInfo);
                }
                catch
                {
                    // 用户取消UAC时可处理
                    return;
                }
                return; // 退出当前进程
            }
            




            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            
            //MainForm = new Main();
            //Application.Run(MainForm);//直接运行主程序
            
            using (Login LoginForm = new Login())
            {
                DialogResult result = LoginForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    MainForm = new Main();
                   Application.Run(MainForm);
                }
            }
            
        }

        // 将静态本地函数移到类级别以解决 CS8370 错误
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            AntdUI.Notification.error(MainForm, "未处理的UI线程异常", e.Exception.Message, autoClose: 3, align: AntdUI.TAlignFrom.TR);
            
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AntdUI.Notification.error(MainForm, "未处理的非UI线程异常", e.ToString(), autoClose: 3, align: AntdUI.TAlignFrom.TR);

        }

        private static bool IsRunAsAdministrator()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

    }
}
