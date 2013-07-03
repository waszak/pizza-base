using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Baza_pizzerii {
    public partial class SearchPizzeriaBase:Page {

        protected void searchPizzeriaPage_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new SearchPizzeriaPage());
        }

        protected void searchPizzaPage_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new SearchPizzaPage());
        }

        protected void logout_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }

        protected void myAccount_Click(object sender, RoutedEventArgs e) {
            if (App.Current.Properties["rola"].ToString() == "gosc") {
                MessageBox.Show("Korzystasz z aplikacji jako gość.\nFunkcjonalność dostępna dla zalogowanych użytkowników.");
                return;
            }
            var userAccountWindow = new UserAccountWindow();
            userAccountWindow.Show();
        }

        protected void hideColumn(GridViewColumn column) {
            column.Width = 0;
            ((System.ComponentModel.INotifyPropertyChanged)column).PropertyChanged += (sender, e) => {
                if (e.PropertyName == "ActualWidth") {
                    column.Width = 0;
                }
            };
        }

        protected void IntializeCity(ComboBox City_comboBox) {
            ObservableCollection<City> cities = new ObservableCollection<City>();
            using (Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB()) {
                string sql = "SELECT DISTINCT miasto" +
                                    " FROM pizzeria order by 1;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    City p = new City();
                    p.name = reader.GetString(0);
                    cities.Add(p);
                }
            }
            City_comboBox.ItemsSource = cities;
        }

        protected void select(object sender, MouseButtonEventArgs e) {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as Pizzeria;
            if (item != null) {
                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new PizzeriaPage(item.Id));
            }
        }

        protected class City {
            public string name {
                get;
                set;
            }
        }

        protected class Pizzeria {
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
            public string adress {
                get;
                set;
            }
        }


    }


}
