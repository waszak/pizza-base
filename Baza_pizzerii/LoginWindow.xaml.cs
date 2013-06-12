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
using System.Windows.Shapes;

namespace Baza_pizzerii
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LogIn_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logowanie..!");
        }

        private void LogInAsGuest_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Wchodzimy jako gość..!");
        }

        private void CreateNewAccount_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Formularz rejestracji nowego konta..!");
        }
    }
}
