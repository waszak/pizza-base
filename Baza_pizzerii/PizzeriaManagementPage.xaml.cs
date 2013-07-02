using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    /// Interaction logic for PizzeriaManagementPage.xaml
    /// </summary>
    public partial class PizzeriaManagementPage : Page
    {
        public PizzeriaManagementPage()
        {
            InitializeComponent();
            InitializeData(); 
        }

        private void InitializeData() 
        {
            NpgsqlDataAdapter pgDataAdapter = new NpgsqlDataAdapter();
            pgDataAdapter.SelectCommand = new NpgsqlCommand("SELECT id_pizzeria, nazwa, miasto, ulica FROM pizzeria WHERE wlasciciel = " + App.Current.Properties["id_osoba"], 
                                                            DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString()));

            DataSet ds = new DataSet();
            pgDataAdapter.Fill(ds);

            Pizzerie.DataContext = ds.Tables[0].DefaultView;
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

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }

        private void DeletePizzeria_Click(object sender, RoutedEventArgs e)
        {
            int selected = Pizzerie.SelectedIndex;
            if (selected < 0)
            {
                MessageBox.Show("Nie wybrałeś żadnej pizzerii!");
                return;
            }
            DataRowView  r = (DataRowView)Pizzerie.Items[selected];

            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            string sql = "DELETE FROM pizzeria WHERE id_pizzeria = " + r[0].ToString();

            NpgsqlCommand command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            Refresh();

        }

        private void Refresh()
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new PizzeriaManagementPage());
        }

        private void EditPizzeria_Click(object sender, RoutedEventArgs e)
        {
            int selected = Pizzerie.SelectedIndex;
            if (selected < 0)
            {
                MessageBox.Show("Nie wybrałeś żadnej pizzerii!");
                return;
            }

            DataRowView  r = (DataRowView)Pizzerie.Items[selected];
            App.Current.Properties["id_pizzeria"] = r[0].ToString();

            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditPizzeria());
        }

        private void AddNewPizzeria_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new AddPizzeriaPage());
        }

        private void EditMenu_Click(object sender, RoutedEventArgs e)
        {
            int selected = Pizzerie.SelectedIndex;
            if (selected < 0)
            {
                MessageBox.Show("Nie wybrałeś żadnej pizzerii!");
                return;
            }

            DataRowView r = (DataRowView)Pizzerie.Items[selected];
            App.Current.Properties["id_pizzeria"] = r[0].ToString();

            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuPage());

        }
    }
}
