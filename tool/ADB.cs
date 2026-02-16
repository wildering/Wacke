using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WackeClient.tool
{
    public static class ADB
    {
        //定义类型
        private static readonly string adb = $"C:\\Program Files\\WackeClient\\Tool\\System\\adb.exe";
        private static readonly string scrcpy = $"C:\\Program Files\\WackeClient\\Tool\\System\\scrcpy.exe";
        private static readonly Cmd cmd = new Cmd();
        public static List<string> Devices()
        {
            try
            {
                string result = null;
                result = cmd.Run(adb, "devices");
                if (result == null)
                {
                    return new List<string>();
                }
                else
                {
                    List<string> devicelist = new List<string>();
                    List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));

                    foreach (var line in lines)
                    {
                        bool isMatch = Regex.IsMatch(line, $@"\b{Regex.Escape("\tdevice")}\b", RegexOptions.IgnoreCase);
                        if (isMatch)
                        {
                            int index = line.IndexOf("\tdevice", StringComparison.OrdinalIgnoreCase);
                            string deviceId = line.Substring(0, index).Trim();
                            devicelist.Add(deviceId);
                        }
                    }
                    return devicelist;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task RestartAsync()
        {
            try
            {
                await Task.Run(() => cmd.Run(adb, "kill-server"));
                await Task.Run(() => cmd.Run(adb, "start-server"));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static void Restart()
        {
            try
            {
                cmd.Run(adb, "kill-server");
                cmd.Run(adb, "start-server");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<List<string>> GetDeviceInfoAsync(string deviceid)
        {
            string result = null;
            List<string> info = new List<string>();
            try
            {
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $" -s {deviceid} shell getprop ro.product.brand");//获取设备品牌
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        info.Add(Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    else
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell getprop ro.product.model");//获取设备型号
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        info.Add(Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    else
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell getprop ro.product.name");//获取设备代号
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        info.Add(Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    else
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell getprop ro.build.version.release");//获取Android版本
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        info.Add("Android " + Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    else
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell dumpsys battery");//获取电池信息
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        int batteryLevel = 0;
                        int batteryScale = 0;
                        foreach (var line in lines)
                        {
                            if (line.Contains("level:"))
                            {
                                batteryLevel = int.Parse(line.Split(':')[1].Trim());
                            }
                            else if (line.Contains("scale:"))
                            {
                                batteryScale = int.Parse(line.Split(':')[1].Trim());
                            }
                        }
                        string batteryInfo = $"{(batteryLevel / (double)batteryScale) * 100:F2} %";
                        info.Add(batteryInfo);
                    }
                    else
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell uname -r");//获取内核版本
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        info.Add(Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    else
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell wm size");//获取分辨率
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        string siza = Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "");
                        info.Add(siza.Split(':')[1].Trim());
                    }
                    else
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell getprop ro.secureboot.lockstate");//获取BL状态
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        info.Add(Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell getprop ro.boot.vbmeta.device_state");//获取BL状态2
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase) && info.Count == 7)
                    {
                        info.Add(Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    if (info.Count == 7)
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell getprop ro.product.cpu.abi");//获取CPU架构
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        info.Add(Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    else
                    {
                        info.Add("--");
                    }
                    result = cmd.Run(adb, $" -s {deviceid} shell getprop ro.system.build.version.incremental");//获取编译架构
                    if (!Regex.IsMatch(result, $"adb.exe", RegexOptions.IgnoreCase))
                    {
                        info.Add(Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", ""));
                    }
                    else
                    {
                        info.Add("--");
                    }
                });
                return info;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> StartScrcpy(string deviceid)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    result = cmd.Openexe(scrcpy, $" -s {deviceid} --window-title WackeClient----->{deviceid}");

                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> StopScrcpy()
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    result = cmd.Closeexe("scrcpy");
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> ScreenReturn(string deviceid)
        {
            bool isSuccess = false;
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} shell input keyevent 4");
                    if (Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "") == "")
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> ScreenHome(string deviceid)
        {
            bool isSuccess = false;
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} shell input keyevent 3");
                    if (Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "") == "")
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> ScreenPower(string deviceid)
        {
            bool isSuccess = false;
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} shell input keyevent 26");
                    if (Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "") == "")
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootAndroid(string deviceid)
        {
            bool isSuccess = false;
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} reboot");
                    if (Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "") == "")
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootRecovery(string deviceid)
        {
            bool isSuccess = false;
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} reboot recovery");
                    if (Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "") == "")
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootFastBoot(string deviceid)
        {
            bool isSuccess = false;
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} reboot bootloader");
                    if (Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "") == "")
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootFastbootD(string deviceid)
        {
            bool isSuccess = false;
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} reboot fastboot");
                    if (Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "") == "")
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootEDL(string deviceid)
        {
            bool isSuccess = false;
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} reboot edl");
                    if (Regex.Replace(result, @"[\s\r\n]+", "").Replace(" ", "") == "")
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<List<string>> Getallapplist(string deviceid)
        {
            List<string> applist = new List<string>();
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} shell pm list packages -f");
                    if (result.Contains(".apk=com."))
                    {
                        List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        foreach (string line in lines)
                        {                           
                            applist.Add(line.Split('/').Last());
                        }
                    }
                });
                return applist;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<List<string>> Getuserapplist(string deviceid)
        {
            List<string> applist = new List<string>();
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(adb, $"-s {deviceid} shell pm list packages -3 -f");
                    if (result.Contains(".apk=com."))
                    {
                        List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        foreach (string line in lines)
                        {
                            applist.Add(line.Split('/').Last());
                        }
                    }
                });
                return applist;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> Disableapp(string deviceid,string package)
        {
            try
            {
                bool isSuccess = false;
                await Task.Run(() =>
                {
                    string result = cmd.Run(adb, $"-s {deviceid} shell pm disable-user {package}");
                    if  (result.Contains("new state:"))
                    {
                        isSuccess = true;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> Enableapp(string deviceid, string package)
        {
            try
            {
                bool isSuccess = false;
                await Task.Run(() =>
                {
                    string result = cmd.Run(adb, $"-s {deviceid} shell pm enable {package}");
                    if (result.Contains("new state:"))
                    {
                        isSuccess = true;
                    }
                });
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> Install(string deviceid , string apkpath )
        {
            try
            {
                bool issuccess = false;
                await Task.Run(() =>
                {
                    string result = cmd.Run(adb, $"-s {deviceid} install -r -d -g {apkpath}");
                    if (result.Contains("Success"))
                    {
                        issuccess = true;
                    }
                });
                return issuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> Uninstall(string deviceid , string package)
        {
            try
            {
                bool issuccess = false;
                await Task.Run(() =>
                {
                    string result = cmd.Run(adb, $"-s {deviceid} uninstall {package}");
                    if (result.Contains("Success"))
                    {
                        issuccess = true;
                    }
                });
                return issuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}