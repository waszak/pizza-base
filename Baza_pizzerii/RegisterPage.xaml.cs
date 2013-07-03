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

            string return_msg;
            
            if (! Validate.Login(login_tb.Text, out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }
            
            if (! Validate.Passwords(password1_pb.Password, password2_pb.Password, out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (imie_tb.Text.Length == 0) {
                MessageBox.Show("Pole imię nie może być puste!");
                return;
            }

            if (! Validate.OnlyLetters(imie_tb.Text, "Imię", out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (nazwisko_tb.Text.Length == 0) {
                MessageBox.Show("Pole nazwisko nie może być puste!");
                return;
            }

            if (! Validate.OnlyLetters(nazwisko_tb.Text, "Nazwisko", out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (! Validate.Email(email_tb.Text, out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (telefon_tb.Text.Length > 0 && (!Validate.PhoneNumber(telefon_tb.Text, out return_msg)))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (Adres_tb.Text.Length > 0 && (!Validate.Address(Adres_tb.Text, out return_msg)))
            {
                MessageBox.Show(return_msg);
                return;
            }

            NpgsqlConnection pgConnection = DB.loginUserToDB("rejestrator", "ndijo1s81a4");
            try {
                NpgsqlCommand pgCommand = new NpgsqlCommand("SELECT * FROM uzytkownik WHERE login = @login;", pgConnection);
                pgCommand.Parameters.AddWithValue("@login", login_tb.Text.ToLower());
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
                        pgCommand.Parameters.AddWithValue("@uzytkownik", login_tb.Text.ToLower());
                        pgCommand.Parameters.AddWithValue("@imie", imie_tb.Text);
                        pgCommand.Parameters.AddWithValue("@nazwisko", nazwisko_tb.Text);
                        pgCommand.Parameters.AddWithValue("@email", email_tb.Text);
                        pgCommand.Parameters.AddWithValue("@telefon", telefon_tb.Text.Length > 0 ? telefon_tb.Text : null);
                        pgCommand.Parameters.AddWithValue("@adres", Adres_tb.Text.Length > 0 ? Adres_tb.Text : null);

                        pgCommand.ExecuteNonQuery();

                        pgCommand = new NpgsqlCommand("CREATE USER " + login_tb.Text.ToLower() +
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
