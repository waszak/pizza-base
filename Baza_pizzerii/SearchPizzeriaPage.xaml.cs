﻿using System;
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

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for SearchPizzeriaPage.xaml
    /// </summary>
    public partial class SearchPizzeriaPage : Page {
        public SearchPizzeriaPage() {
            InitializeComponent();
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
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"])) {
                string sql = "SELECT id_pizzeria, nazwa, miasto, ulica, telefon, www " +
                                    "FROM pizzeria " +
                                    "WHERE miasto like @miasto;";

                adapter = new Npgsql.NpgsqlDataAdapter(sql, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@miasto", City_comboBox.Text);
                dataTable = new DataTable();
                adapter.Fill(dataTable);
                Pizzeria_dataGrid.ItemsSource = dataTable.DefaultView;
            }
        }
        private void selectPizzeria(object sender, RoutedEventArgs e) {
            DataRowView row = (DataRowView)Pizzeria_dataGrid.SelectedItems[0];
            int x = ((int)((DataRowView)(Pizzeria_dataGrid.SelectedItems[0])).Row[0]);
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new PizzeriaPage(((int)row.Row[0]).ToString()));
        }

        Npgsql.NpgsqlDataAdapter adapter = null;
        DataTable dataTable = null;



    }

}










