using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
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
        public MainWindow()
        {
            I = this;
            InitializeComponent();
            helper = new WindowInteropHelper(this);
            clocks.ItemsSource = _clocks;
            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.5) };
            timer.Tick += Timer_Tick;
            timer.Start();
            sound = new SoundPlayer("save.wav");
            sound.Load();
            Timer_Tick(null, null);
        }

        WindowInteropHelper helper;
        public IntPtr Handle { get => helper.Handle; }

        private void Timer_Tick(object _, EventArgs _2)
        {
            var now = Clock.Real2Mabinogi(DateTime.Now);
            this.now.Text = now.ToString("HH:mm");
            foreach(var c in _clocks)
                if (c.IsEnabled && !c.IsInvalid && c.MabinogiTime.Hour == now.Hour && c.MabinogiTime.Minute == now.Minute)
                {
                    sound.Play();
                    c.ShowMessageBox();
                    FlashWindow.Start(this);
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
    }
}
