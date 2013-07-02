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
    /// Interaction logic for AddPizzeriaPage.xaml
    /// </summary>
    public partial class AddPizzeriaPage : Page
    {
        public AddPizzeriaPage()
        {
            InitializeComponent();
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
                        //tworzymy nową, unikalną wartość identyfikatora dla pizzerii
                        NpgsqlCommand pgCommand = new NpgsqlCommand("SELECT nextval('pizzeria_id_pizzeria_seq')", pgConnection);
                        int id = Convert.ToInt32(pgCommand.ExecuteScalar());

                        pgCommand = new NpgsqlCommand("INSERT INTO  pizzeria VALUES (@id_pizzeria, @nazwa, @miasto, @ulica, @telefon, @www, @id_wlasciciel);",
                                                        pgConnection, pgTransaction);

                        // przygotowanie danych do wprowadzenia do bazy
                        pgCommand.Parameters.AddWithValue("@id_wlasciciel", (int)App.Current.Properties["id_osoba"]);
                        pgCommand.Parameters.AddWithValue("@id_pizzeria", id);
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

                MessageBox.Show("Nowa pizzeria została dodana!");

                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new PizzeriaManagementPage());
            }
            catch (Exception msg)
            {
                pgConnection.Close();

                #if DEBUG
                MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas dodawania pizzerii!");
            }
        }
    }
}
