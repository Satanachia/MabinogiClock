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
        SoundPlayer sound;
        DispatcherTimer timer;
        ObservableCollection<Clock> _clocks = new ObservableCollection<Clock>() { new Clock() { TimeText = "19:00" } };
        public MainWindow()
        {
            InitializeComponent();
            clocks.ItemsSource = _clocks;
            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.5) };
            timer.Tick += Timer_Tick;
            timer.Start();
            sound = new SoundPlayer("save.wav");
            sound.Load();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = Clock.Real2Mabinogi(DateTime.Now);
            this.now.Text = now.ToString("HH:mm");
            foreach(var c in _clocks)
                if (c.IsEnabled && !c.IsInvalid && c.MabinogiTime.Hour == now.Hour && c.MabinogiTime.Minute == now.Minute)
                {
                    MessageBox.Show(this.now.Text, Title, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.ServiceNotification);
                    sound.Play();
                }
        }

        private void NewMabinogiClock_Click(object sender, RoutedEventArgs e)
        {
            _clocks.Add(new Clock() { IsEnabled = true, TimeText = now.Text });
        }
    }
}
