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
using Npgsql;
using System.Text.RegularExpressions;

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page {
        public RegisterPage() {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.GoBack();
        }

        private void Register_Click(object sender, RoutedEventArgs e) {

            if (rola_cb.Text == "Wybierz role") {
                MessageBox.Show("Nie wybrano roli!");
                return;
            }

            if (login_tb.Text.Length == 0) {
                MessageBox.Show("Pole login nie może być puste!");
                return;
            }

            if (!Regex.IsMatch(login_tb.Text, @"^[a-zA-Z][a-zA-Z0-9]*$")) {
                MessageBox.Show("Login zawiera niepoprawne znaki! \nW poprawnym loginie pierwszy znak jest literą, a reszta znaków literą lub cyfrą.");
                return;
            }

            if (password1_pb.Password.Length < 6) {
                MessageBox.Show("Hasło musi składać się przynajmniej z 6 znaków!");
                return;
            }

            if (password1_pb.Password != password2_pb.Password) {
                MessageBox.Show("Hasła są różne!");
                return;
            }

            if (!Regex.IsMatch(password1_pb.Password, @"^[a-zA-Z0-9]*$")) {
                MessageBox.Show("Hasło zawiera niepoprawne znaki! \nPoprawne hasło składa się wyłącznie z liter i cyfr.");
                return;
            }

            if (imie_tb.Text.Length == 0) {
                MessageBox.Show("Pole imię nie może być puste!");
                return;
            }

            if (!Regex.IsMatch(imie_tb.Text, @"^[a-zA-Z]*$")) {
                MessageBox.Show("Imię zawiera niepoprawne znaki! \nPoprawne imię składa się wyłącznie z liter.");
                return;
            }

            if (nazwisko_tb.Text.Length == 0) {
                MessageBox.Show("Pole nazwisko nie może być puste!");
                return;
            }

            if (!Regex.IsMatch(nazwisko_tb.Text, @"^[a-zA-Z]*$")) {
                MessageBox.Show("Nazwisko zawiera niepoprawne znaki!\nPoprawne nazwisko składa się wyłącznie z liter.");
                return;
            }

            if (email_tb.Text.Length == 0) {
                MessageBox.Show("Pole e-mail nie może być puste!");
                return;
            }

            if (!Regex.IsMatch(email_tb.Text, @"^[a-zA-Z0-9._]+@[a-zA-Z0-9._]+$")) {
                MessageBox.Show("E-mail jest niepoprawny!");
                return;
            }

            if (!Regex.IsMatch(telefon_tb.Text, @"^[0-9-]*$")) {
                MessageBox.Show("Numer telefonu ma niepoprawny format! \nPowinien składać się wyłącznie z cyfr i '-'");
                return;
            }

            if (!Regex.IsMatch(Adres_tb.Text, @"^[a-zA-Z0-9.,-/ ]*$")) {
                MessageBox.Show("Adres jest niepoprawny!\nPowinien składać się wyłącznie z liter, cyfr i znaków - / . ,");
                return;
            }

            string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                    "localhost", "5432", "rejestrator", "ndijo1s81a4", "bazapizzerii");
            NpgsqlConnection pgConnection = new NpgsqlConnection(connstring);

            try {
                pgConnection.Open();

                NpgsqlCommand pgCommand = new NpgsqlCommand("SELECT * FROM uzytkownik WHERE login = @login;", pgConnection);
                pgCommand.Parameters.AddWithValue("@login", login_tb.Text);
                if (pgCommand.ExecuteReader().Read() == true) {
                    MessageBox.Show("Użytkownik o podanym loginie już istnieje!");
                    pgConnection.Close();
                    return;
                }

                //tworzymy nową, unikalną wartość identyfikatora dla użytkownika
                pgCommand = new NpgsqlCommand("SELECT nextval('osoba_id_osoba_seq')", pgConnection);
                int id = Convert.ToInt32(pgCommand.ExecuteScalar());

                using (NpgsqlTransaction pgTransaction = (NpgsqlTransaction)pgConnection.BeginTransaction()) {
                    try {

                        pgCommand = new NpgsqlCommand("INSERT INTO osoba VALUES(@id, @imie, @nazwisko, @adres, @email, @telefon);" +
                                                        "INSERT INTO uzytkownik VALUES (@id, @uzytkownik, @rola);",
                                                        pgConnection, pgTransaction);

                        // przygotowanie danych do wprowadzenia do bazy
                        pgCommand.Parameters.AddWithValue("@rola", rola_cb.Text == "Klient" ? "klient" : "wlasciciel_pizzerii");
                        pgCommand.Parameters.AddWithValue("@id", id);
                        pgCommand.Parameters.AddWithValue("@uzytkownik", login_tb.Text);
                        pgCommand.Parameters.AddWithValue("@imie", imie_tb.Text);
                        pgCommand.Parameters.AddWithValue("@nazwisko", nazwisko_tb.Text);
                        pgCommand.Parameters.AddWithValue("@email", email_tb.Text);
                        pgCommand.Parameters.AddWithValue("@telefon", telefon_tb.Text.Length > 0 ? telefon_tb.Text : null);
                        pgCommand.Parameters.AddWithValue("@adres", Adres_tb.Text.Length > 0 ? Adres_tb.Text : null);

                        pgCommand.ExecuteNonQuery();

                        pgCommand = new NpgsqlCommand("CREATE USER " + login_tb.Text +
                                " IN GROUP " + (rola_cb.Text == "Klient" ? "klient" : "wlasciciel_pizzerii") +
                                " PASSWORD '" + password1_pb.Password + "';",
                                pgConnection, pgTransaction);
                        pgCommand.ExecuteNonQuery();

                        pgTransaction.Commit();
                    } catch (Exception ex) {
                        //Transaction rolled back to the original state
                        pgTransaction.Rollback();
                        throw new Exception("Transaction rolled back! " + ex.ToString());
                    }
                }

                pgConnection.Close();

                MessageBox.Show("Zarejestrowałeś się w bazie pizzerii.\nMożesz się teraz zalogować.");
                OpenLoginPage();
            } catch (Exception msg) {
                pgConnection.Close();

                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas rejestracji nowego użytkownika!");
            }


        }

        private void OpenLoginPage() {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }
    }
}
