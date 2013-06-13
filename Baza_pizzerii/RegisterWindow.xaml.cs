using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Top = this.Top;
            loginWindow.Left = this.Left;
            loginWindow.Show();
            this.Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (rola_cb.Text == "Wybierz role")
            {
                MessageBox.Show("Nie wybrano roli!");
                return;
            }

            if (login_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole login nie może być puste!");
                return;
            }

            //sprawdzanie, czy dany login jest juz w bazie
            if (login_tb.Text == "w_bazie")
            {
                MessageBox.Show("Użytkownik o podanym loginie już istnieje!");
                return;
            }

            if (password1_pb.Password.Length < 6)
            {
                MessageBox.Show("Hasło musi składać się przynajmniej z 6 znaków!");
                return;
            }

            if (password1_pb.Password != password2_pb.Password)
            {
                MessageBox.Show("Hasła są różne!");
                return;
            }

            if (imie_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole imię nie może być puste!");
                return;
            }

            if (nazwisko_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole nazwisko nie może być puste!");
                return;
            }

            if (email_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole E-mail nie może być puste!");
                return;
            }
        }
    }
}
