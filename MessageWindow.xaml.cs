using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MabinogiClock
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        readonly static List<MessageWindow> windows = new List<MessageWindow>();
        
        public MessageWindow()
        {
            InitializeComponent();
            windows.Add(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        bool force;
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!force)
            {
                e.Cancel = true;
                Hide();
            }
        }
        public new void Close()
        {
            force = true;
            base.Close();
        }
        protected override void OnClosed(EventArgs e)
        {
            windows.Remove(this);
            foreach (var w in windows)
                if (w.IsVisible) return;
            FlashWindow.Stop(MainWindow.I);
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                foreach (var w in windows)
                    if (w.IsVisible) return;
                FlashWindow.Stop(MainWindow.I);
            }
        }
    }
}
