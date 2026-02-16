using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;

namespace WackeClient.tool
{
    /// <summary>
    /// 设备变化事件参数
    /// </summary>
    public class DeviceChangeEventArgs : EventArgs
    {
        public DateTime EventTime { get; }
        public string DeviceId { get; }
        public DeviceChangeEventArgs(string deviceId)
        {
            EventTime = DateTime.Now;
            DeviceId = deviceId;
        }
    }
    
    /// <summary>
    /// 设备监控器，负责监听设备变化并触发事件
    /// </summary>
    public class DeviceMonitor : IDisposable
    {
        private ManagementEventWatcher _watcher;
        private bool _disposed;
        private string _lastDeviceId; // 记录上一次设备ID
        private DateTime _lastEventTime = DateTime.MinValue; // 记录上一次事件时间
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(1000); // 去抖间隔 1000ms

        // 事件：当设备发生变化时触发
        public event EventHandler<DeviceChangeEventArgs> DeviceChanged;

        public DeviceMonitor()
        {
            _disposed = false;
            InitializeWatcher();
        }

        private void InitializeWatcher()
        {
            try
            {
                // 监听设备插入和移除事件
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
                _watcher = new ManagementEventWatcher(query);
                _watcher.EventArrived += Watcher_EventArrived;
            }
            catch (ManagementException ex)
            {
                throw new InvalidOperationException("无法初始化 WMI 事件监听，可能需要管理员权限", ex);
            }
        }

        public void Start()
        {
            if (_watcher == null)
            {
                throw new InvalidOperationException("事件监听器未初始化");
            }
            try
            {
                _watcher.Start();
                
            }
            catch (ManagementException ex)
            {
                throw new InvalidOperationException("无法开始监听设备变化，可能需要管理员权限", ex);
            }
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.Stop();
            }
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                // 获取设备信息
                string deviceId = "未知设备";
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
                {
                    foreach (ManagementObject device in searcher.Get())
                    {
                        deviceId = device["DeviceID"]?.ToString() ?? deviceId;
                        break; // 取第一个设备，实际可根据需求优化
                    }
                }

                // 去重：忽略相同设备ID且时间间隔小于 1000ms 的事件
                if (deviceId == _lastDeviceId && (DateTime.Now - _lastEventTime) < _debounceInterval)
                {
                    return;
                }

                // 更新记录
                _lastDeviceId = deviceId;
                _lastEventTime = DateTime.Now;

                // 触发事件
                DeviceChanged?.Invoke(this, new DeviceChangeEventArgs(deviceId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理设备变化事件时发生错误: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _watcher?.Dispose();
                _watcher = null;
                _disposed = true;
            }
        }
    }
    
}
