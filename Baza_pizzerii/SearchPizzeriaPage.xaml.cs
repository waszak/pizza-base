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
using System.Data;
using System.ComponentModel;

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for SearchPizzeriaPage.xaml
    /// </summary>
    public partial class SearchPizzeriaPage : Page {
        public SearchPizzeriaPage() {
            InitializeComponent();
            IntializeCity();
            GridView gridview = (GridView)((ListView)this.Pizzeria_listView).View;
            GridViewColumn column = gridview.Columns[0];
            ((System.ComponentModel.INotifyPropertyChanged)column).PropertyChanged += (sender, e) => {
                if (e.PropertyName == "ActualWidth") {
                    column.Width = 0;
                }
            };
            
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
        private void myAccount_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Properties["rola"].ToString() == "gosc")
            {
                MessageBox.Show("Korzystasz z aplikacji jako gość.\nFunkcjonalność dostępna dla zalogowanych użytkowników.");
                return;
            }
            var userAccountWindow = new UserAccountWindow();
            userAccountWindow.Show();
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
        private void searchPizzeria_Click(object sender, RoutedEventArgs e) {
            this.Pizzeria_listView.Items.Clear();
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"])) {
                string sql = "SELECT id_pizzeria, nazwa, miasto" +
                                    " FROM pizzeria " +
                                    "WHERE (miasto like @miasto and (ulica like @ulicaFormat1 or ulica like @ulicaFormat2))"+
                                        "or miasto like @miasto";

                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@miasto", City_comboBox.Text);
                query.Parameters.AddWithValue("@ulicaFormat1", pizzeriaAddress_TextBox.Text);
                query.Parameters.AddWithValue("@ulicaFormat2", City_comboBox.Text);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    Pizzeria p = new Pizzeria();
                    p.Id = reader.GetInt32(0).ToString();
                    p.name = reader.GetString(1);
                    p.city = reader.GetString(2);
                    this.Pizzeria_listView.Items.Add(p);
                }
            }
        }

        private void selectPizzeria(object sender, MouseButtonEventArgs e) {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as Pizzeria;
            if (item != null) {
                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new PizzeriaPage(item.Id));
            }
        }
        
         class City {

             public string name {
                 get;
                 set;
             }
        }
        class Pizzeria {
            public string Id {
                get;
                set;
            }
            public string name {
                get;
                set;
            }
            public string city {
                get;
                set;
            }
        }


     
    }

}










