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
using Npgsql;

namespace Baza_pizzerii
{
    /// <summary>
    /// Interaction logic for UserAccountWindow.xaml
    /// </summary>
    public partial class UserAccountWindow : Window
    {
        public UserAccountWindow()
        {
            InitializeComponent();
            PreparePage();
        }

        private void PreparePage()
        {
            PasswordArea.Visibility = System.Windows.Visibility.Hidden;
            Login_tb.IsReadOnly = true;
            Name_tb.IsReadOnly = true;
            Surname_tb.IsReadOnly = true;
            Address_tb.IsReadOnly = true;
            Email_tb.IsReadOnly = true;
            Phone_tb.IsReadOnly = true;
            Accept_bt.IsEnabled = false;

            try
            {
                NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
                string sql = "SELECT imie, nazwisko, adres, mail, telefon " +
                                "FROM uzytkownik JOIN osoba USING (id_osoba) " +
                                "WHERE id_osoba = " +  App.Current.Properties["id_osoba"].ToString() + ";";

                NpgsqlCommand command = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dr = command.ExecuteReader();
                if (dr.Read() == false)
                {
                    MessageBox.Show("Wystąpił błąd podczas łączenia z bazą!");
                    conn.Close();
                    return;
                }
                Name_tb.Text = dr[0].ToString();
                Surname_tb.Text = dr[1].ToString();
                Address_tb.Text = dr[2].ToString();
                Email_tb.Text = dr[3].ToString();
                Phone_tb.Text = dr[4].ToString();
                Login_tb.Text = App.Current.Properties["login"].ToString();
                pass1_pb.Password = pass2_pb.Password = App.Current.Properties["password"].ToString();
                conn.Close();
            }
            catch (Exception msg)
            {   
                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas łączenia się z bazą!");
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            PasswordArea.Visibility = System.Windows.Visibility.Visible;
            Name_tb.IsReadOnly = false;
            Surname_tb.IsReadOnly = false;
            Address_tb.IsReadOnly = false;
            Email_tb.IsReadOnly = false;
            Phone_tb.IsReadOnly = false;
            Accept_bt.IsEnabled = true;
            Edit_bt.IsEnabled = false;
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            string return_msg;


            if (!Validate.Passwords(pass1_pb.Password, pass2_pb.Password, out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (Name_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole imię nie może być puste!");
                return;
            }

            if (!Validate.OnlyLetters(Name_tb.Text, "Imię", out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (Surname_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole nazwisko nie może być puste!");
                return;
            }

            if (!Validate.OnlyLetters(Surname_tb.Text, "Nazwisko", out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (!Validate.Email(Email_tb.Text, out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (Phone_tb.Text.Length > 0 && (!Validate.PhoneNumber(Phone_tb.Text, out return_msg)))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (Address_tb.Text.Length > 0 && (!Validate.Address(Address_tb.Text, out return_msg)))
            {
                MessageBox.Show(return_msg);
                return;
            }

            NpgsqlConnection pgConnection = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            try
            {
                using (NpgsqlTransaction pgTransaction = (NpgsqlTransaction)pgConnection.BeginTransaction())
                {
                    try
                    {
                        
                        NpgsqlCommand pgCommand = new NpgsqlCommand("UPDATE osoba SET "+
                                                                    "imie = @imie, "+
                                                                    "nazwisko = @nazwisko, "+
                                                                    "adres = @adres, "+
                                                                    "mail = @mail, "+
                                                                    "telefon = @telefon "+
                                                                    "WHERE id_osoba = @id_osoba;",
                                                                    pgConnection, pgTransaction);

                        // przygotowanie danych do wprowadzenia do bazy
                        pgCommand.Parameters.AddWithValue("@id_osoba", App.Current.Properties["id_osoba"].ToString());
                        pgCommand.Parameters.AddWithValue("@imie", Name_tb.Text);
                        pgCommand.Parameters.AddWithValue("@nazwisko", Surname_tb.Text);
                        pgCommand.Parameters.AddWithValue("@mail", Email_tb.Text);
                        pgCommand.Parameters.AddWithValue("@telefon", Phone_tb.Text.Length > 0 ? Phone_tb.Text : null);
                        pgCommand.Parameters.AddWithValue("@adres", Address_tb.Text.Length > 0 ? Address_tb.Text : null);
                        pgCommand.ExecuteNonQuery();
                        
                        pgCommand = new NpgsqlCommand(  "ALTER USER " + App.Current.Properties["login"].ToString() +
                                                        " WITH PASSWORD '" + pass1_pb.Password + "';",
                                                        pgConnection, pgTransaction);
                        pgCommand.ExecuteNonQuery();

                        pgTransaction.Commit();
                        App.Current.Properties["password"] = pass1_pb.Password;
                    }
                    catch (Exception ex)
                    {
                        //Transaction rolled back to the original state
                        pgTransaction.Rollback();
                        throw new Exception("Transaction rolled back! " + ex.ToString());
                    }
                }

                pgConnection.Close();

                MessageBox.Show("Dane Twojego konta zostały zaktualizowane!");
                this.Close();
            }
            catch (Exception msg)
            {
                pgConnection.Close();

                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas rejestracji nowego użytkownika!");
            }
        }
            

    }
}
