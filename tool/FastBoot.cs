using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WackeClient.tool;

namespace WackeClient.tool
{
    public class ProcessChangedArgs : EventArgs
    {
        public float Processedvalue { get; set; }
        public long Processedbytes { get; set; }
        public long Totalbytes { get; set; }
        public bool IsWriting { get; set; }
        
        public ProcessChangedArgs(long processedbytes,long totalbytes ,bool iswriting)
        { 
            
            Processedbytes = processedbytes;
            Totalbytes = totalbytes;
            Processedvalue = (float)processedbytes / totalbytes;
            IsWriting = iswriting;
        }
    }
    public static class FastBoot
    {
        private static readonly string fb = $"C:\\Program Files\\WackeClient\\Tool\\System\\fastboot.exe";

        private static long processedbytes = 0;
        private static long lastprocessedbytes = 0;
        private static long totalbytes = 0;
        private static readonly Cmd cmd = new Cmd();
        public static event EventHandler<ProcessChangedArgs> OnProcessChanged;

        public static void Initialize()
        {
            cmd.OnOutputReceived += OnOutput_Received;
        }
        public static List<string> Devices()
        {
            try
            {
                
                string result = null;
                result = cmd.Run(fb, "devices");
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
                        bool isMatch = Regex.IsMatch(line, $@"\b{Regex.Escape("\t fastboot")}\b", RegexOptions.IgnoreCase);
                        if (isMatch)
                        {
                            int index = line.IndexOf("\t fastboot", StringComparison.OrdinalIgnoreCase);
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

        private static void OnOutput_Received(object sender, string e)
        {
            if (!string.IsNullOrEmpty(e))
            {
                if (e.Contains("%")||e.Contains("Writing"))
                {
                    bool iswriting = false;
                    try
                    {
                        iswriting = false;
                        List<string> lines = new List<string>(e.Split(new[] { "(", "bytes)" }, StringSplitOptions.RemoveEmptyEntries));
                        processedbytes = long.Parse(lines[1].Split('/')[0].Trim());
                        totalbytes = long.Parse(lines[1].Split('/')[1].Trim());
                    }
                    catch (Exception)
                    {
                        processedbytes = 1;
                        totalbytes = 1;
                    }
                    if (e.Contains("Writing") || processedbytes == totalbytes) iswriting = true;
                    OnProcessChanged?.Invoke(sender, new ProcessChangedArgs(processedbytes, totalbytes,iswriting));
                }
            }
        }

        public static bool IsFastBootD(string device)
        {
            try
            {
                string result = null;
                result = cmd.Run(fb, $"-s {device} getvar is-userspace");
                if (Regex.IsMatch(result, $@"{Regex.Escape("is-userspace: no")}", RegexOptions.IgnoreCase))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        public static async Task<bool> RebootSystem(string deviceid)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} reboot");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootFastboot(string deviceid)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} reboot bootloader");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootFastbootD(string deviceid)
        {
            try
            {
                bool isuccess = false;
                await Task.Run( () =>
                {
                    isuccess = cmd.Openexe(fb, $"-s {deviceid} reboot fastboot");
                });
                Task.Delay(1000).Wait(); // 等待2秒钟以确保设备重启
                cmd.Closeexe(fb);
                return isuccess;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootRecovery(string deviceid)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} reboot recovery");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootEdl(string deviceid)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} oem edl");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> RebootEdlLenovo(string deviceid)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} reboot edl");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<string> GetProduct(string deviceid)
        {
            try
            {
                string result = null;
                string product = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(fb, $"-s {deviceid} getvar product");
                    if (result.Contains("Finished. Total time:"))
                    {
                        List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        foreach (var line in lines)
                        {
                            if (line.Contains("product:"))
                            {
                                product = line.Split(':')[1].Trim();
                            }
                        }
                    }
                });
                if (string.IsNullOrEmpty(product))
                {
                    product = "--";
                }
                return product;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<string> GetMemory(string deviceid)
        {
            try
            {
                string result = null;
                string memory = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(fb, $"-s {deviceid} getvar variant");
                    if (result.Contains("Finished. Total time:"))
                    {
                        List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        foreach (var line in lines)
                        {
                            if (line.Contains("UFS"))
                            {
                                memory = "UFS";
                            }
                            else if ((line.Contains("EMMC")))
                            {
                                memory = "EMMC";
                            }
                            
                        }
                    }
                });
                if (string.IsNullOrEmpty(memory))
                {
                    memory = "--";
                }
                return memory;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<string> GetSlot(string deviceid)
        {
            try
            {
                string result = null;
                string slot = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(fb, $"-s {deviceid} getvar current-slot");
                    if (result.Contains("Finished. Total time:"))
                    {
                        List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        foreach (var line in lines)
                        {
                            if (line.Contains("current-slot:"))
                            {
                                slot = line.Split(':')[1].Trim();
                            }
                        }
                    }
                });
                if (string.IsNullOrEmpty(slot))
                {
                    slot = "--";
                }
                return slot;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<string> GetBootloader(string deviceid)
        {
            try
            {
                string result = null;
                string bootloader = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(fb, $"-s {deviceid} oem device-info");
                    if (result.Contains("Finished. Total time:"))
                    {
                        if (result.Contains("unlocked: true"))
                        {
                            bootloader = "已解锁";
                        }
                        else if(result.Contains("unlocked: false"))
                        {
                            bootloader = "未解锁";
                        }  
                    }
                    result = cmd.Run(fb, $"-s {deviceid} oem lks");
                    if (result.Contains("Finished. Total time:"))
                    {
                        if (result.Contains("1"))
                        {
                            bootloader = "未解锁";
                        }
                        else if(result.Contains("0"))
                        {
                            bootloader = "已解锁";
                        }
                    }
                });
                if (string.IsNullOrEmpty(bootloader))
                {
                    bootloader = "--";
                }
                return bootloader;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> SetSlot(string deviceid,string slot)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} set_active {slot}");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> LockDevice(string deviceid)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} flashing lock");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> SendCommed(string deviceid,string Command)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} {Command}");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> Flash(string deviceid,string partition, string imgpath)
        {
            try
            {
                bool result = false;
                await Task.Run(async () =>
                {
                    string cmdResult = await cmd.RunAsync(fb, $"-s {deviceid} flash {partition} {imgpath}");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> Erase(string deviceid, string partition)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} erase {partition}");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> Boot (string deviceid, string imgpath)
        {
            try
            {
                bool result = false;
                await Task.Run(() =>
                {
                    string cmdResult = cmd.Run(fb, $"-s {deviceid} boot {imgpath}");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> DisableDmQC(string deviceid, string imgpath)
        {
            try
            {
                bool result = false;
                await Task.Run(async () =>
                {
                    string cmdResult = await cmd.RunAsync(fb, $"-s {deviceid} --disable-verity --disable-verification flash vbmeta {imgpath}");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<bool> DisableDmMTK(string deviceid)
        {
            try
            {
                bool result = false;
                await Task.Run(async () =>
                {
                    string cmdResult = await cmd.RunAsync(fb, $"-s {deviceid} oem cdms");
                    if (cmdResult.Contains("Finished. Total time:"))
                    {
                        result = true;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<string> GetCRC(string deviceid)
        {
            try
            {
                string result = null;
                await Task.Run(() =>
                {
                    result = cmd.Run(fb, $"-s {deviceid} getvar crc");
                    if (result.Contains("Finished. Total time:"))
                    {
                        List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        foreach (string line in lines)
                        {
                            if (line.Contains("crc")) result = line.Trim();
                        }
                    }
                    
                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<List<string>> GetPartitioninfo(string deviceid)
        {
            try
            {
                string result = null;
                List<string> partitions = new List<string>();
                await Task.Run(async () =>
                {
                    result = await cmd.RunAsync(fb, $"-s {deviceid} getvar all");
                    if (result.Contains("Finished. Total time:"))
                    {
                        List<string> lines = new List<string>(result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        foreach (string line in lines)
                        {
                            if (line.Contains("partition-type"))
                            {
                                partitions.Add(StringHelper.GetTextBetween(line, ":", ":"));
                                Debug.WriteLine(line);
                            }
                            
                        }
                    }

                });
                return partitions;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
    
}
