using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WackeClient.date
{
    public static class MyPath
    {

        public static readonly string mainpath = "C:\\Program Files\\WackeClient";
        public static string Configpath {  get; set; }
        public static string oldversionpath = "";
        public static string Oldexeconfigfpath = "";
        public static readonly string rarpath = "C:\\Program Files\\WackeClient\\Tool.rar";
        public static readonly string toolpath = "C:\\Program Files\\WackeClient\\Tool";
        public static readonly string miusb = "C:\\Program Files\\WackeClient\\Tool\\System\\USB3.bat";
        public static readonly string remiusb = "C:\\Program Files\\WackeClient\\Tool\\System\\ReUSB3.bat";
        public static readonly string adb = "C:\\Program Files\\WackeClient\\Tool\\System\\adb.exe";
        public static readonly string fastboot = "C:\\Program Files\\WackeClient\\Tool\\System\\fastboot.exe";
        public static readonly string  fh= "C:\\Program Files\\WackeClient\\Tool\\System\\fh_loader.exe";
        public static readonly string temppath = "C:\\Program Files\\WackeClient\\Tool\\Temp";
        public static void Initialize()
        {
            byte[] resourceData = Properties.Resources.Tool;
            if (resourceData == null || resourceData.Length == 0)
            {
                throw new FileNotFoundException("找不到嵌入资源：Tool");
            }
            try
            {
                Directory.CreateDirectory(mainpath);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);                                                                                                                                                                             
            }
            try
            {
                File.WriteAllBytes(rarpath, resourceData);
                ExtractRar(rarpath, mainpath);
                File.Delete(rarpath);
            }
            catch { }
        }
        public static bool Chenckmypath()
        {
            try
            {
                bool a = Directory.Exists(mainpath);
                bool b = Directory.Exists(toolpath);
                if (a&&b)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static void ExtractRar(string rarPath, string destFolder)
        {
            if (!File.Exists(rarPath))
                throw new FileNotFoundException("找不到 rar 文件", rarPath);

            Directory.CreateDirectory(destFolder);
            DirectoryInfo dir = new DirectoryInfo(destFolder);
            dir.Attributes = FileAttributes.Hidden | FileAttributes.System;//使文件夹不可见
            using (var archive = RarArchive.Open(rarPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(destFolder,
                            new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                    }
                }
            }
        }
    }

    

}
