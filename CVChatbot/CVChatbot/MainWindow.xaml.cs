using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace CVChatbot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RoomManager mng;

        public MainWindow()
        {
            InitializeComponent();
            lblCurrentStatus.Content = "Disconnected";
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            mng = new RoomManager();
            btnStart.IsEnabled = false;
            lblCurrentStatus.Content = "Connected";
        }

        private void btnOpenLogFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
