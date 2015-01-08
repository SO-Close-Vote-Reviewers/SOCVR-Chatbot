using System;
using System.Collections.Generic;
using System.IO;
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
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.txt"))
            {
                var settings = File.ReadAllLines("settings.txt")
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0], x => x[1]);

                txtUsername.Text = settings["LoginUsername"];
                txtEmail.Text = settings["LoginEmail"];
                txtPassword.Password = settings["LoginPassword"];
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            mng = new RoomManager(txtUsername.Text, txtEmail.Text, txtPassword.Password);
            btnLogin.IsEnabled = false;
        }
    }
}
