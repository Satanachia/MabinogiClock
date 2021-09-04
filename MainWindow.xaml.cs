using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<Clock> _clocks = new ObservableCollection<Clock>() { new Clock() { TimeText = "19:00" } };
        ObservableCollection<CountDown> _countDowns = new ObservableCollection<CountDown>();
        private System.Windows.Forms.NotifyIcon notifyIcon;
        public MainWindow()
        {
            I = this;
            InitializeComponent();
            helper = new WindowInteropHelper(this);
            clocks.ItemsSource = _clocks;
            countDowns.ItemsSource = _countDowns;
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
                    if (cd.IsEnabled && cd.PassSeconds > cd.TotalSeconds)
                    {
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
            cd.Restart();
        }

        private void RemoveCountDown_Click(object sender, RoutedEventArgs e)
        {
            var cd = (CountDown)((Button)sender).DataContext;
            cd.Remove();
            _countDowns.Remove(cd);
        }
    }
}
