using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WackeClient.tool
{
    internal class Windowsinfo
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlGetVersion(ref RTL_OSVERSIONINFOW osVersionInfo);
        public static async Task<string> GetWindowsVersionAsync()
        {
            return await Task.Run(() =>
            {
                var osvi = new RTL_OSVERSIONINFOW
                {
                    dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(RTL_OSVERSIONINFOW))
                };

                int result = RtlGetVersion(ref osvi);
                if (result == 0) // STATUS_SUCCESS
                {
                    return GetWindowsVersion(osvi.dwMajorVersion, osvi.dwMinorVersion, osvi.dwBuildNumber);
                }
                else
                {
                    return "获取系统版本失败";
                }
            });
        }
        public static bool Is64Bit()
        {
            
            
            return Environment.Is64BitOperatingSystem;
            
        }
        public static string GetWindowsVersion()
        {
            var osvi = new RTL_OSVERSIONINFOW
            {
                dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(RTL_OSVERSIONINFOW))
            };

            int result = RtlGetVersion(ref osvi);
            if (result == 0) // STATUS_SUCCESS
            {
                return GetWindowsVersion(osvi.dwMajorVersion, osvi.dwMinorVersion, osvi.dwBuildNumber);
            }
            else
            {
                return "获取系统版本失败";
            }
        }

        private struct RTL_OSVERSIONINFOW
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
        }

        private static string GetWindowsVersion(uint majorVersion, uint minorVersion, uint buildNumber)
        {
            switch (majorVersion)
            {
                case 10:
                    if (buildNumber >= 22000)
                    {
                        return "Windows 11";
                    }
                    else
                    {
                        return "Windows 10";
                    }
                case 6:
                    switch (minorVersion)
                    {
                        case 0:
                            return "Windows Vista";
                        case 1:
                            return "Windows 7";
                        case 2:
                            return "Windows 8";
                        case 3:
                            return "Windows 8.1";
                        default:
                            return "Unknown Windows version";
                    }
                case 5:
                    return "Windows XP";
                default:
                    return "Unknown Windows version";
            }
        }
    }
}
