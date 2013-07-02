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
            IntializePizzas();
            //Pierwsza kolumna jest ukryta.
            GridView gridview = (GridView)((ListView)this.Pizza_ListView).View;
            GridViewColumn column = gridview.Columns[0];
            ((System.ComponentModel.INotifyPropertyChanged)column).PropertyChanged += (sender, e) => {
                if (e.PropertyName == "ActualWidth") {
                    column.Width = 0;
                }
            };

        }
        private void IntializePizzas() {
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"])) {

                string sql = "SELECT id_pizza, pizza.nazwa, array_to_string(array_agg(skladnik.nazwa), ', '), oferta_pizza.wielkosc, oferta_pizza.cena " +
                                    "FROM pizzeria join oferta_pizza using(id_pizzeria) join pizza using(id_pizza)" +
                                          "join sklad using(id_pizza) join skladnik using(id_skladnik)" +
                                    " WHERE id_pizzeria = @id" +
                                    " GROUP BY id_pizza, pizza.nazwa, oferta_pizza.wielkosc, oferta_pizza.cena ORDER BY pizza.nazwa,oferta_pizza.wielkosc;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@id", this.pizzeria_id);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    Pizza pizza = new Pizza(reader.GetInt32(0).ToString(), reader.GetString(1));
                    pizza.pizza_ingredients = reader.GetString(2);
                    pizza.pizza_size = (!reader.IsDBNull(3) ? reader.GetInt32(3) : (int?)null);
                    pizza.pizza_price = (!reader.IsDBNull(4) ? reader.GetFloat(4) : (float?)null);
                    ListViewItem item = new ListViewItem();
                    this.Pizza_ListView.Items.Add(pizza);
                }
            }
        }

        private void IntializeLabels() {
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"])) {
                string sql = "SELECT id_pizzeria, nazwa, miasto, ulica, telefon, www, ocena, liczba_ocen " +
                                    "FROM pizzeria join laczna_ocena using(id_pizzeria) " +
                                    "WHERE id_pizzeria = @id;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@id", this.pizzeria_id);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    string name = reader.GetString(1);
                    string adress = reader.GetString(2) + " " + reader.GetString(3);
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

            }
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

    public class Pizza {
        public Pizza(string id_pizza, string pizza_name) {
            this.id_pizza = id_pizza;
            this.pizza_name = pizza_name;
        }
        public string id_pizza {
            get;
            set;
        }
        public string pizza_name {
            get;
            set;
        }
        public string pizza_ingredients {
            get;
            set;
        }
        public int? pizza_size {
            get;
            set;
        }

        public float? pizza_price {
            get;
            set;
        }
    }

}
