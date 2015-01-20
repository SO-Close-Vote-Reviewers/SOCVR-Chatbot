using CVChatbot.Bot;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CVChatbot.UI
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
            mng = new RoomManager();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;

            RoomManagerSettings settings = new RoomManagerSettings()
            {
                ChatRoomUrl = SettingsAccessor.GetSettingValue<string>("ChatRoomUrl"),
                Username = SettingsAccessor.GetSettingValue<string>("LoginUsername"),
                Email = SettingsAccessor.GetSettingValue<string>("LoginEmail"),
                Password = SettingsAccessor.GetSettingValue<string>("LoginPassword"),
                StartUpMessage = SettingsAccessor.GetSettingValue<string>("StartUpMessage"),
            };

            lblCurrentStatus.Content = "Joining...";
            await Task.Run(() => mng.JoinRoom(settings));
           
            lblCurrentStatus.Content = "Connected";
        }

        private void btnOpenLogFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
