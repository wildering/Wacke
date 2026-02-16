using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WackeClient.tool
{
    public class Cmd
    {
        
        public event EventHandler<string> OnOutputReceived;
        public async Task<string> RunAsync(string exePath, string arguments)
        {
            
            if (!File.Exists(exePath))
                return ($"EXE 文件不存在: {exePath}");

            StringBuilder outputBuilder = new StringBuilder();
            StringBuilder errorBuilder = new StringBuilder();

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processInfo })
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);
                            OnOutputReceived?.Invoke(this, e.Data);
                            Debug.WriteLine($"输出: {e.Data}");
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);
                            OnOutputReceived?.Invoke(this, e.Data);
                            Debug.WriteLine($"输出: {e.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    await Task.Run(() =>
                    {
                        process.WaitForExit();
                        process.WaitForExit();
                    }).ConfigureAwait(false);
                    return (outputBuilder.ToString() + errorBuilder.ToString());
                    
                }
            }
            catch (Exception ex)
            {
                return (outputBuilder.ToString() + errorBuilder.ToString() + $"\n错误: {ex.Message}");
            }
        }
        public string Run(string exePath, string arguments)
        {
            if (!File.Exists(exePath))
                return ($"EXE 文件不存在: {exePath}");

            StringBuilder outputBuilder = new StringBuilder();
            StringBuilder errorBuilder = new StringBuilder();

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processInfo })
                {
                    process.Start();

                    // 直接使用 ReadToEnd 读取输出
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    outputBuilder.Append(output);
                    errorBuilder.Append(error);

                    process.WaitForExit();
                    process.WaitForExit();
                    Debug.WriteLine($"输出: {output} {error}");
                    return (outputBuilder.ToString()+errorBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                errorBuilder.AppendLine($"错误: {ex.Message}");
                return (outputBuilder.ToString()+errorBuilder.ToString());
            }
        }
        public bool Openexe(string exePath,string arguments)
        {

            try
            {
                if (!File.Exists(exePath))
                    throw new Exception($"EXE 文件不存在: {exePath}");
                bool found = false;
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    string processName = Path.GetFileNameWithoutExtension(exePath); ; // 替换为你想要检测的进程名称


                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(500); // 等待1秒钟
                        // 获取所有正在运行的进程
                        Process[] processes = Process.GetProcesses();
                        // 遍历进程列表，查找特定进程
                        foreach (Process proc in processes)
                        {
                            try
                            {
                                if (proc.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        if (found) break;

                    }
                }
                return found;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Closeexe (string exename)
        {
            try
            {
                using (Process process = new Process() )
                {
                    Process[] processes = Process.GetProcessesByName(exename);

                    // 遍历进程列表，找到特定进程并关闭
                    foreach (Process proc in processes)
                    {
                        try
                        {
                            proc.Kill();
                            proc.WaitForExit(500);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"无法关闭进程 {exename}: {ex.Message}");
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
