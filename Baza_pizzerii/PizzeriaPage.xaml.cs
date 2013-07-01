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

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for PizzeriaPage.xaml
    /// </summary>
    
    public partial class PizzeriaPage : Page {
        private string pizzeria_id;
        public PizzeriaPage(string id) {
            InitializeComponent();
            this.pizzeria_id = id;
            IntializeLabels();
        }
        private void IntializeLabels(){
            Npgsql.NpgsqlConnection conn = (Npgsql.NpgsqlConnection)App.Current.Properties["Connection"];
            string sql = "SELECT id_pizzeria, nazwa, miasto, ulica, telefon, www, ocena, liczba_ocen " +
                                "FROM pizzeria join laczna_ocena using(id_pizzeria) " +
                                "WHERE id_pizzeria = @id;";
            conn.Open();
            Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
            query.Parameters.AddWithValue("@id", this.pizzeria_id);
            query.Prepare();
            Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
            if (reader.Read()) {
                string name = reader.GetString(1);
                string adress = reader.GetString(2)+" " + reader.GetString(3);
                string phone = reader.GetString(4);
                string www = reader.GetString(5);
                float ocena = reader.GetFloat(6);
                int liczba_ocen = reader.GetInt32(7);
                this.name_label.Content = name;
                this.adress_label.Content = adress;
                this.phone_label.Content = phone;
                this.www_label.Content = www;
                this.grade_label.Content = String.Format("{0:F2} z {1}", ocena, liczba_ocen);
                
               
            }

            conn.Close();
        }
        private void searchPizzeriaPage_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new SearchPizzeriaPage());
        }

        private void searchPizzaPage_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new SearchPizzaPage());
        }
        private void logout_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }

        private void myAccount_Click(object sender, RoutedEventArgs e) {
            if (App.Current.Properties["rola"].ToString() == "gosc") {
                MessageBox.Show("Korzystasz z aplikacji jako gość.\nFunkcjonalność dostępna dla zalogowanych użytkowników.");
                return;
            }
            if (App.Current.Properties["rola"].ToString() == "klient") {
                //testowo na razie tylko wyswietlamy wszystko globalne zmienne
                MessageBox.Show("Login: " + App.Current.Properties["login"].ToString() +
                                "\nId_osoba: " + App.Current.Properties["id_osoba"].ToString() +
                                "\nRola: " + App.Current.Properties["rola"].ToString() +
                                "\nImie: " + App.Current.Properties["imie"].ToString() +
                                "\nNazwisko: " + App.Current.Properties["nazwisko"].ToString());
                return;
            }
        }
    }



}
