using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

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

            if (!Regex.IsMatch(login_tb.Text, @"^[a-zA-Z][a-zA-Z0-9]*$"))
            {
                MessageBox.Show("Login zawiera niepoprawne znaki! \nW poprawnym loginie pierwszy znak jest literą, a reszta znaków literą lub cyfrą.");
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
            
            if (!Regex.IsMatch(password1_pb.Password, @"^[a-zA-Z0-9]*$"))
            {
                MessageBox.Show("Hasło zawiera niepoprawne znaki! \nPoprawne hasło składa się wyłącznie z liter i cyfr.");
                return;
            }
            
            if (imie_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole imię nie może być puste!");
                return;
            }

            if (!Regex.IsMatch(imie_tb.Text, @"^[a-zA-Z]*$"))
            {
                MessageBox.Show("Imię zawiera niepoprawne znaki! \nPoprawne imię składa się wyłącznie z liter.");
                return;
            }

            if (nazwisko_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole nazwisko nie może być puste!");
                return;
            }

            if (!Regex.IsMatch(nazwisko_tb.Text, @"^[a-zA-Z]*$"))
            {
                MessageBox.Show("Nazwisko zawiera niepoprawne znaki!\nPoprawne nazwisko składa się wyłącznie z liter.");
                return;
            }

            if (email_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole e-mail nie może być puste!");
                return;
            }

            if (!Regex.IsMatch(email_tb.Text, @"^[a-zA-Z0-9._]+@[a-zA-Z0-9._]+$"))
            {
                MessageBox.Show("E-mail jest niepoprawny!");
                return;
            }

            if (!Regex.IsMatch(telefon_tb.Text, @"^[0-9-]*$"))
            {
                MessageBox.Show("Numer telefonu ma niepoprawny format! \nPowinien składać się wyłącznie z cyfr i '-'");
                return;
            }

            if (!Regex.IsMatch(Adres_tb.Text, @"^[a-zA-Z0-9.,-/ ]*$"))
            {
                MessageBox.Show("Adres jest niepoprawny!\nPowinien składać się wyłącznie z liter, cyfr i znaków - / . ,");
                return;
            }
            
            try
            {
                string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                                    "localhost", "5432", "rejestrator", "ndijo1s81a4", "bazapizzerii");

                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();

                //sprawdzamy, czy dany login nie jest zarezerwowany
                string login = "'" + login_tb.Text + "'";
                string sql = "SELECT * FROM uzytkownik WHERE login = " + login + ";";
               
                NpgsqlCommand command = new NpgsqlCommand(sql, conn);
                if (command.ExecuteReader().Read() == true)
                {
                    MessageBox.Show("Użytkownik o podanym loginie już istnieje!");
                    conn.Close();
                    return;
                }

                //tworzymy nową, unikalną wartość identyfikatora dla użytkownika
                sql = "SELECT nextval('osoba_id_osoba_seq')";
                command = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dr = command.ExecuteReader();

                if (dr.Read() == false)
                    throw new Exception("Nie mozna pobrać max id z bazy danych!");

                // przygotowanie danych do wprowadzenia do bazy
                string id = dr[0].ToString(); 
                dr.Close();
                string rola = rola_cb.Text == "Klient" ? "'klient'" : "'wlasciciel_pizzerii'";
                string imie = "'" + imie_tb.Text + "'";
                string nazwisko = "'" + nazwisko_tb.Text + "'";
                string email = "'" + email_tb.Text + "'";
                string telefon = telefon_tb.Text.Length > 0 ? "'" + telefon_tb.Text + "'" : "NULL";
                string adres = Adres_tb.Text.Length > 0 ? "'" + Adres_tb.Text + "'" : "NULL";

                sql =   "INSERT INTO osoba VALUES (" + id + "," + imie + "," + nazwisko + "," + adres + "," + email + "," + telefon +");" +
                        "INSERT INTO uzytkownik VALUES (" + id + "," + login + "," + rola +");";

                command = new NpgsqlCommand(sql, conn);
                if (command.ExecuteNonQuery() != 2)
                    throw new Exception("Insert error!");

                rola = rola_cb.Text == "Klient" ? "klient" : "wlasciciel_pizzerii";
                sql = "CREATE USER " + login_tb.Text + " IN GROUP " + rola + " PASSWORD '" + password1_pb.Password + "';";
               
                command = new NpgsqlCommand(sql, conn);
                command.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Zarejestrowałeś się w bazie pizzerii.\nMożesz się teraz zalogować.");
                var loginWindow = new LoginWindow();
                loginWindow.Top = this.Top;
                loginWindow.Left = this.Left;
                loginWindow.Show();
                this.Close();
            }
            catch (Exception msg)
            {
                #if DEBUG
                MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas rejestracji nowego użytkownika!");
            }


        }
    }
}
