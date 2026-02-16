using AntdUI;
using Partition.FluentApi;
using Partition.Gpt;
using Loader;
using Loader.code.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;
using WackeClient.Ace;
using WackeClient.date;
using WackeClient.tool;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace WackeClient
{
    public partial class Main : AntdUI.Window
    {
        private static Cmd cmd = new Cmd();
        private static yyz Yyz = new yyz();//登录验证程序
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static CancellationToken token = cts.Token;
        private static List<DeviceDate> Listdevice = new List<DeviceDate>();
        private static DeviceMonitor DeviceMonitor = new DeviceMonitor();
        private static IGptReader gptmainReader = Partition.DiskPartition.ReadGpt().Primary();
        private static bool isrun = false;
        private static int stopint = 0;
        private static Flash _flash;
        private static Stopwatch Stopwatch = new Stopwatch();
        private static FileInfo[] pachxmlpath;
        BindingList<Applist> applists = new BindingList<Applist>();
        BindingList<FastbootPartition> fastbootPartitions = new BindingList<FastbootPartition>();
        BindingList<Qcxmlinfo> QcPartitions = new BindingList<Qcxmlinfo>();
        private long _lastWritten = 0;//记录的位置
        private long _lastTime = 0;// 上一次记录的时间
        private static string adbapkpath = null;
        private static string fbrecpath = null;
        private static string fbbootpath = null;
        private static string fbintbootpath = null;
        private static string fbvendorpath = null;
        private static string fbcustomimgpath = null;
        private static string fbbatcrc = null;
        private static bool isstop = false;
        private static string qcstate = null;
        public Main()
        {
            InitializeComponent();
            InitTableColumns();
            DeviceMonitor.Start();//开始监听设备变化
            DeviceMonitor.DeviceChanged += DeviceMonitor_DeviceChanged;
            FastBoot.Initialize();//初始化fastboot
            Yyz.验证初始化("709", "ZTSOWH3CBUFCIVO7FFFG3XBLP22JDXTK", "8R6pipHyGdxK2SQMY7Mxn2X8cTSypHxz");
            ProgressManager.ProgressUpdated += OnQualcommProgress_Changed;//开始高通刷机数据变化事件
            _ = Yyz.开始心跳();
            Yyz.OnClosed += OnAceClosed;//用户被禁用
            AntdUI.Config.ShowInWindow = true;
            AntdUI.Message.MaxCount = 3; // 设置最大消息数量
        }

        private void OnAceClosed(object sender, EventArgs e)
        {
            try
            {
                cmd.Closeexe("adb");
                cmd.Closeexe("fastboot");
                cmd.Closeexe("scrcpy");
                cmd.Closeexe("fh_loader");
                Directory.Delete(MyPath.mainpath, true);
                Application.Exit();
                Environment.Exit(0);
            }
            catch 
            {
                Application.Exit();
                Environment.Exit(0);
            }
        }

        private void InitTableColumns()//初始化所有表格列表
        {
            tableapplist.Columns = new ColumnCollection()
            {
                new ColumnCheck("Selected")
                {
                    Fixed = true
                },
                new Column("Name","名称")
                {
                    Width = "90"
                },
                new Column("App","应用")
                {
                    Width = "180"
                },
            };
            tableapplist.Binding(applists);//绑定数据模型
            tablefb.Columns = new ColumnCollection()
            {
                new ColumnCheck("Selected")
                {
                    Fixed = true
                },
                new Column("Partition","分区名")
                {
                    Width = "140",
                    Align = ColumnAlign.Center,
                },
                new Column("File","文件")
                {
                    Width = "180",
                    Align = ColumnAlign.Center,
                },
                new Column("FilePath","文件路径")
                {
                    Width = "800"
                },
            };
            tablefb.Binding(fastbootPartitions);
            tableqc.Columns = new ColumnCollection()
            {
                new ColumnCheck("Selected")
                {
                    Fixed = true
                },
                new Column("Lun","Lun")
                {
                    Width = "40",
                    Align = ColumnAlign.Center,
                },
                new Column("Partition","分区名")
                {
                    Width = "140",
                    Align = ColumnAlign.Center,
                },
                new Column("Size","大小")
                {
                    Width = "100",
                    Align = ColumnAlign.Center,
                },
                new Column("File","文件")
                {
                    Width = "180",
                    Align = ColumnAlign.Center,
                },
                new Column("FilePath","文件路径")
                {
                    Width = "800"
                },
            };
            tableqc.Binding(QcPartitions);

        }
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                AntdUI.Spin.open(this, new AntdUI.Spin.Config()
                {
                    Back = Color.FromArgb(255, 255, 255),
                    Color = Color.FromArgb(0, 0, 0),//转圈颜色
                    Radius = 6,
                    Fore = Color.Black,//字体颜色
                    Font = new Font("Microsoft YaHei UI", 14f),//字体可以控制进度圈的大小

                }, (config) =>
                {
                    config.Text = "Loading";
                    labelUser.Text = AceData.Uid;
                    string system = Windowsinfo.GetWindowsVersion();
                    string systembit = Windowsinfo.Is64Bit() ? "64位" : "32位";
                    labelWindows.Text = $"系统：{system} {systembit}";
                    ADB.Restart();//重启ADB服务
                    Listdevice = DeviceInfo.GetDeviceDate();//监测设备
                    Updateselectdevicelist();//更新设备列表
                }, () =>
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, "欢迎使用 Wacke Box Tool", TType.Success)
                    {
                        AutoClose = 3,
                        Align = TAlignFrom.Top,
                    });
                });
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"加载失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 3,
                    Align = TAlignFrom.Top,
                });
            }

        }

        

        private async void OnFastBootProcess_Changed(object sender, ProcessChangedArgs e)
        {
            try
            {
                string processed = null;
                string total = null;
                long speedBps = -1;
                string speed = null;
                long nowMs = Stopwatch.ElapsedMilliseconds;
                long dt = nowMs - _lastTime;
                if (dt >= 100)
                {
                    long delta = e.Processedbytes - _lastWritten;
                    speedBps = delta * 1000 / dt;
                    await Task.Run(() =>
                    {
                        speed = BytesHelper.FormatBytes(speedBps) + "/s";
                    });
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = e.Processedbytes;
                }
                if (progress.InvokeRequired)
                {
                    progress.Invoke(new Action(() =>
                    {
                        progress.Value = e.Processedvalue;
                    }));
                }
                else
                {
                    progress.Value = e.Processedvalue;
                }
                await Task.Run(() =>
                {
                    processed = BytesHelper.FormatBytes(e.Processedbytes);
                    total = BytesHelper.FormatBytes(e.Totalbytes);
                });
                string Text = e.IsWriting ? "Writing..." : $"Sending  {processed} / {total}";
                InfoText(Text);
                if (e.IsWriting)
                {
                    SpdText("");
                }
                else
                {
                    if (speedBps != -1)
                    {
                        SpdText($"{speed}");
                    }
                }
                
            }
            catch
            {
                
            }
        }
        private async void OnQualcommProgress_Changed(object sender, ProgressEventArgs e)
        {
            if (progress.InvokeRequired)
            {
                progress.Invoke(new Action(() =>
                {
                    progress.Value = e.Progress;
                }));
            }
            else
            {
                progress.Value = e.Progress;
            }
            long processed = e.Processed;
            long total = e.TotalSize;
            var now = Stopwatch.ElapsedMilliseconds;
            long dt = now - _lastTime;
            long speedBps = -1 ;
            string pro = null;
            string tot = null;
            if (dt >= 100)
            {
                
                long delta = processed - _lastWritten;
                Debug.WriteLine(dt + "     " + delta);
                speedBps = (delta * 1000 / dt);
                if (speedBps != -1)
                {
                    string speed = BytesHelper.FormatBytes(speedBps) + "/s";
                    SpdText($"{speed}");
                }
                _lastTime = now;
                _lastWritten = processed;
            }
            await Task.Run(() =>
            {
                pro = BytesHelper.FormatBytes(processed);
                tot = BytesHelper.FormatBytes(total);
            });
            InfoText($"{qcstate} {pro} / {tot}");

        }


        private void Allenable(bool enable)
        {
            //待完整
            buttonRefreshdevicedate.Enabled = enable;
            buttonRefreshdeviceinfo.Enabled = enable;
            buttonscrcpy.Enabled = enable;
            buttonadbhome.Enabled = enable;
            buttonadbreturn.Enabled = enable;
            buttonadbPower.Enabled = enable;
            dropdownadbreboot.Enabled = enable;
            segmentedapplistset.Enabled = enable;
            buttonuninstallapp.Enabled = enable;
            buttoninstallapp.Enabled = enable;
            buttondisableapp.Enabled = enable;
            buttonenableapp.Enabled = enable;
            buttonfbexecutelock.Enabled = enable;
            buttonfbexecuteunlock.Enabled = enable;
            buttongetfbdeviceinfo.Enabled = enable;
            dropdownsetslot.Enabled = enable;
            dropdownfbreboot.Enabled = enable;
            buttonfbbootboot.Enabled = enable;
            buttonfbflashboota.Enabled = enable;
            buttonfbflashbootb.Enabled = enable;
            buttonfbflashrec.Enabled = enable;
            buttonfbflashreca.Enabled = enable;
            buttonfbflashrecb.Enabled = enable;
            buttonfbflashcustompartition.Enabled = enable;
            buttonfbselectcustompath.Enabled = enable;
            buttonfbflashvendor.Enabled = enable;
            buttonfbselectvendorpath.Enabled = enable;
            buttonfbflashintboot.Enabled = enable;
            buttonfbselectintbootpath.Enabled = enable;
            buttonfbflashboot.Enabled = enable;
            buttonfbselectbootpath.Enabled = enable;
            buttonfbselectbat.Enabled = enable;
            checkboxfbflashuserdata.Enabled = enable;
            checkboxfbiscrc.Enabled = enable;
            checkboxfbisdm.Enabled = enable;
            checkboxfbislock.Enabled = enable;
            checkboxfbreboot.Enabled = enable;
            checkboxfbslota.Enabled = enable;
            buttonfbpartitioninfo.Enabled = enable;
            buttonfberase.Enabled = enable;
            buttonfbwrite.Enabled = enable;
            buttonqccheckrew.Enabled = enable;
            checkboxqcreboot.Enabled = enable;
            checkboxqcsaveuserdate.Enabled = enable;
            checkboxqcsavelun5.Enabled = enable;
            buttonqcerase.Enabled = enable;
            buttonqcflash.Enabled = enable;
            buttonqcread.Enabled = enable;
            buttonqcreadpartition.Enabled = enable;
            dropdownqcfunction.Enabled = enable;
            buttonmiusb.Enabled = enable;
            buttonremiusb.Enabled = enable;
            selectqcelf.Enabled = enable;
            buttonstopscrcpy.Enabled = enable;
            dropdownadbsystem1.Enabled = enable;
            dropdownadbsystem2.Enabled = enable;
            buttonselectapk.Enabled = enable;
            segmentedstorageType.Enabled = enable;
            checkboxpayloaddisablecheck.Enabled = enable;
            checkboxpayloadnomistakes.Enabled = enable;
            buttonpayloadexport.Enabled = enable;
            buttonpayloadreadinfo.Enabled = enable;
        }
        private void Updateselectdevicelist()
        {
            try
            {
                selectdevice.SelectedIndex = -1;
                selectdevice.Items.Clear();
                if (Listdevice.Count == 0)
                {
                    selectdevice.Text = "没有设备连接";
                }
                else
                {
                    foreach (var d in Listdevice)
                    {
                        selectdevice.Items.Add($"{d.DeviceType}    {d.DviceName}");
                    }
                    if (tabs.SelectedIndex == 0)
                    {
                        for (int i = 0; i < selectdevice.Items.Count; i++)
                        {
                            if (selectdevice.Items[i].ToString().Contains("ADB"))
                            {

                                selectdevice.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    else if (tabs.SelectedIndex == 1)
                    {
                        for (int i = 0; i < selectdevice.Items.Count; i++)
                        {
                            if (selectdevice.Items[i].ToString().Contains("FastBoot"))
                            {
                                selectdevice.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    else if (tabs.SelectedIndex == 2)
                    {
                        for (int i = 0; i < selectdevice.Items.Count; i++)
                        {
                            if (selectdevice.Items[i].ToString().Contains("9008"))
                            {
                                selectdevice.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    if (selectdevice.SelectedIndex == -1) // 如果没有匹配的设备，清空选择
                    {
                        selectdevice.Text = "请选择设备";
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }
        private void AddFormLog(string text)
        {
            if (inputlog.InvokeRequired)
            {
                // 如果当前线程不是UI线程，则使用Invoke
                inputlog.Invoke(new Action(() => inputlog.AppendText(text)));
            }
            else
            {
                // 如果当前线程已经是UI线程，直接更新
                inputlog.AppendText(text);
            }
        }
        private void InfoText(string text)
        {
            if (labelinfo.InvokeRequired)
            {
                // 如果当前线程不是UI线程，则使用Invoke
                labelinfo.Invoke(new Action(() => labelinfo.Text = text));
            }
            else
            {
                // 如果当前线程已经是UI线程，直接更新
                labelinfo.Text = text;
            }
        }
        private void SpdText(string text)
        {
            if (labelspd.InvokeRequired)
            {
                // 如果当前线程不是UI线程，则使用Invoke
                labelspd.Invoke(new Action(() => labelspd.Text = text));
            }
            else
            {
                // 如果当前线程已经是UI线程，直接更新
                labelspd.Text = text;
            }
        }
        private async void DeviceMonitor_DeviceChanged(object sender, DeviceChangeEventArgs e)
        {
            try
            {
                if (!isrun)
                {

                    isrun = true;
                    Listdevice = await DeviceInfo.GetDeviceDateAsync();
                    Updateselectdevicelist();
                    isrun = false;
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"刷新设备失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }

        }

        private async void buttonRefreshdevice_Click(object sender, EventArgs e)
        {
            buttonRefreshdeviceinfo.Loading = true;
            Allenable(false);
            try
            {

                Listdevice = await DeviceInfo.GetDeviceDateAsync();
                Updateselectdevicelist();
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"刷新设备信息失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            buttonRefreshdeviceinfo.Loading = false;
            Allenable(true);
        }

        private void tabs_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            Updateselectdevicelist();
        }

        private async void buttonRefreshdevicedate_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonRefreshdevicedate.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    List<string> deviceinfo = await ADB.GetDeviceInfoAsync(deviceid);
                    labeladb0.Text = deviceinfo[0];
                    labeladb1.Text = deviceinfo[1];
                    labeladb2.Text = deviceinfo[2];
                    labeladb3.Text = deviceinfo[3];
                    labeladb4.Text = deviceinfo[4];
                    labeladb5.Text = deviceinfo[5];
                    labeladb6.Text = deviceinfo[6];
                    labeladb7.Text = deviceinfo[7];
                    labeladb8.Text = deviceinfo[8];
                    labeladb9.Text = deviceinfo[9];
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"获取设备信息失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            buttonRefreshdevicedate.Loading = false;
            Allenable(true);
        }

        private async void buttonscrcpy_Click(object sender, EventArgs e)
        {
            buttonscrcpy.Loading = true;
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    bool issuccess = await ADB.StartScrcpy(Listdevice[selectdevice.SelectedIndex].DviceName);
                    if (!issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"启动投屏失败!!!", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"启动 scrcpy 失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            buttonscrcpy.Loading = false;
            Allenable(true);
        }

        private void selectdevice_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            string result = null;
            if (selectdevice.SelectedIndex >= 0)
            {
                if (tabs.SelectedIndex == 0)
                {
                    result = selectdevice.Items[selectdevice.SelectedIndex].ToString();
                    if (!result.Contains("ADB"))
                    {
                        Updateselectdevicelist();
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"请选择符合操作区域的设备", TType.Info)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else if (tabs.SelectedIndex == 1)
                {
                    result = selectdevice.Items[selectdevice.SelectedIndex].ToString();
                    if (!result.Contains("FastBoot"))
                    {
                        Updateselectdevicelist();
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"请选择符合操作区域的设备", TType.Info)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else if (tabs.SelectedIndex == 2)
                {
                    result = selectdevice.Items[selectdevice.SelectedIndex].ToString();
                    if (!result.Contains("9008"))
                    {
                        Updateselectdevicelist();
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"请选择符合操作区域的设备", TType.Info)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
            }
        }

        private async void buttonstopscrcpy_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonstopscrcpy.Loading = true;
            try
            {
                bool issuccess = await ADB.StopScrcpy();
                if (!issuccess)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"停止投屏失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"停止投屏失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            buttonstopscrcpy.Loading = false;
            Allenable(true);

        }

        private async void buttonadbreturn_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonadbreturn.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    bool issuccess = await ADB.ScreenReturn(Listdevice[selectdevice.SelectedIndex].DviceName);
                    if (!issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"返回失败!!!", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"返回失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttonadbreturn.Loading = false;
        }

        private async void buttonadbhome_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonadbhome.Loading = false;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    bool issuccess = await ADB.ScreenHome(Listdevice[selectdevice.SelectedIndex].DviceName);
                    if (!issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"返回失败!!!", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"返回失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttonadbhome.Loading = false;
        }

        private async void buttonadblockscreen_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonadbPower.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    bool issuccess = await ADB.ScreenPower(Listdevice[selectdevice.SelectedIndex].DviceName);
                    if (!issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"亮屏/息屏失败!!!", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"返回失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttonadbPower.Loading = false;
        }

        private async void dropdownadbreboot_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            Allenable(false);
            dropdownadbreboot.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    if (e.Value is string rebootType)
                    {
                        bool issuccess = false;
                        switch (rebootType)
                        {
                            case "Android":
                                issuccess = await ADB.RebootAndroid(Listdevice[selectdevice.SelectedIndex].DviceName);
                                break;
                            case "Recovery":
                                issuccess = await ADB.RebootRecovery(Listdevice[selectdevice.SelectedIndex].DviceName);
                                break;
                            case "FastBoot":
                                issuccess = await ADB.RebootFastBoot(Listdevice[selectdevice.SelectedIndex].DviceName);
                                break;
                            case "FastBootD":
                                issuccess = await ADB.RebootFastbootD(Listdevice[selectdevice.SelectedIndex].DviceName);
                                break;
                            case "EDL":
                                issuccess = await ADB.RebootEDL(Listdevice[selectdevice.SelectedIndex].DviceName);
                                break;
                            default:
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知的重启类型: {rebootType}", TType.Error)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                                break;
                        }
                        if (!issuccess)
                        {
                            AntdUI.Message.open(new AntdUI.Message.Config(this, $"重启失败!!!", TType.Error)
                            {
                                AutoClose = 1,
                                Align = TAlignFrom.Top,
                            });
                        }
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"重启失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            dropdownadbreboot.SelectedValue = -1; // 重置选择
            dropdownadbreboot.Loading = false;
        }

        private async void buttonGetapplist_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonGetapplist.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    List<string> lines = new List<string>();
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    selectsearchapp.Clear();
                    selectsearchapp.Items.Clear();
                    applists.Clear();
                    if (segmentedapplistset.SelectIndex == 0)
                    {
                        lines = await ADB.Getuserapplist(deviceid);
                        foreach (string line in lines)
                        {
                            string name = line.Split('=')[0];
                            string app = line.Split('=')[1];
                            applists.Add(new Applist { App = app, Name = name, Selected = false });
                            selectsearchapp.Items.Add(app);
                        }
                    }
                    else if (segmentedapplistset.SelectIndex == 1)
                    {
                        lines = await ADB.Getallapplist(deviceid);
                        foreach (string line in lines)
                        {
                            string name = line.Split('=')[0];
                            string app = line.Split('=')[1];
                            applists.Add(new Applist { App = app, Name = name, Selected = false });
                            selectsearchapp.Items.Add(app);
                        }
                    }
                    else
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }

                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    buttonGetapplist.Loading = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"获取失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttonGetapplist.Loading = false;
        }

        private async void buttondisableapp_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttondisableapp.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    foreach (var app in applists)
                    {
                        if (app.Selected)
                        {
                            bool issuccess = await ADB.Disableapp(deviceid, app.App);
                            if (!issuccess)
                            {
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"停用应用 {app.App} 失败", TType.Error)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                            }
                            else
                            {
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"停用应用 {app.App} 成功", TType.Success)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                            }
                        }
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"停用失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttondisableapp.Loading = false;
        }
        private async void buttonenableapp_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonenableapp.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    foreach (var app in applists)
                    {
                        if (app.Selected)
                        {
                            bool issuccess = await ADB.Enableapp(deviceid, app.App);
                            if (!issuccess)
                            {
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"停用应用 {app.App} 失败", TType.Error)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                            }
                            else
                            {
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"停用应用 {app.App} 成功", TType.Success)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                            }
                        }
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"启用失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttonenableapp.Loading = false;
        }
        private void buttonsearchapp_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectsearchapp.Text != "")
                {
                    string app = selectsearchapp.Text;
                    for (int i = 0; i < applists.Count; i++)
                    {
                        if (applists[i].App == app)
                        {

                            tableapplist.ScrollLine(i);
                            applists[i].Selected = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"搜索失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }
        private async void buttonuninstallapp_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonuninstallapp.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    foreach (var app in applists)
                    {
                        if (app.Selected)
                        {
                            bool issuccess = await ADB.Uninstall(deviceid, app.App);
                            if (!issuccess)
                            {
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"卸载应用 {app.App} 失败", TType.Error)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                            }
                            else
                            {
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"卸载应用 {app.App} 成功", TType.Success)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                            }
                        }
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"卸载失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttonuninstallapp.Loading = false;
        }

        private void buttonselectapk_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "APK 文件 (*.apk;*.apk.1)|*.apk;*.apk.1|所有文件 (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.Multiselect = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        adbapkpath = openFileDialog.FileName;
                        inputapkpath.Text = Path.GetFileName(adbapkpath);
                    }
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }
        private async void buttoninstallapp_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttoninstallapp.Loading = true;
            bool issuccess = false;
            try
            {
                if (selectdevice.SelectedIndex != -1 && adbapkpath != "")
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    issuccess = await ADB.Install(deviceid, adbapkpath);
                    if (issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"安装成功", TType.Success)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                    else
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"安装失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接或未选择APK文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"安装失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttoninstallapp.Loading = false;

        }
        private async void buttongetfbdeviceinfo_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttongetfbdeviceinfo.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    string a = await FastBoot.GetProduct(deviceid);
                    string b = await FastBoot.GetSlot(deviceid);
                    string c = await FastBoot.GetBootloader(deviceid);
                    string d = await FastBoot.GetMemory(deviceid);
                    labelfb1.Text = a;
                    labelfb2.Text = c;
                    labelfb3.Text = b;
                    labelfb4.Text = d;
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"获取设备信息失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttongetfbdeviceinfo.Loading = false;
        }

        private async void dropdownsetslot_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            Allenable(false);
            dropdownsetslot.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    bool issuccess = false;
                    if (e.Value is string slot)
                    {
                        switch (slot)
                        {
                            case "A":
                                issuccess = await FastBoot.SetSlot(deviceid, "a");
                                break;
                            case "B":
                                issuccess = await FastBoot.SetSlot(deviceid, "b");
                                break;
                            default:
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知的分区: {slot}", TType.Error)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                                break;
                        }

                    }
                    labelfb3.Text = await FastBoot.GetSlot(deviceid);
                    if (issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"设置分区成功", TType.Success)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                    else
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"设置分区失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"设置分区失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            dropdownsetslot.SelectedValue = -1; // 重置选择
            dropdownsetslot.Loading = false;
        }

        private async void buttonfbexecutelock_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonfbexecutelock.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1 && selectfbblcmd.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    string command = selectfbblcmd.Text;
                    bool issuccess = false;
                    issuccess = await FastBoot.SendCommed(deviceid, command);
                    if (issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"命令执行成功", TType.Success)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                    else
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"命令执行失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择命令", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"执行失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttonfbexecutelock.Loading = false;
        }

        private async void buttonfbexecuteunlock_Click(object sender, EventArgs e)
        {
            Allenable(false);
            buttonfbexecuteunlock.Loading = true;
            try
            {
                if (selectdevice.SelectedIndex != -1 && selectfbblcmd.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    string command = selectfbblcmd.Text;
                    bool issuccess = await FastBoot.SendCommed(deviceid, command);
                    if (issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"命令执行成功", TType.Success)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                    else
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"命令执行失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择命令", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"执行失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            buttonfbexecuteunlock.Loading = false;
        }


        private async void dropdownfbreboot_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            Allenable(false);
            dropdownfbreboot.Loading = true;
            try
            {
                bool issuccess = false;
                string deviceid = null;
                if (selectdevice.SelectedIndex != -1)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    if (e.Value is string type)
                    {
                        switch (type)
                        {
                            case "Android":
                                issuccess = await FastBoot.RebootSystem(deviceid);
                                break;
                            case "Recovery":
                                issuccess = await FastBoot.RebootRecovery(deviceid);
                                break;
                            case "FastBoot":
                                issuccess = await FastBoot.RebootFastboot(deviceid);
                                break;
                            case "FastBootD":
                                issuccess = await FastBoot.RebootFastbootD(deviceid);
                                break;
                            case "EDL":
                                issuccess = await FastBoot.RebootEdl(deviceid);
                                break;
                            case "EDL(Lenovo)":
                                issuccess = await FastBoot.RebootEdlLenovo(deviceid);
                                break;
                            default:
                                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误", TType.Error)
                                {
                                    AutoClose = 1,
                                    Align = TAlignFrom.Top,
                                });
                                break;
                        }
                    }
                    if (issuccess)
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"重启成功", TType.Success)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                    else
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"重启失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"重启失败{ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
            dropdownfbreboot.SelectedValue = -1;
            dropdownfbreboot.Loading = false;
        }

        private void buttonfbselectrec_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "img 文件 (*.img)|*.img|所有文件 (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.Multiselect = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        fbrecpath = openFileDialog.FileName;
                        inputfbrecimg.Text = Path.GetFileName(fbrecpath);
                    }
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private async void buttonfbflashrec_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbrecpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbrecimg.Text}==>Recovery ......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, "recovery", fbrecpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private void inputlog_TextChanged(object sender, EventArgs e)
        {
            inputlog.ScrollToEnd();
        }

        private async void buttonfbflashreca_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbrecpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbrecimg.Text}==>Recovery_A......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, "recovery_a", fbrecpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonfbflashrecb_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbrecpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbrecimg.Text}==>Recovery_B......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, "recovery_b", fbrecpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonfbbootboot_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbrecpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:启动分区{inputfbrecimg.Text}==>Boot......");
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        progress.Value = 0;
                        issuccess = await FastBoot.Boot(deviceid, fbrecpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonfbflashboota_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbrecpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbrecimg.Text}==>Boot_A......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, "boot_a", fbrecpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonfbflashbootb_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbrecpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbrecimg.Text}==>Boot_B......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, "boot_b", fbrecpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private void buttonfbselectbootpath_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "img 文件 (*.img)|*.img|所有文件 (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.Multiselect = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        fbbootpath = openFileDialog.FileName;
                        inputfbpathboot.Text = Path.GetFileName(fbbootpath);
                    }
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private void buttonfbselectintbootpath_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "img 文件 (*.img)|*.img|所有文件 (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.Multiselect = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        fbintbootpath = openFileDialog.FileName;
                        inputfbpathintboot.Text = Path.GetFileName(fbintbootpath);
                    }
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private void buttonfbselectvendorpath_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "img 文件 (*.img)|*.img|所有文件 (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.Multiselect = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        fbvendorpath = openFileDialog.FileName;
                        inputfbpathwendor.Text = Path.GetFileName(fbvendorpath);
                    }
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private void buttonfbselectcustompath_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "img 文件 (*.img)|*.img|所有文件 (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.Multiselect = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        fbcustomimgpath = openFileDialog.FileName;
                        inputfbpathcustomimg.Text = Path.GetFileName(fbcustomimgpath);
                    }
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private async void buttonfbflashboot_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbbootpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbpathboot.Text}==>Boot......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, "boot", fbbootpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonfbflashintboot_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbintbootpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbpathintboot.Text}==>Int_Boot......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, "int_boot", fbrecpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonfbflashvendor_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbvendorpath) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbpathwendor.Text}==>Vendor......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, "vendor", fbrecpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonfbflashcustompartition_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                string deviceid = null;
                bool issuccess = false;
                if (selectdevice.SelectedIndex != -1 && string.IsNullOrEmpty(fbcustomimgpath) != true && string.IsNullOrEmpty(inputfbcustompartition.Text) != true)
                {
                    deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl == "已解锁")
                    {
                        AddFormLog($"FastBoot:写入分区{inputfbpathcustomimg.Text}==>{inputfbcustompartition.Text}......");
                        Stopwatch.Restart();
                        progress.Value = 0;
                        _lastWritten = 0;
                        _lastTime = Stopwatch.ElapsedMilliseconds;
                        FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                        issuccess = await FastBoot.Flash(deviceid, inputfbcustompartition.Text.Trim(), fbcustomimgpath);
                        FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                        Stopwatch.Stop();
                        labelinfo.Text = "";
                        labelspd.Text = "";
                        if (issuccess)
                        {
                            AddFormLog($"OK\n\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n\n");
                        }
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)");
                    }
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有选择文件", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private void buttonfbselectbat_Click(object sender, EventArgs e)
        {
            try
            {
                checkboxfbflashuserdata.Checked = false;
                checkboxfbiscrc.Checked = false;
                checkboxfbisdm.Checked = false;
                checkboxfbislock.Checked = false;
                checkboxfbreboot.Checked = false;
                checkboxfbslota.Checked = false;
                fbbatcrc = null;
                fastbootPartitions.Clear();
                selectfbsearch.Clear();
                selectfbsearch.Items.Clear();
                inputfbbat.Text = null;
                using (OpenFileDialog fileDialog = new OpenFileDialog())
                {
                    fileDialog.Title = "选择Fastboot批处理文件";
                    fileDialog.Filter = "BAT文件|*.bat";
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        inputfbbat.Text = fileDialog.FileName;
                    }
                    else
                    {

                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"用户取消选择", TType.Warn)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                if (!string.IsNullOrEmpty(inputfbbat.Text))
                {
                    string batpath = inputfbbat.Text;
                    string original = File.ReadAllText(batpath, Encoding.Default);

                    string replaced = original
                            .Replace(((char)10).ToString(), "\r\n")
                            .Replace(@"/", @"\");


                    List<string> lines = new List<string>(replaced.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                    Debug.WriteLine(lines);
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed)) continue;
                        if (!trimmed.StartsWith("fastboot")) continue;
                        if (trimmed.StartsWith("fastboot %* getvar crc"))
                        {
                            fbbatcrc = StringHelper.GetTextBetween(trimmed, "findstr \\r \\c:\"^", "\"").Trim();
                            checkboxfbiscrc.Checked = true;
                        }
                        if (trimmed.StartsWith("fastboot %* set_active a"))
                        {
                            checkboxfbslota.Checked = true;
                        }
                        if (trimmed.StartsWith("fastboot %* oem lock"))
                        {
                            checkboxfbislock.Checked = true;
                        }
                        if (trimmed.StartsWith("fastboot %* reboot"))
                        {
                            checkboxfbreboot.Checked = true;
                        }
                        if (trimmed.StartsWith("fastboot %* oem cdms"))
                        {
                            checkboxfbisdm.Checked = true;
                        }
                        if (trimmed.StartsWith("fastboot %* erase"))
                        {
                            string partition = StringHelper.GetTextBetween(trimmed, "fastboot %* erase", "|| ").Trim();
                            fastbootPartitions.Add(new FastbootPartition { Selected = true, Command = "erase", Partition = partition });
                        }
                        if (trimmed.StartsWith("fastboot %* flash"))
                        {
                            string partition = StringHelper.GetTextBetween(trimmed, "fastboot %* flash", "%~dp0images\\").Trim();
                            string filepath = Path.GetDirectoryName(batpath) + "\\" + StringHelper.GetTextBetween(trimmed, "%~dp0", "||").Trim();
                            selectfbsearch.Items.Add(partition);
                            fastbootPartitions.Add(new FastbootPartition { Selected = true, Command = "flash", Partition = partition, File = Path.GetFileName(filepath), FilePath = filepath });
                        }
                        if (trimmed.StartsWith("fastboot %* flash userdata"))
                        {
                            checkboxfbflashuserdata.Checked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private void tablefb_CellDoubleClick(object sender, TableClickEventArgs e)
        {
            if (e.RowIndex == 0) return;
            if (e.ColumnIndex == 0) return;
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    fastbootPartitions[e.RowIndex - 1].FilePath = fileDialog.FileName;
                    fastbootPartitions[e.RowIndex - 1].File = Path.GetFileName(fileDialog.FileName);
                    tablefb.Refresh();
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }


        }

        private void buttonfbsearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectfbsearch.Text != "")
                {
                    string partition = selectfbsearch.Text;
                    for (int i = 0; i < fastbootPartitions.Count; i++)
                    {
                        if (fastbootPartitions[i].Partition == partition)
                        {
                            tablefb.ScrollLine(i + 1);
                            tablefb.SelectedIndex = i + 1;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"搜索失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private async void buttonfbwrite_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex != -1 && fastbootPartitions.Count != 0)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    bool issuccess = false;
                    bool crcresult = true;
                    bool blresult = true;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (checkboxfbiscrc.Checked && string.IsNullOrEmpty(fbbatcrc) == false)
                    {
                        AddFormLog("CRC验证......");
                        string crc = await FastBoot.GetCRC(deviceid);
                        crcresult = crc.Contains(fbbatcrc);
                        string a = crcresult ? "OK" : "Error";
                        AddFormLog($"{a}\n");
                    }
                    if (bl != "已解锁")
                    {
                        DialogResult ressult = AntdUI.Modal.open(new AntdUI.Modal.Config(this, "严重警告", "当前BootLoader异常\n是否继续操作")
                        {
                            Icon = TType.Warn,
                            //内边距
                            Padding = new Size(24, 20),
                        });
                        if (ressult == DialogResult.No) blresult = false;
                    }
                    if (blresult && crcresult)
                    {
                        buttonstop.Enabled = true;
                        foreach (var partitiondata in fastbootPartitions)
                        {
                            if (isstop == false && partitiondata.Selected)
                            {
                                if (!checkboxfbflashuserdata.Checked && partitiondata.Partition == "userdata") continue;
                                string commd = string.IsNullOrEmpty(partitiondata.Command) ? "flash" : partitiondata.Command;
                                if (commd == "flash")
                                {
                                    AddFormLog($"写入分区{partitiondata.File}==>{partitiondata.Partition}......");
                                    progress.Value = 0;
                                    if (partitiondata.FilePath == "双击选择文件")
                                    {
                                        AddFormLog($"未选择文件\n");
                                        continue;
                                    }
                                    if (!File.Exists(partitiondata.FilePath))
                                    {
                                        AddFormLog($"文件路径异常\n");
                                        continue;
                                    }
                                    Stopwatch.Restart();
                                    _lastTime = Stopwatch.ElapsedMilliseconds;
                                    _lastWritten = 0;
                                    FastBoot.OnProcessChanged += OnFastBootProcess_Changed;
                                    issuccess = await FastBoot.Flash(deviceid, partitiondata.Partition, partitiondata.FilePath);
                                    FastBoot.OnProcessChanged -= OnFastBootProcess_Changed;
                                    Stopwatch.Stop();
                                    progress.Value = 1;
                                    labelspd.Text = "";
                                    if (issuccess)
                                    {
                                        AddFormLog($"OK\n");
                                    }
                                    else
                                    {
                                        AddFormLog($"Error\n");
                                    }
                                }
                                else
                                {
                                    AddFormLog($"擦除分区 {partitiondata.Partition}......");
                                    progress.Value = 0;
                                    issuccess = await FastBoot.Erase(deviceid, partitiondata.Partition);
                                    if (issuccess)
                                    {
                                        AddFormLog($"OK\n");
                                    }
                                    else
                                    {
                                        AddFormLog($"Error\n");
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        cts?.Dispose();
                        cts = new CancellationTokenSource();
                        token = cts.Token;
                        buttonstop.Enabled = false;
                        isstop = false;
                        if (checkboxfbisdm.Checked)
                        {
                            AddFormLog("禁用DM校验......");
                            string a = await FastBoot.DisableDmMTK(deviceid) ? "OK" : "Error";
                            AddFormLog($"{a}\n");
                        }
                        if (checkboxfbislock.Checked)
                        {
                            AddFormLog("锁定BootLoader......");
                            string a = await FastBoot.LockDevice(deviceid) ? "OK" : "Error";
                            AddFormLog($"{a}\n");
                        }
                        if (checkboxfbslota.Checked)
                        {
                            AddFormLog("设置启动分区......");
                            string a = await FastBoot.SetSlot(deviceid, "a") ? "OK" : "Error";
                            AddFormLog($"{a}\n");
                        }
                        if (checkboxfbreboot.Checked)
                        {
                            AddFormLog("重启设备......");
                            string a = await FastBoot.RebootSystem(deviceid) ? "OK" : "Error";
                            AddFormLog($"{a}\n");
                        }
                        AddFormLog($"刷机完成\n\n");
                        labelinfo.Text = "";
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常/CRC验证失败)\n\n");
                    }

                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备未连接/没有数据", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private void buttonstop_Click(object sender, EventArgs e)
        {
            if (stopint != 2)
            {
                stopint++;
            }
            if (stopint == 2)
            {
                DialogResult ressult = AntdUI.Modal.open(new AntdUI.Modal.Config(this, "严重警告", "是否停止当前操作")
                {
                    Icon = TType.Warn,
                    //内边距
                    Padding = new Size(24, 20),
                });
                if (ressult == DialogResult.OK)
                {
                    isstop = true;
                    cts.Cancel();
                    buttonstop.Enabled = false;
                }
                stopint = 0;
            }
        }

        private async void buttonfberase_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex != -1 && fastbootPartitions.Count != 0)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    bool issuccess = false;
                    bool blresult = true;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl != "已解锁")
                    {
                        DialogResult ressult = AntdUI.Modal.open(new AntdUI.Modal.Config(this, "严重警告", "当前BootLoader异常\n是否继续操作")
                        {
                            Icon = TType.Warn,
                            //内边距
                            Padding = new Size(24, 20),
                        });
                        if (ressult == DialogResult.No) blresult = false;
                    }
                    if (blresult)
                    {
                        buttonstop.Enabled = true;
                        foreach (var partitiondata in fastbootPartitions)
                        {
                            if (isstop == false && partitiondata.Selected)
                            {
                                AddFormLog($"擦除分区 {partitiondata.Partition}......");
                                progress.Value = 0;
                                issuccess = await FastBoot.Erase(deviceid, partitiondata.Partition);
                                if (issuccess)
                                {
                                    AddFormLog($"OK\n");
                                }
                                else
                                {
                                    AddFormLog($"Error\n");
                                }

                            }
                            else
                            {
                                continue;
                            }
                        }
                        cts?.Dispose();
                        cts = new CancellationTokenSource();
                        token = cts.Token;
                        buttonstop.Enabled = false;
                        isstop = false;
                        if (checkboxfbreboot.Checked)
                        {
                            AddFormLog("重启设备......");
                            string a = await FastBoot.RebootSystem(deviceid) ? "OK" : "Error";
                            AddFormLog($"{a}\n");
                        }
                        AddFormLog($"刷机完成\n\n");
                        labelinfo.Text = "";
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)\n\n");
                    }

                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备未连接/没有数据", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonfbpartitioninfo_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex != -1)
                {
                    string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                    bool blresult = true;
                    AddFormLog($"目标设备: {deviceid} \n");
                    string bl = await FastBoot.GetBootloader(deviceid);
                    AddFormLog($"当前BootLoader状态: {bl} \n");
                    if (bl != "已解锁")
                    {
                        DialogResult ressult = AntdUI.Modal.open(new AntdUI.Modal.Config(this, "严重警告", "当前BootLoader异常\n是否继续操作")
                        {
                            Icon = TType.Warn,
                            //内边距
                            Padding = new Size(24, 20),
                        });
                        if (ressult == DialogResult.No) blresult = false;
                    }
                    if (blresult)
                    {
                        checkboxfbflashuserdata.Checked = false;
                        checkboxfbiscrc.Checked = false;
                        checkboxfbisdm.Checked = false;
                        checkboxfbislock.Checked = false;
                        checkboxfbreboot.Checked = false;
                        checkboxfbslota.Checked = false;
                        fbbatcrc = null;
                        fastbootPartitions.Clear();
                        selectfbsearch.Clear();
                        selectfbsearch.Items.Clear();
                        AddFormLog($"读取分区表信息......");
                        List<string> partition = await FastBoot.GetPartitioninfo(deviceid);
                        if (partition.Count > 0)
                        {
                            foreach (string s in partition)
                            {
                                fastbootPartitions.Add(new FastbootPartition { Selected = false, Partition = s, Command = null, File = "双击选择文件", FilePath = "双击选择文件" });
                                selectfbsearch.Items.Add(s);
                            }
                            AddFormLog($"OK\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n");
                        }
                        if (checkboxfbreboot.Checked)
                        {
                            AddFormLog("重启设备......");
                            string a = await FastBoot.RebootSystem(deviceid) ? "OK" : "Error";
                            AddFormLog($"{a}\n");
                        }
                        AddFormLog($"读取分区信息完成\n\n");
                        labelinfo.Text = "";
                    }
                    else
                    {
                        AddFormLog($"结束操作！(原因:BootLoader异常)\n\n");
                    }

                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备未连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);

        }

        private async void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                await Yyz.用户登出(AceData.Token,"");
                cmd.Closeexe("adb");
                cmd.Closeexe("fastboot");
                cmd.Closeexe("scrcpy");
                cmd.Closeexe("fh_lader");
                Directory.Delete(MyPath.mainpath, true);
            }
            catch { }
        }

        private void buttonmiusb_Click(object sender, EventArgs e)
        {
            try
            {
                string result = cmd.Run(MyPath.miusb, "");
                if (result.Contains("操作成功完成。"))
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"执行成功", TType.Success)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"执行失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private void buttonremiusb_Click(object sender, EventArgs e)
        {
            try
            {
                string result = cmd.Run(MyPath.remiusb, "");
                if (result.Contains("操作成功完成。"))
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"执行成功", TType.Success)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
                else
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"执行失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private async void dropdownqcfunction_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex == -1)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    dropdownsetslot.SelectedValue = -1; // 重置选择
                    return;
                }
                if (segmentedstorageType.SelectIndex == -1 || selectqcelf.SelectedIndex == -1)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有选择闪存类型/没有选择引导方案", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    dropdownsetslot.SelectedValue = -1; // 重置选择
                    return;
                }
                //变量初始化
                string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                bool issuccess = false;
                string storage = segmentedstorageType.SelectIndex == 0 ? "ufs" : "emmc";
                List<string> modem = new List<string>();
                string elfpath = null;
                AddFormLog($"\n\n目标设备: {deviceid} \n");
                _flash = Flash.Instance;
                _flash.Initialize(deviceid, storage);
                AddFormLog($"连接到设备 {deviceid} ......");
                _flash.RegisterPort();
                AddFormLog($"OK\n");
                AddFormLog($"存储类型: {storage} \n");
                AddFormLog($"引导方案: {selectqcelf.Text} \n\n");
                AddFormLog($"尝试响应设备......");
                string modemtemp = await Task.Run(() =>
                {
                    return _flash.GetModem();
                });
                //对于sahara和firehose做出不同操作
                if (string.IsNullOrEmpty(modemtemp) == true)
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (modemtemp.Contains("Sahara"))
                {
                    AddFormLog($"Sahara\n");
                    modem = modemtemp.Split(' ').ToList();
                    if (modem[1] == "3")
                    {
                        AddFormLog($"    Version: 3 \n\n");
                    }
                    else
                    {
                        AddFormLog($"    Version: {modem[1]} \n");
                        AddFormLog($"    MsmId: {modem[2]} \n");
                        AddFormLog($"    OemId: {modem[3]} \n");
                        AddFormLog($"    ModelId: {modem[4]} \n");
                        AddFormLog($"    Serial: {modem[5]} \n");
                        AddFormLog($"    PkHash: {modem[6]} \n\n");
                    }
                    if (inputqcelfpath.Text == "" || inputqcelfpath.Text == "")
                    {
                        _flash.Close();
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"请选择引导文件后重试", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    if (inputqcelfpath.Text == "自动匹配引导")
                    {
                        elfpath = QCHelper.Getelfpath(selectqcelf.Text);
                    }
                    else
                    {
                        elfpath = inputqcelfpath.Text;
                    }
                    AddFormLog($"发送引导文件......");
                    Stopwatch.Restart();
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = 0;
                    issuccess = await Task.Run(async () =>
                    {
                        return await _flash.Sahara(elfpath);
                    });
                    progress.Value = 1;
                    Stopwatch.Stop();
                    InfoText("");
                    SpdText("");
                    if (!issuccess)
                    {
                        _flash.Close();
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"引导文件失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    AddFormLog($"OK\n\n");
                }
                else if (modemtemp.Contains("Firehose"))
                {
                    AddFormLog($"Firehose\n\n");
                }
                else
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //开始刷机流程
                AddFormLog($"\n");
                if (!await ConfigCom())
                {
                    _flash.Close();
                    Allenable(true);
                    return;
                }
                if (e.Value is string type)
                {
                    switch (type)
                    {
                        case "重启至EDL":
                            issuccess = true;
                            AddFormLog($"\n重启设备......");
                            try
                            {
                                await Task.Run(() =>
                                {
                                    _flash.Reset();
                                });
                            }
                            catch
                            {
                                issuccess = false;
                            }
                            if (issuccess)
                            {
                                AddFormLog($"OK\n");
                            }
                            else
                            {
                                AddFormLog($"Error\n");
                            }
                            break;
                        case "重启至System":
                            issuccess = true;
                            AddFormLog($"\n重启设备......");
                            try
                            {
                                await Task.Run(() =>
                                {
                                    _flash.Reboot();
                                });
                            }
                            catch
                            {
                                issuccess = false;
                            }
                            if (issuccess)
                            {
                                AddFormLog($"OK\n");
                            }
                            else
                            {
                                AddFormLog($"Error\n");
                            }
                            break;
                        case "一键ACC(XiaoMi)":
                            
                            break;
                        default:
                            AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误", TType.Error)
                            {
                                AutoClose = 1,
                                Align = TAlignFrom.Top,
                            });
                            break;
                    }
                }
                AddFormLog($"\n操作完成\n");

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误{ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            _flash.Close();
            Allenable(true);
            dropdownqcfunction.SelectedValue = -1; // 重置选择
        }

        private async void buttonqccheckrew_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                List<string> rawpath = new List<string>();
                string images = null;
                string elfpath = null;
                QcPartitions.Clear();
                pachxmlpath = null;
                inputqcrawpath.Text = "";
                selectqcpartitions.Clear();
                selectqcpartitions.Items.Clear();

                using (OpenFileDialog fileDialog = new OpenFileDialog())
                {
                    fileDialog.Multiselect = true;
                    fileDialog.Filter = "XML文件|rawprogram*.xml";
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        rawpath = fileDialog.FileNames.ToList();
                    }
                    else
                    {

                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"用户取消选择", TType.Warn)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }
                if (rawpath.Count != 0)
                {
                    inputqcrawpath.Text = "";
                    for (int i = 0; i < rawpath.Count; i++)
                    {
                        string path = rawpath[i];
                        if (i == rawpath.Count - 1)
                        {
                            inputqcrawpath.Text = inputqcrawpath.Text + Path.GetFileName(path);
                        }
                        else
                        {
                            inputqcrawpath.Text = inputqcrawpath.Text + Path.GetFileName(path) + ",";
                        }
                    }
                    images = Path.GetDirectoryName(rawpath[0]);
                    elfpath = QCHelper.FindProgrammerFiles(images);
                    AddFormLog($"镜像文件夹: {images} \n");
                    if (string.IsNullOrEmpty(elfpath) == false && inputqcelfpath.Text != "自动匹配引导")
                    {
                        inputqcelfpath.Text = elfpath;
                        AddFormLog($"识别到引导: {elfpath} \n");
                    }
                    FileInfo[] f = new DirectoryInfo(images).GetFiles("*.xml", SearchOption.TopDirectoryOnly);
                    pachxmlpath = f.Where(file => System.Text.RegularExpressions.Regex.IsMatch
                    (
                        file.Name, @"^patch[0-9]+\.xml$",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    )).ToArray();
                    if (pachxmlpath.Count() > 0)
                    {
                        foreach (FileInfo file in pachxmlpath)
                        {
                            AddFormLog($"识别到补丁: {file.Name} \n");
                        }
                    }
                    foreach (string path in rawpath)
                    {
                        AddFormLog($"解析 XML: {Path.GetFileName(path)}......");
                        //预留
                        List<Qcxmlinfo> lines = await QCHelper.GetQcxmlinfosAsync(path);
                        if (lines.Count == 0)
                        {
                            AddFormLog($"Error\n");
                            continue;
                        }
                        foreach (var line in lines)
                        {
                            QcPartitions.Add(new Qcxmlinfo
                            {
                                Selected = line.Selected,
                                Partition = line.Partition,
                                Size = line.Size,
                                File = line.File,
                                FilePath = line.FilePath,
                                StarSector = line.StarSector,
                                Sector = line.Sector,
                                FileSectorOffset = line.FileSectorOffset,
                                Lun = line.Lun
                            });
                            selectqcpartitions.Items.Add(line.Partition);

                        }
                        AddFormLog($"OK\n");
                    }
                    string storage = await QCHelper.StorageType(rawpath[0]);
                    if (storage != null)
                    {
                        if (storage == "EMMC")
                        {
                            segmentedstorageType.SelectIndex = 1;
                        }
                        if (storage == "UFS")
                        {
                            segmentedstorageType.SelectIndex = 0;
                        }
                    }
                    AddFormLog($"识别到存储类型: {storage}\n");
                    AddFormLog("完成解析\n\n");
                }
            }
            catch (Exception ex)
            {
                QcPartitions.Clear();
                pachxmlpath = null;
                inputqcrawpath.Text = "";
                selectqcpartitions.Clear();
                selectqcpartitions.Items.Clear();
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectqcpartitions.Text != "")
                {
                    string partition = selectqcpartitions.Text;
                    for (int i = 0; i < QcPartitions.Count; i++)
                    {
                        if (QcPartitions[i].Partition == partition)
                        {
                            tableqc.ScrollLine(i + 1);
                            tableqc.SelectedIndex = i + 1;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"搜索失败: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private void selectqcelf_Leave(object sender, EventArgs e)
        {
            bool iself = false;
            if (string.IsNullOrEmpty(selectqcelf.Text) == false)
            {
                foreach (var item in selectqcelf.Items)
                {
                    if (item.ToString() == selectqcelf.Text)
                    {
                        iself = true; break;
                    }
                }
            }
            if (iself == false)
            {
                selectqcelf.SelectedIndex = -1;
                selectqcelf.Text = null;
            }
        }

        private void inputqcelfpath_DoubleClick(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                if (QCHelper.Getelfpath(selectqcelf.Text) != null)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"不需要选择引导", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                inputqcelfpath.Text = "";
                using (OpenFileDialog fileDialog = new OpenFileDialog())
                {
                    fileDialog.Filter = "Qualcomm ELF/MBN/MELF|*.elf;*.mbn;*.melf|all|*.*";
                    fileDialog.Title = "选择引导程序";
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        inputqcelfpath.Text = fileDialog.FileName;
                    }
                    else
                    {

                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"用户取消选择", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误:{ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private void selectqcelf_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            if (QCHelper.Getelfpath(selectqcelf.Text) != null)
            {
                inputqcelfpath.Text = "自动匹配引导";
            }
            else
            {
                if (inputqcelfpath.Text == "自动匹配引导")
                {
                    inputqcelfpath.Text = "";
                }
            }
            if (selectqcelf.Text == "小米高通骁龙710芯片(新加密,免授权)")
            {
                segmentedstorageType.SelectIndex = 1; // EMMC
            }
            else
            {
                segmentedstorageType.SelectIndex = 0; // UFS
            }
        }

        private async Task<bool> ConfigCom()
        {
            AddFormLog($"配置设备......");

            string temp = await Task.Run(() =>
            {
                return _flash.ConfigureDDR();
            });


            if (temp == "failed")
            {
                AddFormLog($"Error\n\n");
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"配置设备失败", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
                return false;
            }
            else if (temp == "needsig")
            {
                if (QCHelper.IsBypassMi(selectqcelf.Text))
                {
                    AddFormLog($"需要签名\n");
                    AddFormLog($"签名设备......");
                    if (!_flash.BypassSendSig())
                    {
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"签名失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        return false ;
                    }
                    AddFormLog($"配置设备......");

                    string temp2 = await Task.Run(() =>
                    {
                        return _flash.ConfigureDDR();
                    });
                    if (temp2 != "success")
                    {
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"签名失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        return false;
                    }
                    AddFormLog($"OK\n");
                }
                else if (selectqcelf.Text == "小米高通骁龙芯片(新加密,需授权)")
                {
                    AddFormLog($"需要签名\n");//等待添加授权窗口
                    MyData.Blob = _flash.GetBlob();
                    if (string.IsNullOrEmpty(MyData.Blob))
                    {
                        AddFormLog($"获取设备密钥失败\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"获取Blob失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        return false;
                    }
                    var miAuthForm = new MiAuth();
                    var result = miAuthForm.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        AddFormLog($"用户取消签名\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"用户取消签名", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        return false;
                    }
                    AddFormLog($"签名设备......");
                    bool issuccess = true;
                    
                    issuccess = await Task.Run(() =>
                    {
                        try
                        {
                            return _flash.SendSignature(MyData.Sign);
                        }
                        catch
                        {
                            return false;
                        }

                    });
                    if (!issuccess)
                    {
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"签名失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        return false;
                    }
                    AddFormLog($"OK\n\n");
                    AddFormLog($"配置设备......");
                    string temp2 = await Task.Run(() =>
                    {
                        return _flash.ConfigureDDR();
                    });
                    if (temp2 != "success")
                    {
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"签名失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        return false;
                    }
                    AddFormLog($"OK\n");
                }
                else
                {
                    AddFormLog($"需要签名\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"不支持当前设备", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    return false;
                }
            }
            else if (temp == "success")
            {
                AddFormLog($"OK\n");
            }
            else
            {
                AddFormLog($"Error\n\n");
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"配置设备失败", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
                return false;
            }
            return true;
        }
        private async void buttonqcflash_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex == -1 || QcPartitions.Count == 0)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有分区可进行", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (segmentedstorageType.SelectIndex == -1 || selectqcelf.SelectedIndex == -1)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有选择闪存类型/没有选择引导方案", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //变量初始化
                string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                bool issuccess = false;
                string storage = segmentedstorageType.SelectIndex == 0 ? "ufs" : "emmc";
                List <string> modem = new List<string>();
                string elfpath = null;
                AddFormLog($"\n\n目标设备: {deviceid} \n");
                _flash = Flash.Instance;
                _flash.Initialize(deviceid, storage);
                AddFormLog($"连接到设备 {deviceid} ......");
                _flash.RegisterPort();
                AddFormLog($"OK\n");
                AddFormLog($"存储类型: {storage} \n");
                AddFormLog($"引导方案: {selectqcelf.Text} \n\n");
                AddFormLog($"尝试响应设备......");
                string modemtemp = await Task.Run(() =>
                {
                    return _flash.GetModem();
                });
                //对于sahara和firehose做出不同操作
                if (string.IsNullOrEmpty(modemtemp) == true)
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (modemtemp.Contains("Sahara"))
                {
                    AddFormLog($"Sahara\n");
                    modem =  modemtemp.Split(' ').ToList();
                    if (modem[1] == "3")
                    {
                        AddFormLog($"    Version: 3 \n\n");
                    }
                    else
                    {
                        AddFormLog($"    Version: {modem[1]} \n");
                        AddFormLog($"    MsmId: {modem[2]} \n");
                        AddFormLog($"    OemId: {modem[3]} \n");
                        AddFormLog($"    ModelId: {modem[4]} \n");
                        AddFormLog($"    Serial: {modem[5]} \n");
                        AddFormLog($"    PkHash: {modem[6]} \n\n");
                    }
                    if (inputqcelfpath.Text == "" || inputqcelfpath.Text == "")
                    {
                        _flash.Close();
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"请选择引导文件后重试", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    if (inputqcelfpath.Text == "自动匹配引导")
                    {
                        elfpath = QCHelper.Getelfpath(selectqcelf.Text);
                    }
                    else
                    {
                        elfpath = inputqcelfpath.Text;
                    }
                    AddFormLog($"发送引导文件......");
                    Stopwatch.Restart();
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = 0;
                    issuccess = await Task.Run(async () => 
                    {
                        return await _flash.Sahara(elfpath);
                    });
                    progress.Value = 1;
                    Stopwatch.Stop();
                    InfoText("");
                    SpdText("");
                    if (!issuccess)
                    {
                        _flash.Close();
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"引导文件失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    AddFormLog($"OK\n\n");
                }
                else if (modemtemp.Contains("Firehose"))
                {
                    AddFormLog($"Firehose\n\n");
                }
                else
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //开始刷机流程
                AddFormLog($"\n");
                if (!await ConfigCom())
                {
                    _flash.Close();
                    Allenable(true);
                    return;
                }
                qcstate = "Writing ";
                buttonstop.Enabled = true;
                foreach (var qcpartition in QcPartitions)
                {
                    if (isstop) break;
                    if (!qcpartition.Selected) continue;
                    if (checkboxqcsaveuserdate.Checked == true && qcpartition.Partition.ToLower().Trim() == "userdata") continue;
                    if (checkboxqcsaveuserdate.Checked == true && qcpartition.Partition.ToLower().Trim() == "metadata") continue;
                    if (checkboxqcsavelun5.Checked == true && issavepartition(qcpartition)) continue;
                    AddFormLog($"写入分区{qcpartition.File}==>{qcpartition.Partition}......");
                    if (qcpartition.File == "空文件")
                    {
                        AddFormLog($"未选择文件\n");
                        continue;
                    }
                    if (File.Exists(qcpartition.FilePath) == false)
                    {
                        AddFormLog($"文件路径异常\n");
                        continue;
                    } 
                    Stopwatch.Restart();
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = 0;
                    issuccess = true;
                    await Task.Run(() =>
                    {
                        try
                        {
                            _flash.WriteFile(qcpartition.FilePath, qcpartition.FileSectorOffset, qcpartition.StarSector, qcpartition.Sector, qcpartition.Lun, qcpartition.Partition, token);
                        }
                        catch
                        {
                            issuccess = false;
                        }
                    });
                    progress.Value = 1;
                    Stopwatch.Stop();
                    
                    if (issuccess)
                    {
                        AddFormLog($"OK\n");
                    }
                    else
                    {
                        AddFormLog($"Error\n");
                    }
                }
                cts?.Dispose();
                cts = new CancellationTokenSource();
                token = cts.Token;
                isstop = false;
                buttonstop.Enabled = false;
                InfoText("");
                SpdText("");
                if (pachxmlpath != null && pachxmlpath.Length > 0 )
                {
                    AddFormLog($"\n写入补丁\n");
                    foreach (FileInfo file in pachxmlpath)
                    {
                        issuccess = true;
                        AddFormLog($"    {file.Name} ......");
                        try
                        {
                            issuccess  = await Task.Run(() =>
                            {
                                return _flash.ApplyPatchesToDevice(file.FullName);
                            });
                        }
                        catch (Exception ex)
                        {
                            issuccess = false;
                        }
                        if (issuccess)
                        {
                            AddFormLog($"OK\n");
                        }
                        else
                        {
                            AddFormLog($"Error\n");
                        }

                    }
                }
                if (checkboxqcreboot.Checked)
                {
                    issuccess = true;
                    AddFormLog($"\n重启设备......");
                    try
                    {
                        await Task.Run(() =>
                        {
                            _flash.Reboot();
                        });
                    }
                    catch 
                    {
                        issuccess = false;
                    }
                    if (issuccess)
                    {
                        AddFormLog($"OK\n");
                    }
                    else
                    {
                        AddFormLog($"Error\n");
                    }
                }
                AddFormLog($"\n刷写分区完成\n");
                _flash.Close();
            }
            catch(Exception ex)
            {
                _flash.Close();
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonqcerase_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex == -1 || QcPartitions.Count == 0)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有分区可进行", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (segmentedstorageType.SelectIndex == -1 || selectqcelf.SelectedIndex == -1)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有选择闪存类型/没有选择引导方案", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //变量初始化
                string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                bool issuccess = false;
                string storage = segmentedstorageType.SelectIndex == 0 ? "ufs" : "emmc";
                List<string> modem = new List<string>();
                string elfpath = null;
                AddFormLog($"\n\n目标设备: {deviceid} \n");
                _flash = Flash.Instance;
                _flash.Initialize(deviceid, storage);
                AddFormLog($"连接到设备 {deviceid} ......");
                _flash.RegisterPort();
                AddFormLog($"OK\n");
                AddFormLog($"存储类型: {storage} \n");
                AddFormLog($"引导方案: {selectqcelf.Text} \n\n");
                AddFormLog($"尝试响应设备......");
                string modemtemp = await Task.Run(() =>
                {
                    return _flash.GetModem();
                });
                //对于sahara和firehose做出不同操作
                if (string.IsNullOrEmpty(modemtemp) == true)
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (modemtemp.Contains("Sahara"))
                {
                    AddFormLog($"Sahara\n");
                    modem = modemtemp.Split(' ').ToList();
                    if (modem[1] == "3")
                    {
                        AddFormLog($"    Version: 3 \n\n");
                    }
                    else
                    {
                        AddFormLog($"    Version: {modem[1]} \n");
                        AddFormLog($"    MsmId: {modem[2]} \n");
                        AddFormLog($"    OemId: {modem[3]} \n");
                        AddFormLog($"    ModelId: {modem[4]} \n");
                        AddFormLog($"    Serial: {modem[5]} \n");
                        AddFormLog($"    PkHash: {modem[6]} \n\n");
                    }
                    if (inputqcelfpath.Text == "" || inputqcelfpath.Text == "")
                    {
                        _flash.Close();
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"请选择引导文件后重试", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    if (inputqcelfpath.Text == "自动匹配引导")
                    {
                        elfpath = QCHelper.Getelfpath(selectqcelf.Text);
                    }
                    else
                    {
                        elfpath = inputqcelfpath.Text;
                    }
                    AddFormLog($"发送引导文件......");
                    Stopwatch.Restart();
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = 0;
                    issuccess = await Task.Run(async () =>
                    {
                        return await _flash.Sahara(elfpath);
                    });
                    progress.Value = 1;
                    Stopwatch.Stop();
                    InfoText("");
                    SpdText("");
                    if (!issuccess)
                    {
                        _flash.Close();
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"引导文件失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    AddFormLog($"OK\n\n");
                }
                else if (modemtemp.Contains("Firehose"))
                {
                    AddFormLog($"Firehose\n\n");
                }
                else
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //开始刷机流程
                AddFormLog($"\n");
                if (!await ConfigCom())
                {
                    _flash.Close();
                    Allenable(true);
                    return;
                }
                buttonstop.Enabled = true;
                foreach (var qcpartition in QcPartitions)
                {
                    if (isstop) break;
                    if (!qcpartition.Selected) continue;
                    if (checkboxqcsaveuserdate.Checked == true && qcpartition.Partition.ToLower().Trim() == "userdata") continue;
                    if (checkboxqcsaveuserdate.Checked == true && qcpartition.Partition.ToLower().Trim() == "metadata") continue;
                    if (checkboxqcsavelun5.Checked == true && issavepartition(qcpartition)) continue;
                    
                    AddFormLog($"擦除分区{qcpartition.Partition}......");
                    issuccess = true;
                    if (int.TryParse(qcpartition.Sector, out int sector) == false)
                    {
                        AddFormLog($"Error\n");
                        continue;
                    }
                    if (int.TryParse(qcpartition.Lun, out int lun) == false)
                    {
                        AddFormLog($"Error\n");
                        continue;
                    }
                    try
                    {
                        await Task.Run(() =>
                        {
                            _flash.Erase(qcpartition.StarSector,sector,lun);
                        });
                    }
                    catch
                    {
                        issuccess = false;
                    }
                    progress.Value = 1;
                    Stopwatch.Stop();
                    if (issuccess)
                    {
                        AddFormLog($"OK\n");
                    }
                    else
                    {
                        AddFormLog($"Error\n");
                    }
                }
                cts?.Dispose();
                cts = new CancellationTokenSource();
                token = cts.Token;
                isstop = false;
                buttonstop.Enabled = false;
                InfoText("");
                SpdText("");
                if (checkboxqcreboot.Checked)
                {
                    issuccess = true;
                    AddFormLog($"\n重启设备......");
                    try
                    {
                        await Task.Run(() =>
                        {
                            _flash.Reboot();
                        });
                    }
                    catch
                    {
                        issuccess = false;
                    }
                    if (issuccess)
                    {
                        AddFormLog($"OK\n");
                    }
                    else
                    {
                        AddFormLog($"Error\n");
                    }
                }
                AddFormLog($"\n擦除分区完成\n");
                _flash.Close();
            }
            catch (Exception ex)
            {
                _flash.Close();
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private async void buttonqcread_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex == -1 || QcPartitions.Count == 0)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接/没有分区可进行", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (segmentedstorageType.SelectIndex == -1 || selectqcelf.SelectedIndex == -1)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有选择闪存类型/没有选择引导方案", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //选择保存路径
                string selectedPath = null;
                using (var dlg = new FolderBrowserDialog())
                {
                    dlg.Description = "请选择要保存的文件夹";
                    dlg.ShowNewFolderButton = true;   // 允许新建文件夹
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        selectedPath = dlg.SelectedPath;
                    }
                    else
                    {
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"用户取消选择保存路径", TType.Info)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                }
                string backuppath = selectedPath + $"\\Backup{DateTime.Now:yyyyMMddHHmmss}";
                Directory.CreateDirectory(backuppath);
                //变量初始化
                string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                bool issuccess = false;
                string storage = segmentedstorageType.SelectIndex == 0 ? "ufs" : "emmc";
                List<string> modem = new List<string>();
                string elfpath = null;
                AddFormLog($"\n\n目标设备: {deviceid} \n");
                _flash = Flash.Instance;
                _flash.Initialize(deviceid, storage);
                AddFormLog($"连接到设备 {deviceid} ......");
                _flash.RegisterPort();
                AddFormLog($"OK\n");
                AddFormLog($"存储类型: {storage} \n");
                AddFormLog($"引导方案: {selectqcelf.Text} \n\n");
                AddFormLog($"尝试响应设备......");
                string modemtemp = await Task.Run(() =>
                {
                    return _flash.GetModem();
                });
                //对于sahara和firehose做出不同操作
                if (string.IsNullOrEmpty(modemtemp) == true)
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (modemtemp.Contains("Sahara"))
                {
                    AddFormLog($"Sahara\n");
                    modem = modemtemp.Split(' ').ToList();
                    if (modem[1] == "3")
                    {
                        AddFormLog($"    Version: 3 \n\n");
                    }
                    else
                    {
                        AddFormLog($"    Version: {modem[1]} \n");
                        AddFormLog($"    MsmId: {modem[2]} \n");
                        AddFormLog($"    OemId: {modem[3]} \n");
                        AddFormLog($"    ModelId: {modem[4]} \n");
                        AddFormLog($"    Serial: {modem[5]} \n");
                        AddFormLog($"    PkHash: {modem[6]} \n\n");
                    }
                    if (inputqcelfpath.Text == "" || inputqcelfpath.Text == "")
                    {
                        _flash.Close();
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"请选择引导文件后重试", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    if (inputqcelfpath.Text == "自动匹配引导")
                    {
                        elfpath = QCHelper.Getelfpath(selectqcelf.Text);
                    }
                    else
                    {
                        elfpath = inputqcelfpath.Text;
                    }
                    AddFormLog($"发送引导文件......");
                    Stopwatch.Restart();
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = 0;
                    issuccess = await Task.Run(async () =>
                    {
                        return await _flash.Sahara(elfpath);
                    });
                    progress.Value = 1;
                    Stopwatch.Stop();
                    InfoText("");
                    SpdText("");
                    if (!issuccess)
                    {
                        _flash.Close();
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"引导文件失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    AddFormLog($"OK\n\n");
                }
                else if (modemtemp.Contains("Firehose"))
                {
                    AddFormLog($"Firehose\n\n");
                }
                else
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //开始刷机流程
                AddFormLog($"\n");
                if (!await ConfigCom())
                {
                    _flash.Close();
                    Allenable(true);
                    return;
                }
                qcstate = "Reading ";
                buttonstop.Enabled = true;
                foreach (var qcpartition in QcPartitions)
                {
                    if (isstop) break;
                    if (!qcpartition.Selected) continue;
                    if (qcpartition.Partition.ToLower().Trim() == "userdata")
                    {
                        DialogResult ressult = AntdUI.Modal.open(new AntdUI.Modal.Config(this, "温馨提示", "当前读取分区为用户数据分区\n是否继续读取")
                        {
                            Icon = TType.Warn,
                            //内边距
                            Padding = new Size(24, 20),
                        });
                        if (ressult != DialogResult.OK)
                        {
                            continue;
                        }
                    }
                    AddFormLog($"读取{qcpartition.Partition}==>{qcpartition.Partition}_{qcpartition.Lun}.img......");
                    if (int.TryParse(qcpartition.Sector, out int sector) == false)
                    {
                        AddFormLog($"Error\n");
                        continue;
                    }
                    if (int.TryParse(qcpartition.Lun, out int lun) == false)
                    {
                        AddFormLog($"Error\n");
                        continue;
                    }
                    Stopwatch.Restart();
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = 0;
                    issuccess = true;
                    string outputFilePath = Path.Combine(backuppath, $"{qcpartition.Partition}_{qcpartition.Lun}.img");
                    await Task.Run(() =>
                    {
                        try
                        {
                           _flash.Read(qcpartition.StarSector, sector, lun, outputFilePath);
                        }
                        catch
                        {
                            issuccess = false;
                        }
                    });
                    progress.Value = 1;
                    Stopwatch.Stop();
                    InfoText("");
                    SpdText("");
                    if (issuccess)
                    {
                        AddFormLog($"OK\n");
                    }
                    else
                    {
                        AddFormLog($"Error\n");
                    }
                }
                cts?.Dispose();
                cts = new CancellationTokenSource();
                token = cts.Token;
                isstop = false;
                buttonstop.Enabled = false;
                
                AddFormLog($"\n生成XML......");
                List<Qcxmlinfo> list = new List<Qcxmlinfo>();
                foreach (var item in QcPartitions)
                {
                    string filename = item.Selected ? $"{item.Partition}_{item.Lun}.img" : "";
                    list.Add(new Qcxmlinfo
                    {
                        Lun = item.Lun,
                        Partition = item.Partition,
                        FileSectorOffset = item.FileSectorOffset,
                        Sector = item.Sector,
                        StarSector = item.StarSector,
                        File = filename,
                    }); 
                }
                issuccess = await QCHelper.BuildRawXml(list, storage, backuppath);
                if (issuccess)
                {
                    AddFormLog($"OK\n");
                }
                else
                {
                    AddFormLog($"Error\n");
                }
                InfoText("");
                SpdText("");
                
                if (checkboxqcreboot.Checked)
                {
                    issuccess = true;
                    AddFormLog($"\n重启设备......");
                    try
                    {
                        await Task.Run(() =>
                        {
                            _flash.Reboot();
                        });
                    }
                    catch
                    {
                        issuccess = false;
                    }
                    if (issuccess)
                    {
                        AddFormLog($"OK\n");
                    }
                    else
                    {
                        AddFormLog($"Error\n");
                    }
                }
                AddFormLog($"\n读取分区完成\n");
                _flash.Close();
            }
            catch (Exception ex)
            {
                _flash.Close();
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private void tableqc_CellDoubleClick(object sender, TableClickEventArgs e)
        {
            if (e.RowIndex == 0) return;
            if (e.ColumnIndex == 0) return;
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    QcPartitions[e.RowIndex - 1].FilePath = fileDialog.FileName;
                    QcPartitions[e.RowIndex - 1].File = Path.GetFileName(fileDialog.FileName);
                    tableqc.Refresh();
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
        }

        private async void buttonqcreadpartition_Click(object sender, EventArgs e)
        {
            Allenable(false);
            try
            {
                if (selectdevice.SelectedIndex == -1)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有设备连接", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (segmentedstorageType.SelectIndex == -1 || selectqcelf.SelectedIndex == -1)
                {
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"没有选择闪存类型/没有选择引导方案", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //变量初始化
                string deviceid = Listdevice[selectdevice.SelectedIndex].DviceName;
                bool issuccess = false;
                string storage = segmentedstorageType.SelectIndex == 0 ? "ufs" : "emmc";
                List<string> modem = new List<string>();
                string elfpath = null;
                long sizeinone = storage == "ufs" ? 4096 : 512;
                AddFormLog($"\n\n目标设备: {deviceid} \n");
                _flash = Flash.Instance;
                _flash.Initialize(deviceid, storage);
                AddFormLog($"连接到设备 {deviceid} ......");
                _flash.RegisterPort();
                AddFormLog($"OK\n");
                AddFormLog($"存储类型: {storage} \n");
                AddFormLog($"引导方案: {selectqcelf.Text} \n\n");
                AddFormLog($"尝试响应设备......");
                string modemtemp = await Task.Run(() =>
                {
                    return _flash.GetModem();
                });
                //对于sahara和firehose做出不同操作
                if (string.IsNullOrEmpty(modemtemp) == true)
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                if (modemtemp.Contains("Sahara"))
                {
                    AddFormLog($"Sahara\n");
                    modem = modemtemp.Split(' ').ToList();
                    if (modem[1] == "3")
                    {
                        AddFormLog($"    Version: 3 \n\n");
                    }
                    else
                    {
                        AddFormLog($"    Version: {modem[1]} \n");
                        AddFormLog($"    MsmId: {modem[2]} \n");
                        AddFormLog($"    OemId: {modem[3]} \n");
                        AddFormLog($"    ModelId: {modem[4]} \n");
                        AddFormLog($"    Serial: {modem[5]} \n");
                        AddFormLog($"    PkHash: {modem[6]} \n\n");
                    }
                    if (inputqcelfpath.Text == "" || inputqcelfpath.Text == "")
                    {
                        _flash.Close();
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"请选择引导文件后重试", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    if (inputqcelfpath.Text == "自动匹配引导")
                    {
                        elfpath = QCHelper.Getelfpath(selectqcelf.Text);
                    }
                    else
                    {
                        elfpath = inputqcelfpath.Text;
                    }
                    AddFormLog($"发送引导文件......");
                    Stopwatch.Restart();
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = 0;
                    issuccess = await Task.Run(async () =>
                    {
                        return await _flash.Sahara(elfpath);
                    });
                    progress.Value = 1;
                    Stopwatch.Stop();
                    InfoText("");
                    SpdText("");
                    if (!issuccess)
                    {
                        _flash.Close();
                        AddFormLog($"Error\n\n");
                        AntdUI.Message.open(new AntdUI.Message.Config(this, $"引导文件失败", TType.Error)
                        {
                            AutoClose = 1,
                            Align = TAlignFrom.Top,
                        });
                        Allenable(true);
                        return;
                    }
                    AddFormLog($"OK\n\n");
                }
                else if (modemtemp.Contains("Firehose"))
                {
                    AddFormLog($"Firehose\n\n");
                }
                else
                {
                    _flash.Close();
                    AddFormLog($"Error\n");
                    AntdUI.Message.open(new AntdUI.Message.Config(this, $"设备响应失败", TType.Error)
                    {
                        AutoClose = 1,
                        Align = TAlignFrom.Top,
                    });
                    Allenable(true);
                    return;
                }
                //开始刷机流程
                AddFormLog($"\n");
                if (!await ConfigCom())
                {
                    _flash.Close();
                    Allenable(true);
                    return;
                }
                QcPartitions.Clear();
                selectqcpartitions.Clear();
                AddFormLog($"\n");
                qcstate = "Reading ";
                int jici = storage == "ufs" ? 9 : 1 ;
                for(int i = 0; i < jici; i++)
                {
                    int sector = storage == "ufs" ? 6 : 34;
                    string stringlun = i.ToString();
                    int lun = i;
                    string output = Path.Combine(MyPath.temppath, $"gpt_main_{stringlun}");
                    
                    Stopwatch.Restart();
                    _lastTime = Stopwatch.ElapsedMilliseconds;
                    _lastWritten = 0;
                    issuccess = true;
                    await Task.Run(() =>
                    {
                        try
                        {
                            _flash.Read("0", sector, lun, output);
                        }
                        catch
                        {
                            issuccess = false;
                        }
                    });
                    progress.Value = 1;
                    Stopwatch.Stop();
                    InfoText("");
                    SpdText("");
                    string totalsize = null;
                    long filesize = new FileInfo(output).Length;
                    if (filesize == 0) break;
                    AddFormLog($"读取分区表LUN{stringlun}......");
                    GuidPartitionTable gpt = null;
                    try
                    {
                        await Task.Run(() =>
                        {
                            gpt = gptmainReader.FromPath(output);
                        });
                    }
                    catch
                    {
                        AddFormLog($"Error\n");
                        continue;
                    }
                    if (gpt.Partitions.Count == 0) continue;
                    foreach (var partition in gpt.Partitions)
                    {
                        if (partition.Guid.ToString() == "00000000-0000-0000-0000-000000000000") continue;
                        await Task.Run(() =>
                        {
                            if (partition.Name == "last_parti" || partition.Name == "userdata")
                            {
                                totalsize = BytesHelper.FormatBytes((long)partition.LastLba * sizeinone);
                            }
                            string size = BytesHelper.FormatBytes((long)(partition.LastLba - partition.FirstLba + 1) * sizeinone);
                            selectqcpartitions.Items.Add(partition.Name);
                            QcPartitions.Add(new Qcxmlinfo
                            {
                                Selected = false,
                                Partition = partition.Name,
                                FileSectorOffset = "0",
                                StarSector = partition.FirstLba.ToString(),
                                Sector = (partition.LastLba - partition.FirstLba + 1).ToString(),
                                Lun = stringlun,
                                Size = size,
                                File = "空文件",
                                FilePath = "双击选择文件"
                            });
                        });
                    }
                    if (string.IsNullOrEmpty(totalsize))
                    {
                        AddFormLog($"OK\n");
                    }
                    else
                    {
                        AddFormLog($"{totalsize}\n");
                    }
                    tableqc.Refresh();
                    selectqcpartitions.Refresh();
                }
                InfoText("");
                SpdText("");
                try
                {
                    await Task.Run(() =>
                    {
                        foreach (var file in Directory.GetFiles(MyPath.temppath, "*", SearchOption.AllDirectories))
                        {
                            File.Delete(file);
                        }
                    });
                }
                catch { }
                if (checkboxqcreboot.Checked)
                {
                    issuccess = true;
                    AddFormLog($"\n重启设备......");
                    try
                    {
                        await Task.Run(() =>
                        {
                            _flash.Reboot();
                        });
                    }
                    catch
                    {
                        issuccess = false;
                    }
                    if (issuccess)
                    {
                        AddFormLog($"OK\n");
                    }
                    else
                    {
                        AddFormLog($"Error\n");
                    }
                }
                AddFormLog($"\n读取分区表完成\n");
                _flash.Close();
            }
            catch (Exception ex)
            {
                _flash.Close();
                AntdUI.Message.open(new AntdUI.Message.Config(this, $"未知错误: {ex.Message}", TType.Error)
                {
                    AutoClose = 1,
                    Align = TAlignFrom.Top,
                });
            }
            Allenable(true);
        }

        private bool issavepartition(Qcxmlinfo qcxmlinfo)
        {
            if (qcxmlinfo.Lun != "5") return false;
            if (qcxmlinfo.Partition == "last_parti" || qcxmlinfo.Partition == "BackupGPT" || qcxmlinfo.Partition == "PrimaryGPT") return false;
            return true;
        }

        
    }
}

