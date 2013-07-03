using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Baza_pizzerii {
    public partial class EditMenuBase:Page {
        protected void myAccount_Click(object sender, RoutedEventArgs e)
        {
            var userAccountWindow = new UserAccountWindow();
            userAccountWindow.Show();
        }

        protected void logout_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }

        protected void EditMenuPizza_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuPizzaPage());
        }

        protected void EditMenuOtherDishes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuOtherDishesPage());
        }

        protected void EditMenuDrinks_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuDrinksPage());
        }

        protected void EditMenuAlkohols_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuAlkoholPage());
        }

        protected void EditMenuAdditions_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuAdditionPage());
        }

        protected void ChoosePizzeria_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new PizzeriaManagementPage());
        }

        protected void hideColumn(GridViewColumn column) {
            column.Width = 0;
            ((System.ComponentModel.INotifyPropertyChanged)column).PropertyChanged += (sender, e) => {
                if (e.PropertyName == "ActualWidth") {
                    column.Width = 0;
                }
            };
        }

    }
}
