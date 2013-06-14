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
    /// Interaction logic for SearchPizzeriaWindow.xaml
    /// </summary>
    public partial class SearchPizzeriaWindow : Window
    {
        public SearchPizzeriaWindow()
        {
            InitializeComponent();
        }

        private void myAccount_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Properties["rola"].ToString() == "gosc")
            {
                MessageBox.Show("Korzystasz z aplikacji jako gość.\nFunkcjonalność dostępna dla zalogowanych użytkowników.");
                return;
            }
            if (App.Current.Properties["rola"].ToString() == "klient")
            {
                //testowo na razie tylko wyswietlamy wszystko globalne zmienne
                MessageBox.Show("Login: "+ App.Current.Properties["login"].ToString() +
                                "\nId_osoba: " + App.Current.Properties["id_osoba"].ToString() +
                                "\nRola: " + App.Current.Properties["rola"].ToString() +
                                "\nImie: " + App.Current.Properties["imie"].ToString() +
                                "\nNazwisko: " + App.Current.Properties["nazwisko"].ToString());
                return;
            }
        }

        private void searchPizzeria_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new SearchPizzeriaWindow();
            loginWindow.Top = this.Top;
            loginWindow.Left = this.Left;
            loginWindow.Show();
            this.Close();
        }

        private void searchPizza_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new SearchPizzaWindow();
            loginWindow.Top = this.Top;
            loginWindow.Left = this.Left;
            loginWindow.Show();
            this.Close();
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Top = this.Top;
            loginWindow.Left = this.Left;
            loginWindow.Show();
            this.Close();
        }
    }
}
