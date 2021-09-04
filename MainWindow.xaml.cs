using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MabinogiClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow I;
        SoundPlayer sound;
        DispatcherTimer timer;
        ObservableCollection<Clock> _clocks;
        ObservableCollection<CountDown> _countDowns;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        public MainWindow()
        {
            I = this;
            int panel = 0;
            double left = 0d, top = 0d;
            _clocks = new ObservableCollection<Clock>();
            _countDowns = new ObservableCollection<CountDown>();
            try
            {
                var error = false;
                var config = ConfigurationManager.AppSettings;
                foreach(var c in (config["clocks"] ?? "").Split(';'))
                    try
                    {
                        if (string.IsNullOrEmpty(c)) continue;
                        var a = c.Split(',');
                        var clock = new Clock() { TimeText = a[1] };
                        clock.IsEnabled = a[0] == "1";
                        _clocks.Add(clock);
                    }
                    catch
                    {
                        error = true;
                    }
                foreach(var cd in (config["countdowns"] ?? "").Split(';'))
                    try
                    {
                        if (string.IsNullOrEmpty(cd)) continue;
                        //1,h:m:s,memo,loop,startTime
                        var a = cd.Split(',');
                        var t = a[1].Split(':');
                        var cd2 = new CountDown(t[0], t[1], t[2], a[2]);
                        cd2.StartTime = new DateTime(long.Parse(a[4]));
                        cd2.IsEnabled = a[0] == "1" && cd2.PassSeconds < cd2.TotalSeconds;
                        if (a[3] == "1") cd2.Loop = true;
                        _countDowns.Add(cd2);
                    }
                    catch
                    {
                        error = true;
                    }
                if (error) MessageBox.Show("存档部分损坏");
                int.TryParse(config["panel"] ?? "0", out panel);
                double.TryParse(config["left"] ?? "0", out left);
                double.TryParse(config["top"] ?? "0", out top);
            }
            catch
            {
                MessageBox.Show("存档损坏");
            }
            InitializeComponent();
            Left = left;
            Top = top;
            helper = new WindowInteropHelper(this);
            clocks.ItemsSource = _clocks;
            countDowns.ItemsSource = _countDowns;
            tab.SelectedIndex = panel;
            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.5) };
            timer.Tick += Timer_Tick;
            timer.Start();
            sound = new SoundPlayer("save.wav");
            sound.Load();
            Timer_Tick(null, null);
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += NotifyIcon_Click; ;
            notifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("clock.ico", UriKind.Relative)).Stream);
            notifyIcon.Visible = true;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            if (IsVisible) Hide();
            else
            {
                Show();
                if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            notifyIcon.Dispose();
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                string key, value;
                key = "panel"; value = tab.SelectedIndex.ToString();
                if (settings[key] == null) settings.Add(key, value);
                else settings[key].Value = value;
                key = "left"; value = Left.ToString();
                if (settings[key] == null) settings.Add(key, value);
                else settings[key].Value = value;
                key = "top"; value = Top.ToString();
                if (settings[key] == null) settings.Add(key, value);
                else settings[key].Value = value;
                key = "clocks";
                {
                    var sb = new StringBuilder();
                    foreach(var c in _clocks)
                    {
                        sb.Append(c.IsEnabled ? '1' : '0');
                        sb.Append(',');
                        sb.Append(c.TimeText);
                        sb.Append(';');
                    }
                    value = sb.ToString();
                }
                if (settings[key] == null) settings.Add(key, value);
                else settings[key].Value = value;
                key = "countdowns";
                {
                    var sb = new StringBuilder();
                    foreach(var cd in _countDowns)
                    {
                        //1,h:m:s,memo,loop,startTime
                        sb.Append(cd.IsEnabled ? '1' : '0');
                        sb.Append(',');
                        sb.Append(cd.H + ":" + cd.M + ":" + cd.S);
                        sb.Append(',');
                        sb.Append(cd.Memo);
                        sb.Append(',');
                        sb.Append(cd.Loop ? '1' : '0');
                        sb.Append(',');
                        sb.Append(cd.StartTime.Ticks);
                        sb.Append(';');
                    }
                    value = sb.ToString();
                }
                if (settings[key] == null) settings.Add(key, value);
                else settings[key].Value = value;
                configFile.Save(ConfigurationSaveMode.Modified);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("存档失败");
            }
        }

        WindowInteropHelper helper;
        public IntPtr Handle { get => helper.Handle; }

        private void Timer_Tick(object _, EventArgs _2)
        {
            var now = Clock.Real2Mabinogi(DateTime.Now);
            this.now.Text = now.ToString("HH:mm");
            foreach (var c in _clocks)
                if (c.IsEnabled && !c.IsInvalid && c.MabinogiTime.Hour == now.Hour && c.MabinogiTime.Minute == now.Minute)
                {
                    sound.Play();
                    c.ShowMessageBox();
                }
            foreach (var cd in _countDowns)
                if (cd.IsEnabled)
                {
                    cd.RefreshProgress();
                    if (cd.IsEnabled && cd.PassSeconds >= cd.TotalSeconds)
                    {
                        sound.Play();
                        cd.ShowMessage();
                        if (cd.Loop) cd.Restart();
                        else cd.IsEnabled = false;
                    }
                }
        }

        private void NewClock_Click(object sender, RoutedEventArgs e)
        {
            _clocks.Add(new Clock() { IsEnabled = true, TimeText = now.Text });
        }

        private void RemoveClock_Click(object sender, RoutedEventArgs e)
        {
            var c = (Clock)((Button)sender).DataContext;
            c.Remove();
            _clocks.Remove(c);
        }

        private void NewCountDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _countDowns.Add(new CountDown(hours.Text, minutes.Text, seconds.Text, memo.Text));
            }
            catch(Exception ex)
            {
                new Thread(()=>MessageBox.Show(ex.Message)).Start();
            }
        }


        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && Keyboard.Modifiers == ModifierKeys.Shift) Hide();
        }

        private void RestartCountDown_Click(object sender, RoutedEventArgs e)
        {
            var cd = (CountDown)((Button)sender).DataContext;
            if (cd.IsEnabled == false) cd.IsEnabled = true;
            else cd.Restart();
        }

        private void RemoveCountDown_Click(object sender, RoutedEventArgs e)
        {
            var cd = (CountDown)((Button)sender).DataContext;
            cd.Remove();
            _countDowns.Remove(cd);
        }
    }
}
