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

    public partial class SearchPizzaPage : SearchPizzeriaBase {
        public SearchPizzaPage() {
            InitializeComponent();
            InitializeCity ( this.City_comboBox );
            InitializeIngredients ( this.menuIngredients );
            GridView gridview = ( GridView ) ( ( ListView ) this.Pizza_listView ).View;
            hideColumn ( gridview.Columns[0] );
        }

        protected void InitializeIngredients ( MenuItem menuIngridients ) {
            ObservableCollection<Ingredient> ingredients = new ObservableCollection<Ingredient>();
            using ( Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB() ) {
                string sql = "SELECT DISTINCT nazwa" +
                             " FROM skladnik order by 1;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand ( sql, conn );
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();

                while ( reader.Read() ) {
                    Ingredient i = new Ingredient();
                    i.Name = reader.GetString ( 0 );
                    i.IsSelected = false;
                    ingredients.Add ( i );
                }
            }
            menuIngredients.ItemsSource = ingredients;
        }


        private void searchPizza_Click ( object sender, RoutedEventArgs e ) {
            this.Pizza_listView.Items.Clear();
            using ( Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB() ) {
                if ( pizzeriaAddress_TextBox.Text.Trim() == "Wprowadź adres pizzerii" ) {
                    pizzeriaAddress_TextBox.Text = "";
                }

                string adress = pizzeriaAddress_TextBox.Text.Trim();
                string sql = "SELECT distinct id_pizzeria, pizzeria.nazwa, miasto, ulica, pizza.nazwa, array_to_string(array_agg(skladnik.nazwa), ', ')" +
                             " FROM pizzeria join oferta_pizza using(id_pizzeria) join pizza using(id_pizza)" +
                             "join sklad using(id_pizza) join skladnik using(id_skladnik)" +
                             " WHERE " + ( adress != ""
                                           ? "(miasto like @miasto and (ulica like @ulicaFormat1 or ulica like @ulicaFormat2))"
                                           : "miasto like @miasto" ) +
                             " GROUP BY id_pizzeria, pizzeria.nazwa, miasto, ulica, pizza.nazwa, id_oferta_pizza ORDER BY pizza.nazwa,ulica;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand ( sql, conn );
                query.Parameters.AddWithValue ( "@miasto", City_comboBox.Text );

                if ( adress != "" ) {
                    //Adress format łukasza 47/4
                    query.Parameters.AddWithValue ( "@ulicaFormat1", "%" + CultureInfo.CurrentCulture.TextInfo.ToLower ( adress ) + "%" );
                    //Adress format Łukasza 47/4
                    adress = CultureInfo.CurrentCulture.TextInfo.ToUpper ( adress.Substring ( 0, 1 ) ) + CultureInfo.CurrentCulture.TextInfo.ToLower ( adress.Substring ( 1 ) );
                    query.Parameters.AddWithValue ( "@ulicaFormat2", "%" + adress + "%" );
                }

                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();

                while ( reader.Read() ) {
                    PizzeriaPizza p = new PizzeriaPizza();
                    p.Id = reader.GetInt32 ( 0 ).ToString();
                    p.name = reader.GetString ( 1 );
                    p.city = reader.GetString ( 2 );
                    p.adress = reader.GetString ( 3 );
                    p.name_pizza = reader.GetString ( 4 );
                    p.ingridients = reader.GetString ( 5 );

                    if ( matchIngridients ( p.ingridients ) ) {
                        this.Pizza_listView.Items.Add ( p );
                    }
                }
            }
        }

        private bool matchIngridients ( string ingridients ) {
            foreach ( object item in menuIngredients.Items ) {
                if ( ( ( Ingredient ) item ).IsSelected ) {
                    if ( !ingridients.Contains ( ( ( Ingredient ) item ).Name ) ) {
                        return false;
                    }
                }
            }

            return true;
        }

        class PizzeriaPizza : Pizzeria {

            public string ingridients {
                get;
                set;
            }
            public string name_pizza {
                get;
                set;
            }

        }

        class Ingredient {
            public bool IsSelected {
                get;
                set;
            }
            public string Name {
                get;
                set;
            }
        }
    }
}
