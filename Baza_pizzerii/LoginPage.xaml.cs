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
        private bool IsValidLogin() {
            bool isValid = false;
            if (login_tb.Text.Length == 0) {
                MessageBox.Show("Pole login nie może być puste!");
            } else if (!Regex.IsMatch(login_tb.Text, @"^[a-zA-Z][a-zA-Z0-9]*$")) {
                MessageBox.Show("Login zawiera niepoprawne znaki! \nW poprawnym loginie pierwszy znak jest literą, a reszta znaków literą lub cyfrą.");
            } else {
                isValid = true;
            }
            return isValid;
        }

        private bool IsValidPassword() {
            bool isValid = false;
            if (password_pb.Password.Length == 0) {
                MessageBox.Show("Pole hasło nie może być puste!");
            } else if (!Regex.IsMatch(password_pb.Password, @"^[a-zA-Z0-9]*$")) {
                MessageBox.Show("Hasło zawiera niepoprawne znaki! \nPoprawne hasło składa się wyłącznie z liter i cyfr.");
            } else {
                isValid = true;
            }
            return isValid;
        }

        private NpgsqlConnection loginUserToDB(string username, string password) {
            string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                                   "localhost", "5432", username, password, "bazapizzerii");
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            return conn;
        }

        private void LogIn_click(object sender, RoutedEventArgs e) {
            if (!IsValidLogin() || !IsValidPassword()) {
                return;
            }

            try {
                NpgsqlConnection conn = loginUserToDB(login_tb.Text, password_pb.Password);
                string sql = "SELECT id_osoba, rola, imie, nazwisko " +
                                "FROM uzytkownik JOIN osoba USING (id_osoba) " +
                                "WHERE login = '" + login_tb.Text + "';";

                NpgsqlCommand command = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dr = command.ExecuteReader();
                if (dr.Read() == false) {
                    MessageBox.Show("Wystąpił błąd podczas logowania!");
                    conn.Close();
                    return;
                }
                //przechoujemy potrzebne informacje w zmiennych globalnych całej aplikacji
                App.Current.Properties["login"] = login_tb.Text;
                App.Current.Properties["id_osoba"] = dr[0];
                App.Current.Properties["rola"] = dr[1];
                App.Current.Properties["imie"] = dr[2];
                App.Current.Properties["nazwisko"] = dr[3];
                App.Current.Properties["Connection"] = conn;

                if (App.Current.Properties["rola"].ToString() == "wlasciciel_pizzerii") {
                    MessageBox.Show("Twoje konto jest typu wlasciciel_pizzerii. " +
                                    "\nNie zaimplementowano jeszcze dalszej funkcjonalności dla właściciela ;)");
                    return;
                }
                conn.Close();
                openSearchPizzeriaWindow();
               // this.Close();
            } catch (Exception msg) {
                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Logowanie nie powiodło się!");
            }
        }

        private void openSearchPizzeriaWindow() {
            var loginWindow = new SearchPizzeriaWindow();
           /* loginWindow.Top = this.Top;
            loginWindow.Left = this.Left;*/
            loginWindow.Show();
        }

        private void LogInAsGuest_click(object sender, RoutedEventArgs e) {
            try {
                NpgsqlConnection conn = loginUserToDB("gosc_konto", "gosc_haslo");

                //zmienne globalne
                App.Current.Properties["login"] = "gosc";
                App.Current.Properties["rola"] = "gosc";
                App.Current.Properties["Connection"] = conn;

                conn.Close();
                openSearchPizzeriaWindow();
                //this.Close();
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
    }
}
