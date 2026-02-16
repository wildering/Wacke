using AntdUI;
using Downloader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WackeClient.Ace;
using WackeClient.date;
using WackeClient.tool;


namespace WackeClient
{
    public partial class Login : AntdUI.Window
    {
        
        private static yyz Yyz = new yyz();
        private static DownloadConfiguration downloadOpt = new DownloadConfiguration()
        {
            ChunkCount = 8, // Number of file parts, default is 1
            ParallelDownload = true // Download parts in parallel (default is false)
        };
        DownloadService downloader = new DownloadService(downloadOpt);

        public Login()
        {
            
            InitializeComponent();
            //全局配置通知显示在窗口中
            AntdUI.Config.ShowInWindow = true;
            //...
            
            AceData.NowVersion = "1.0.0.1";
            MyPath.Configpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WackeConfig");
            MyPath.Oldexeconfigfpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Wackeoldexe");
            if (File.Exists(MyPath.Oldexeconfigfpath))
            {
                MyPath.oldversionpath = File.ReadAllText(MyPath.Oldexeconfigfpath);
            }
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (progress.InvokeRequired)
            {
                progress.Invoke(new Action(() =>
                {
                    progress.Value = (float)e.ProgressPercentage/100;
                }));
            }
            else
            {
                progress.Value = (float)e.ProgressPercentage/100;
            }

        }

        private async void LoginForm_Load(object sender, EventArgs e)
        {
            buttonlogin.Enabled = false;
            pageHeader.Loading = true;
            try
            {
                inputnotice.Text = await Yyz.获取变量("400", "", "");
                
                MyPath.Initialize();
                try
                {
                    File.Delete(MyPath.Oldexeconfigfpath);
                    File.Delete(MyPath.oldversionpath);
                }
                catch
                {

                }
                if (!MyPath.Chenckmypath())
                {
                    AntdUI.Modal.open(new AntdUI.Modal.Config(this, "WackeClient", "启动失败...\r\n有疑问？\r\n联系开发者...\r\n按任意键退出...")
                    {
                        Icon = TType.Info,
                        //内边距
                        Padding = new Size(24, 20),

                    });
                    Application.Exit();
                }
            }
            catch (Exception)
            {
                AntdUI.Modal.open(new AntdUI.Modal.Config(this, "WackeClient", "启动失败...\r\n有疑问？\r\n联系开发者...\r\n按任意键退出...")
                {
                    Icon = TType.Info,
                    //内边距
                    Padding = new Size(24, 20),

                });
                Application.Exit();
            }
            try
            {
                List<string> config = await Readconfig();
                if (config.Count == 2)
                {
                    checkbox.Checked = true;
                    inputuserna.Text = config[0];
                    inputpassword.Text = config[1];
                }
                
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this, ex.Message, autoClose: 3);
            }
            buttonlogin.Enabled = true;
            pageHeader.Loading = false;
        }
        private async void buttonlogin_Click(object sender, EventArgs e)
        {
            buttonlogin.Loading = true;
            buttonlogin.Enabled = false;
            AceData.Uid = inputuserna.Text.Trim();
            AceData.Password = inputpassword.Text.Trim();
            if (string.IsNullOrEmpty(AceData.Uid) == false && string.IsNullOrEmpty(AceData.Password) == false)
            {
                AceData.NewVersion = await Yyz.获取最新版本("","");
                if (AceData.NewVersion != AceData.NowVersion)
                {
                    DialogResult ressult = AntdUI.Modal.open(new AntdUI.Modal.Config(this, "温馨提示", "当前版本已不受支持\n是否更新最新版")
                    {
                        Icon = TType.Warn,
                        //内边距
                        Padding = new Size(24, 20),
                    });
                    if (ressult == DialogResult.OK)
                    {
                        buttonlogin.Enabled = false;//开始更新
                        await Task.Run(async () =>
                        {
                            MyPath.oldversionpath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                            File.WriteAllText(MyPath.Oldexeconfigfpath, MyPath.oldversionpath);
                            string file = Path.Combine(Path.GetDirectoryName(MyPath.oldversionpath), $"Wacke{AceData.NewVersion}.exe");
                            string url = AceData.UpdataUrl;
                            progress.Visible = true;
                            await downloader.DownloadFileTaskAsync(url, file);
                            var psi = new ProcessStartInfo
                            {
                                FileName = file,
                                Verb = "runas",             // 关键：触发 UAC 提升
                                UseShellExecute = true                 // runas 必须配合 ShellExecute
                            };
                            Process.Start(psi);  // 会弹出 UAC 确认框
                        });
                        Environment.Exit(0);
                    }
                    else
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"登录失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        buttonlogin.Loading = false;
                        buttonlogin.Enabled = true;
                        return;
                    }

                }
                bool issuccess = await Yyz.用户登录(AceData.Uid, AceData.Password, "");
                if (issuccess )
                {
                    if (checkbox.Checked == true)
                    {
                        await Writeconfig();
                    }
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"登录失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            else
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"无效的账户", TType.Warn)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            buttonlogin.Loading = false;
            buttonlogin.Enabled = true;
        }
        
        
        private async Task Writeconfig()
        {
            try
            {
                string encrypted = TextKey.Encrypt($"{AceData.Uid}||{AceData.Password}");
                await Task.Run(() => File.WriteAllText(MyPath.Configpath, $"{encrypted}"));

            }
            catch (Exception)
            {

            }
        }
        //<summary>
        /// <summary>
        /// 读取配置文件list第一个是用户名第二个是密码
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> Readconfig()
        {
            
            List<string> confige = new List<string>();
            try
            {
                if (File.Exists(MyPath.Configpath))
                {
                    string encrypted = await Task.Run(() => File.ReadAllText(MyPath.Configpath, Encoding.UTF8));
                    string text = TextKey.Decrypt(encrypted);
                    string[] normaltext = text.Split(new string[] { "||" }, StringSplitOptions.None);
                    confige.AddRange(normaltext);
                    return confige;
                }
                else
                {
                    return new List<string>();
                }
            }
            catch (Exception)
            {
                return confige;
            }
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if ( DialogResult != DialogResult.OK)
                {
                    Directory.Delete(MyPath.mainpath, true);
                }  
            }
            catch { }
        }


        
    }
}

