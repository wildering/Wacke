using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using WackeClient.date;
using ZstdSharp.Unsafe;

namespace WackeClient.tool
{
     
    public static class QCHelper
    {
        private static readonly Regex NameRegex = new Regex
            (
                @"^(prog_.+_firehose([_.].*)?|prog_.*firehose_.+|prog_firehose_.+|xbl_.+devprg_.+)$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            );

        // 允许的后缀
        private static readonly HashSet<string> AllowedExt = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase) { ".elf", ".mbn", ".melf" };
        /// <summary>
        /// 在指定目录（含子目录）中查找符合 ProgrammerPattern 且后缀为 .elf/.mbn/.melf 的文件。
        /// </summary>
        public static string FindProgrammerFiles(string root)
        {


            if (string.IsNullOrWhiteSpace(root))
                throw new ArgumentException("目录路径不能为空", nameof(root));
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException($"目录不存在: {root}");

            var results = new List<string>();

            foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(path);
                if (!AllowedExt.Contains(ext)) continue;

                var name = Path.GetFileNameWithoutExtension(path);
                if (NameRegex.IsMatch(name))
                    results.Add(path);
            }
            if (results.Count > 0)
            {
                return results[0];
            }
            else
            {
                return null;
            }


        }
        public static async Task<List<Qcxmlinfo>> GetQcxmlinfosAsync(string qcxmlpath)
        {
            try
            {
                var list = new List<Qcxmlinfo>();
                string xmlFilePath = qcxmlpath;
                string images = Path.GetDirectoryName(qcxmlpath);
                await Task.Run(() =>
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(xmlFilePath);
                    XmlNode xmlNode = xmldoc.SelectSingleNode("data");
                    XmlNodeList xmlnodelist = xmlNode.SelectNodes("program");
                    foreach (XmlNode node in xmlnodelist)
                    {
                        string a = node.Attributes["SECTOR_SIZE_IN_BYTES"]?.InnerText;
                        string filename = node.Attributes["filename"]?.InnerText;
                        string label = node.Attributes["label"]?.InnerText;
                        string start = node.Attributes["start_sector"]?.InnerText;
                        string number = node.Attributes["num_partition_sectors"]?.InnerText;
                        string lun = node.Attributes["physical_partition_number"]?.InnerText;
                        string offset = (node.Attributes["file_sector_offset"]?.InnerText) ?? "0";
                        long bytes = long.Parse(a) * long.Parse(number);
                        string size = BytesHelper.FormatBytes(bytes);
                        bool selected = !string.IsNullOrEmpty(filename);
                        string filepath = string.IsNullOrEmpty(filename) ? "双击选择文件" : Path.Combine(images, filename);
                        string file = string.IsNullOrEmpty(filename) ? "空文件" : filename;
                        list.Add(new Qcxmlinfo
                        {
                            Selected = selected,
                            Lun = lun,
                            Partition = label,
                            Size = size,
                            File = file,
                            FilePath = filepath,
                            StarSector = start,
                            Sector = number,
                            FileSectorOffset = offset
                        });
                    }
                });
                return list;
            }
            catch (Exception ex)
            {
                return new List<Qcxmlinfo>();
            }
            
        }
        public static async Task<string> StorageType(string qcxmlpath)
        {
            try
            {
                
                string xmlFilePath = qcxmlpath;
                List<string> list = new List<string>();
                string storageType = "";
                await Task.Run(() =>
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(xmlFilePath);
                    XmlNode xmlNode = xmldoc.SelectSingleNode("data");
                    XmlNodeList xmlnodelist = xmlNode.SelectNodes("program");
                    foreach (XmlNode node in xmlnodelist)
                    {
                        string a = node.Attributes["SECTOR_SIZE_IN_BYTES"]?.InnerText;
                        list.Add(a);
                    }
                });
                if (list.Count > 0)
                {
                    storageType = list[0] == "512" ? "EMMC" : "UFS";
                }
                return storageType;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static string Getelfpath(string name)
        {
            switch (name)
            {
                case "小米高通骁龙625芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_625";
                case "小米高通骁龙632芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_632";
                case "小米高通骁龙636芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_636";
                case "小米高通骁龙660芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_660";
                case "小米高通骁龙665芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_665";
                case "小米高通骁龙675芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_675";
                case "小米高通骁龙680芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_680";
                case "小米高通骁龙710芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_710";
                case "小米高通骁龙712芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_712";
                case "小米高通骁龙730芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_730";
                case "小米高通骁龙778G芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_778g";
                case "小米高通骁龙845芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_845";
                case "小米高通骁龙855芯片(新加密,免授权)(方案1)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_855";
                case "小米高通骁龙855芯片(新加密,免授权)(方案2)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_855_2";
                case "小米高通骁龙865芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_865";
                case "小米高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_870";
                case "小米高通骁龙888_ddr芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_888_ddr";
                case "小米高通骁龙888_lite芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\mi_noauth\\devprg_888_lite";
                case "黑鲨1高通骁龙845芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_1";
                case "黑鲨hello高通骁龙845芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_helo";
                case "黑鲨2高通骁龙855芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_2";
                case "黑鲨2Pro高通骁龙855芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_2pro";
                case "黑鲨3高通骁龙865芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_3";
                case "黑鲨3Pro高通骁龙865芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_3pro";
                case "黑鲨3S高通骁龙865芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_3s";
                case "黑鲨4高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_4_4s";
                case "黑鲨4S高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_4_4s";
                case "黑鲨5高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\blackshark\\devprg_4_4s";
                case "OPPO高通骁龙450芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_450";
                case "OPPO高通骁龙460芯片(新加密,免授权)(A32机型)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_460_A32";
                case "OPPO高通骁龙665芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_665";
                case "OPPO高通骁龙765G芯片(新加密,免授权)(4g版)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_765g";
                case "OPPO高通骁龙765G芯片(新加密,免授权)(5g版)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_765g_5g";
                case "OPPO高通骁龙778G芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_778";
                case "OPPO高通骁龙855芯片(新加密, 免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_855";
                case "OPPO高通骁龙865芯片(新加密,免授权)(方案1)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_865_1";
                case "OPPO高通骁龙865芯片(新加密,免授权)(方案2)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_865_2";
                case "OPPO高通骁龙870芯片(新加密,免授权)(方案1)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_870_1";
                case "OPPO高通骁龙870芯片(新加密,免授权)(方案2)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_870_2";
                case "OPPO高通骁龙888芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oppo\\devprg_888";
                case "一加5高通骁龙835芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oneplus\\devprg_5";
                case "一加6高通骁龙835芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\oneplus\\devprg_6";
                case "联想Y700一代高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_y700_1";
                case "联想Y700二代高通骁龙8+Gen1芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_y700_2";
                case "联想Y700三代高通骁龙8Gen2芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_y700_3";
                case "联想Y700四代高通骁龙8Elite芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_y700_4";
                case "联想L70081高通骁龙888芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_L70081";
                case "联想L71061高通骁龙8Gen1芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_L71061";
                case "联想L71091高通骁龙8+Gen1芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_L71091";
                case "联想L79031高通骁龙865Plus芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_L79031";
                case "联想TB128FU高通骁龙680芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TB128FU";
                case "联想TB138FC高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TB138FC";
                case "联想TB371FC高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TB371FC";
                case "联想TB520FU高通骁龙8Gen3芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TB520FU";
                case "联想TBJ606F高通骁龙662芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TBJ606F";
                case "联想TBJ607F高通骁龙750G芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TBJ607F";
                case "联想TBJ706F高通骁龙730GF芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TBJ706F";
                case "联想TBJ716F高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TBJ716F";
                case "联想TBQ706F高通骁龙870芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\Lenovo\\devprg_TBQ706F";
                case "红魔3/3S高通骁龙855芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_3_3S";
                case "红魔5G高通骁龙865芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_5G";
                case "红魔6/6Pro高通骁龙888芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_6_6Pro";
                case "红魔6R高通骁龙888芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_6R";
                case "红魔6SPro高通骁龙888Plus芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_6SPro";
                case "红魔7高通骁龙8Gen1芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_7";
                case "红魔7Pro高通骁龙8Gen1芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_7Pro";
                case "红魔7S高通骁龙8+Gen1芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_7S";
                case "红魔7SPro高通骁龙8+Gen1芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_7SPro";
                case "红魔8高通骁龙8Gen2芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_8";
                case "红魔9高通骁龙8Gen3芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_9";
                case "红魔10高通骁龙8Elite芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_10";
                case "红魔电竞平板高通骁龙8+Gen1芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_gamepad";
                case "红魔电竞平板Pro高通骁龙8Gen3芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_padpro";
                case "红魔电竞游戏手机高通骁龙835芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_game";
                case "红魔平板3D探索版高通骁龙8Gen2芯片(新加密,免授权)":
                    return "C:\\Program Files\\WackeClient\\Tool\\Sahara\\redmagic\\devprg_pad3d";
            }
            return null;
        }
        public static bool IsBypassMi(string name)
        {
            switch (name)
            {
                case "小米高通骁龙625芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙632芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙636芯片(新加密,免授权)":
                    return false;
                case "小米高通骁龙660芯片(新加密,免授权)":
                    return false;
                case "小米高通骁龙665芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙675芯片(新加密,免授权)":
                    return false;
                case "小米高通骁龙680芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙710芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙712芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙730芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙778G芯片(新加密,免授权)":
                    return false;
                case "小米高通骁龙845芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙855芯片(新加密,免授权)(方案1)":
                    return true;
                case "小米高通骁龙855芯片(新加密,免授权)(方案2)":
                    return true;
                case "小米高通骁龙865芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙870芯片(新加密,免授权)":
                    return true;
                case "小米高通骁龙888_ddr芯片(新加密,免授权)":
                    return false;
                case "小米高通骁龙888_lite芯片(新加密,免授权)":
                    return false;
                case "黑鲨1高通骁龙845芯片(新加密,免授权)":
                    return false;
                case "黑鲨hello高通骁龙845芯片(新加密,免授权)":
                    return false;
                case "黑鲨2高通骁龙855芯片(新加密,免授权)":
                    return false;
                case "黑鲨2Pro高通骁龙855芯片(新加密,免授权)":
                    return false;
                case "黑鲨3高通骁龙865芯片(新加密,免授权)":
                    return false;
                case "黑鲨3Pro高通骁龙865芯片(新加密,免授权)":
                    return false;
                case "黑鲨3S高通骁龙865芯片(新加密,免授权)":
                    return false;
                case "黑鲨4高通骁龙870芯片(新加密,免授权)":
                    return true;
                case "黑鲨4S高通骁龙870芯片(新加密,免授权)":
                    return true;
                case "黑鲨5高通骁龙870芯片(新加密,免授权)":
                    return true;
            }
            return false;
        }
        public static async Task<bool> BuildRawXml(List<Qcxmlinfo> qcxmlinfo,string type,string outputpath)
        {
            try
            {
                bool issuccess = await Task.Run(() =>
                {
                    try
                    {
                        string Sectorsize = type == "ufs" ? "4096" : "512";
                        int sizeinone = type == "ufs" ? 4096 : 512;
                        int jici = type == "ufs" ? 9 : 1;
                        for (int i = 0; i < jici; i++)
                        {
                            var xmldata = new List<Qcxmlinfo>();
                            string xmllun = i.ToString();
                            foreach (var item in qcxmlinfo)
                            {
                                if (xmllun == item.Lun)
                                {
                                    xmldata.Add(item);
                                }
                            }
                            if (xmldata.Count > 0)
                            {
                                XmlDocument doc = new XmlDocument();
                                XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", null, null);
                                doc.AppendChild(decl);
                                // 写注释
                                doc.AppendChild(doc.CreateComment($"NOTE: WackeClient "));
                                doc.AppendChild(doc.CreateComment("NOTE: This is an ** Autogenerated file **"));
                                doc.AppendChild(doc.CreateComment($"NOTE: Sector size is {Sectorsize}bytes"));
                                // 创建<data>
                                XmlElement root = doc.CreateElement("data");
                                doc.AppendChild(root);
                                foreach (var item in xmldata)
                                {
                                    XmlElement prog = doc.CreateElement("program");
                                    prog.SetAttribute("SECTOR_SIZE_IN_BYTES", Sectorsize);
                                    prog.SetAttribute("file_sector_offset", item.FileSectorOffset);
                                    prog.SetAttribute("filename", item.File);
                                    prog.SetAttribute("label", item.Partition);
                                    prog.SetAttribute("num_partition_sectors", item.Sector);
                                    prog.SetAttribute("physical_partition_number", item.Lun);
                                    int number = int.Parse(item.Sector);
                                    long bytes = (long)number * sizeinone;
                                    string kb = (bytes / 1024).ToString();
                                    prog.SetAttribute("size_in_KB", kb);
                                    prog.SetAttribute("sparse", "false");
                                    string starthex = null;
                                    if (item.Partition.Contains("BackupGPT"))
                                    {

                                        string sizehex = (sizeinone * number).ToString();
                                        starthex = $"({Sectorsize}*NUM_DISK_SECTORS)-{sizehex}";
                                    }
                                    else
                                    {
                                        int startnumber = int.Parse(item.StarSector);
                                        starthex = "0x" + (startnumber * sizeinone).ToString("x");
                                    }
                                    prog.SetAttribute("start_byte_hex", starthex);
                                    prog.SetAttribute("start_sector", item.StarSector);
                                    root.AppendChild(prog);
                                }
                                doc.Save(Path.Combine(outputpath, $"rawprogram{xmllun}.xml"));

                            }
                        }
                        return true;

                    }
                    catch { }
                    {
                        return false;
                    }
                });
                
                    
                
                return issuccess;
            }
            catch
            {
                return false;
            }
        }

    }
}