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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Npgsql;

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page {
        public LoginPage() {
            InitializeComponent();
        }

        private void LogIn_click(object sender, RoutedEventArgs e) {

            string return_msg;
            if (!Validate.Login(login_tb.Text, out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (!Validate.Password(password_pb.Password, out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            try {
                NpgsqlConnection conn = DB.loginUserToDB(login_tb.Text, password_pb.Password);
                string sql = "SELECT id_osoba, rola " +
                                "FROM uzytkownik JOIN osoba USING (id_osoba) " +
                                "WHERE login = @login;";

                NpgsqlCommand command = new NpgsqlCommand(null, conn);
                command.CommandText = sql;
                command.Parameters.AddWithValue("@login", login_tb.Text);
                command.Prepare();
                NpgsqlDataReader dr = command.ExecuteReader();
                if (dr.Read() == false) {
                    MessageBox.Show("Wystąpił błąd podczas logowania!");
                    conn.Close();
                    return;
                }
                //przechoujemy potrzebne informacje w zmiennych globalnych całej aplikacji
                App.Current.Properties["login"] = login_tb.Text;
                App.Current.Properties["password"] = password_pb.Password;
                App.Current.Properties["id_osoba"] = dr[0];
                App.Current.Properties["rola"] = dr[1];

                conn.Close();
                if (App.Current.Properties["rola"].ToString() == "wlasciciel_pizzerii") {
                    openPizzeriaManagementWindow();
                }
                else {              
                    openSearchPizzeriaWindow();
                }
            } catch (Exception msg) {
                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Logowanie nie powiodło się!");
            }
        }

        private void openSearchPizzeriaWindow() {
            this.NavigationService.Navigate(new SearchPizzaPage());
        }

        private void openPizzeriaManagementWindow() {
            this.NavigationService.Navigate(new PizzeriaManagementPage());
        }


        private void LogInAsGuest_click(object sender, RoutedEventArgs e) {
            try {
                NpgsqlConnection conn = DB.loginUserToDB("gosc_konto", "gosc_haslo");

                //zmienne globalne
                App.Current.Properties["login"] = "gosc_konto";
                App.Current.Properties["password"] = "gosc_haslo";
                App.Current.Properties["rola"] = "gosc";

                conn.Close();
                openSearchPizzeriaWindow();
            } catch (Exception msg) {
                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas łączenia się z bazą danych!");
            }
        }

        private void CreateNewAccount_click(object sender, RoutedEventArgs e) {
            this.NavigationService.Navigate(new RegisterPage());
        }


        private void logout_Click(object sender, RoutedEventArgs e){
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }
    }
}
