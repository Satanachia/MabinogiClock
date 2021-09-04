using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabinogiClock
{
    class CountDown : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public DateTime StartTime;
        public readonly string H, M, S;

        public CountDown(string hours, string minutes, string seconds, string memo)
        {
            H = hours;
            M = minutes;
            S = seconds;
            var h = string.IsNullOrWhiteSpace(hours) ? 0d : double.Parse(hours);
            var m = string.IsNullOrWhiteSpace(minutes) ? 0d : double.Parse(minutes);
            var s = string.IsNullOrWhiteSpace(seconds) ? 0d : double.Parse(seconds);
            TotalSeconds = h * 3600 + m * 60 + s;
            if (TotalSeconds < 1d) throw new Exception("至少要1秒");
            StartTime = DateTime.Now;
            if (string.IsNullOrEmpty(memo))
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(hours)) sb.Append(hours + "时");
                if (!string.IsNullOrEmpty(minutes)) sb.Append(minutes + "分");
                if (!string.IsNullOrEmpty(seconds)) sb.Append(seconds + "秒");
                Memo = sb.ToString();
            }
            else Memo = memo;
            _isEnabled = true;
        }
        bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == false && value == true) Restart();
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
            }
        }
        bool _loop;
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Loop"));
            }
        }
        public string Memo { get; set; }
        public double PassSeconds { get => (DateTime.Now - StartTime).TotalSeconds; }
        public double TotalSeconds { get; private set; }

        public void Restart()
        {
            StartTime = DateTime.Now;
            RefreshProgress();
        }

        static readonly PropertyChangedEventArgs PASS_SECONDS = new PropertyChangedEventArgs("PassSeconds");
        public void RefreshProgress()
        {
            PropertyChanged?.Invoke(this, PASS_SECONDS);
        }
        MessageWindow window;
        public void ShowMessage()
        {
            if (window == null) window = new MessageWindow();
            window.t.Text = Memo + " 倒计时到了";
            window.Show();
        }
        public void Remove()
        {
            if (window != null) window.Close();
        }
    }
}
