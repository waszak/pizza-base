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

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for SearchPizzaPage.xaml
    /// </summary>
     class Ingredient {
        public Ingredient() {
        }

        public string Name { get; set; }
    }
    public partial class SearchPizzaPage : Page {
        public SearchPizzaPage() {
            InitializeComponent();
            ObservableCollection<Ingredient> ingredients = new ObservableCollection<Ingredient>
                                               {
                                                   new Ingredient {Name = "szynka"},
                                                   new Ingredient {Name = "pieczarki"},
                                                   new Ingredient {Name = "gyros"},
                                                   new Ingredient {Name = "papryka"}
                                               };
            this.menuIngredients.ItemsSource = ingredients;
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


    }
}
