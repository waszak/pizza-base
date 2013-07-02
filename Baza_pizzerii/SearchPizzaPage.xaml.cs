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
using System.Collections.ObjectModel;
using System.Globalization;

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for SearchPizzaPage.xaml
    /// </summary>
     class Ingredient {
        public Ingredient() {
        }
        public bool IsSelected { get; set; }
        public string Name { get; set; }
  
    }
    public partial class SearchPizzaPage : Page {
        public SearchPizzaPage() {
            InitializeComponent();
            IntializeCity();
            IntializeIngredients();

            GridView gridview = (GridView)((ListView)this.Pizza_listView).View;
            GridViewColumn column = gridview.Columns[0];
            ((System.ComponentModel.INotifyPropertyChanged)column).PropertyChanged += (sender, e) => {
                if (e.PropertyName == "ActualWidth") {
                    column.Width = 0;
                }
            };
            
        }

        private void IntializeIngredients() {
            ObservableCollection<Ingredient> ingredients = new ObservableCollection<Ingredient>();
            
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"])) {
                string sql = "SELECT DISTINCT nazwa" +
                                    " FROM skladnik order by 1;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    Ingredient i = new Ingredient();
                    i.Name = reader.GetString(0);
                    i.IsSelected = false;
                    ingredients.Add(i);
                }
            }
            this.menuIngredients.ItemsSource = ingredients;
        }

        private void IntializeCity() {
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"])) {
                string sql = "SELECT DISTINCT miasto" +
                                    " FROM pizzeria order by 1;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    City p = new City();
                    p.name = reader.GetString(0);
                    this.City_comboBox.Items.Add(p);
                }
            }
        }
        private void myAccount_Click(object sender, RoutedEventArgs e) {
            if (App.Current.Properties["rola"].ToString() == "gosc")
            {
                MessageBox.Show("Korzystasz z aplikacji jako gość.\nFunkcjonalność dostępna dla zalogowanych użytkowników.");
                return;
            }
            var userAccountWindow = new UserAccountWindow();
            userAccountWindow.Show();
        }

        private void searchPizza_Click(object sender, RoutedEventArgs e) {
            this.Pizza_listView.Items.Clear();
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"])) {
                if (pizzeriaAddress_TextBox.Text.Trim() == "Wprowadź adres pizzerii") pizzeriaAddress_TextBox.Text = "";
                string sql = "SELECT id_pizzeria, pizzeria.nazwa, miasto, ulica, pizza.nazwa, array_to_string(array_agg(skladnik.nazwa), ', ')" +
                                    " FROM pizzeria join oferta_pizza using(id_pizzeria) join pizza using(id_pizza)" +
                                                    "join sklad using(id_pizza) join skladnik using(id_skladnik)" +
                                    " WHERE " + (pizzeriaAddress_TextBox.Text.Trim() != ""
                                                        ? "(miasto like @miasto and (ulica like @ulicaFormat1 or ulica like @ulicaFormat2))"
                                                        : "miasto like @miasto") +
                                    " GROUP BY id_pizzeria, pizzeria.nazwa, miasto, ulica, pizza.nazwa ORDER BY pizza.nazwa,ulica;";

                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@miasto", City_comboBox.Text);
                if (pizzeriaAddress_TextBox.Text.Trim() != "") {
                    string adress = pizzeriaAddress_TextBox.Text.Trim();
                    //Adress format łukasza 47/4 
                    query.Parameters.AddWithValue("@ulicaFormat1", "%" + CultureInfo.CurrentCulture.TextInfo.ToLower(adress) + "%");
                    //Adress format Łukasza 47/4
                    adress = CultureInfo.CurrentCulture.TextInfo.ToUpper(adress.Substring(0, 1)) + CultureInfo.CurrentCulture.TextInfo.ToLower(adress.Substring(1));
                    query.Parameters.AddWithValue("@ulicaFormat2", "%" + adress + "%");
                }
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    PizzeriaPizza p = new PizzeriaPizza();
                    p.Id = reader.GetInt32(0).ToString();
                    p.name = reader.GetString(1);
                    p.city = reader.GetString(2);
                    p.adress = reader.GetString(3);
                    p.name_pizza = reader.GetString(4);
                    p.ingridients = reader.GetString(5);
                    
                    if(matchIngridients(p.ingridients))this.Pizza_listView.Items.Add(p);
                }
            }
        }

        private bool matchIngridients(string ingridients) {
 
            foreach (object item in menuIngredients.Items) {
                if (((Ingredient)item).IsSelected) {
                    if (!ingridients.Contains(((Ingredient)item).Name)) return false;
                }

            }
            return true;
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

        private void selectPizza(object sender, MouseButtonEventArgs e) {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as PizzeriaPizza;
            if (item != null) {
                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new PizzeriaPage(item.Id));
            }
        }

        class PizzeriaPizza {
            public string Id {
                get;
                set;
            }
            public string ingridients {
                get;
                set;
            }
            public string name {
                get;
                set;
            }
            public string name_pizza {
                get;
                set;
            }
            public string city {
                get;
                set;
            }
            public string adress {
                get;
                set;
            }
        }
    

        class City {

            public string name {
                get;
                set;
            }
        }

    }
}
