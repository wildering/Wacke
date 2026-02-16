using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WackeClient.date
{
    public class Applist : INotifyPropertyChanged
    {
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }

        public string App { get; set; }

        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
    public class FastbootPartition : INotifyPropertyChanged
    {
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }

        public string Command { get; set; }
        public string Partition { get; set; }

        public string File { get; set; }

        public string FilePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
    public class Qcxmlinfo : INotifyPropertyChanged
    {
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }

        public string Lun { get; set; }
        public string Partition { get; set; }
        public string Size { get; set; }
        public string File { get; set; }
        public string FilePath { get; set; }
        public string StarSector { get; set; }
        public string Sector { get; set; }
        public string FileSectorOffset { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

}
