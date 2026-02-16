using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using WackeClient.date;

namespace WackeClient.tool
{
    public static class DeviceInfo
    {
        public static async Task<List<DeviceDate>> GetDeviceDateAsync()
        {
            try
            {
                bool isFastBootD = false;
                List<string> lines = new List<string>();
                List<DeviceDate> deviceList = new List<DeviceDate>();
                deviceList = await Task.Run(() =>
                {
                    lines = FindAll9008Ports();
                    foreach (var line in lines)
                    {
                        if (line != "") deviceList.Add(new DeviceDate { DeviceType = "9008", DviceName = line });
                    }
                    lines = FastBoot.Devices();
                    foreach (var line in lines)
                    {
                        if (line != "")
                        {
                            isFastBootD = FastBoot.IsFastBootD(line);
                            string devicetype = isFastBootD ? "FastBootD" : "FastBoot";
                            deviceList.Add(new DeviceDate { DeviceType = devicetype, DviceName = line });
                        }
                    }
                    lines = ADB.Devices();
                    foreach (var line in lines)
                    {
                        if (line != "") deviceList.Add(new DeviceDate { DeviceType = "ADB", DviceName = line });
                    }


                    return deviceList;
                });
                return deviceList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            

            

        }
        public static List<DeviceDate> GetDeviceDate()
        {
            try
            {
                bool isFastBootD = false;
                List<string> lines = new List<string>();
                List<DeviceDate> deviceList = new List<DeviceDate>();
                lines = FindAll9008Ports();
                foreach (var line in lines)
                {
                    if (line != "") deviceList.Add(new DeviceDate { DeviceType = "9008", DviceName = line });
                }
                lines = FastBoot.Devices();
                foreach (var line in lines)
                {
                    if (line != "")
                    {
                        isFastBootD = FastBoot.IsFastBootD(line);
                        string devicetype = isFastBootD ? "FastBootD" : "FastBoot";
                        deviceList.Add(new DeviceDate { DeviceType = devicetype, DviceName = line });
                    }
                }
                lines = ADB.Devices();
                foreach (var line in lines)
                {
                    if (line != "") deviceList.Add(new DeviceDate { DeviceType = "ADB", DviceName = line });
                }


                return deviceList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        public static List<string> FindAll9008Ports()
        {
            var ports = new List<string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%)'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString();
                        if (string.IsNullOrWhiteSpace(name) || !name.Contains("9008"))
                            continue;

                        int start = name.IndexOf("COM");
                        int end = name.IndexOf(")", start);
                        if (start > 0 && end > start)
                            ports.Add(name.Substring(start, end - start));
                    }
                }
            }
            catch
            {
            }
            return ports;
        }
    }

}
