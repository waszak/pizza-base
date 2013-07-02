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

namespace Baza_pizzerii
{
    /// <summary>
    /// Interaction logic for EditPizzeria.xaml
    /// </summary>
    public partial class EditPizzeria : Page
    {
        public EditPizzeria()
        {
            InitializeComponent();
            PreparePage();
        }

        private void PreparePage()
        {
            try
            {
                NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
                string sql = "SELECT nazwa, miasto, ulica, telefon, www " +
                                "FROM pizzeria " +
                                "WHERE id_pizzeria = " + App.Current.Properties["id_pizzeria"].ToString();

                NpgsqlCommand command = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader dr = command.ExecuteReader();
                if (dr.Read() == false)
                {
                    MessageBox.Show("Wystąpił błąd podczas łączenia z bazą!");
                    conn.Close();
                    return;
                }
                Name_tb.Text = dr[0].ToString();
                City_tb.Text = dr[1].ToString();
                Street_tb.Text = dr[2].ToString();
                Phone_tb.Text = dr[3].ToString();
                WWW_tb.Text = dr[4].ToString();
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new PizzeriaManagementPage());
        }


        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            string return_msg;

            if (Name_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole nazwa nie może być puste!");
                return;
            }

            if (!Validate.AlphaNumericSpace(Name_tb.Text, "Nazwa", out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (City_tb.Text.Length == 0)
            {
                MessageBox.Show("Pole miasto nie może być puste!");
                return;
            }

            if (!Validate.OnlyLetters(City_tb.Text, "Miasto", out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (Phone_tb.Text.Length > 0 && (!Validate.PhoneNumber(Phone_tb.Text, out return_msg)))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (!Validate.Address(Street_tb.Text, out return_msg))
            {
                MessageBox.Show(return_msg);
                return;
            }

            if (WWW_tb.Text.Length > 0 && (!Validate.WWW(WWW_tb.Text, out return_msg)))
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

                        NpgsqlCommand pgCommand = new NpgsqlCommand("UPDATE pizzeria SET " +
                                                                    "nazwa = @nazwa, " +
                                                                    "miasto = @miasto, " +
                                                                    "ulica = @ulica, " +
                                                                    "www = @www, " +
                                                                    "telefon = @telefon " +
                                                                    "WHERE id_pizzeria = @id_pizzeria;",
                                                                    pgConnection, pgTransaction);

                        // przygotowanie danych do wprowadzenia do bazy
                        pgCommand.Parameters.AddWithValue("@id_pizzeria", App.Current.Properties["id_pizzeria"].ToString());
                        pgCommand.Parameters.AddWithValue("@nazwa", Name_tb.Text);
                        pgCommand.Parameters.AddWithValue("@miasto", City_tb.Text);
                        pgCommand.Parameters.AddWithValue("@ulica", Street_tb.Text);
                        pgCommand.Parameters.AddWithValue("@telefon", Phone_tb.Text.Length > 0 ? Phone_tb.Text : null);
                        pgCommand.Parameters.AddWithValue("@www", WWW_tb.Text.Length > 0 ? WWW_tb.Text : null);
                        pgCommand.ExecuteNonQuery();

                        pgTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        //Transaction rolled back to the original state
                        pgTransaction.Rollback();
                        throw new Exception("Transaction rolled back! " + ex.ToString());
                    }
                }

                pgConnection.Close();

                MessageBox.Show("Dane Twojej pizzerii zostały zaktualizowane!");

                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new PizzeriaManagementPage());
            }
            catch (Exception msg)
            {
                pgConnection.Close();

                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas aktualizacji danych!");
            }
        }

        private void myAccount_Click(object sender, RoutedEventArgs e)
        {
            var userAccountWindow = new UserAccountWindow();
            userAccountWindow.Show();
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }
    }
}
