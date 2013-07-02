using System;
using System.Collections.Generic;
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
    /// Interaction logic for EditMenuPizzaPage.xaml
    /// </summary>
    public partial class EditMenuPizzaPage : Page
    {
        public EditMenuPizzaPage()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            NpgsqlDataAdapter pgDataAdapter1 = new NpgsqlDataAdapter();
            pgDataAdapter1.SelectCommand = new NpgsqlCommand(   "SELECT pizza.id_pizza AS id, "+
                                                                "pizza.nazwa AS nazwa_pizzy, "+
                                                                "array_to_string(array_agg(skladnik.nazwa), ', ') AS skladniki "+
                                                                "FROM pizza JOIN sklad USING (id_pizza) JOIN skladnik USING (id_skladnik) "+
                                                                "GROUP BY pizza.nazwa, pizza.id_pizza",
                                                                DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString()));
            DataSet ds1 = new DataSet();
            pgDataAdapter1.Fill(ds1);
            allPizzas.DataContext = ds1.Tables[0].DefaultView;

            NpgsqlDataAdapter pgDataAdapter2 = new NpgsqlDataAdapter();
            pgDataAdapter2.SelectCommand = new NpgsqlCommand(   "SELECT pizza.id_pizza AS id, " +
                                                                "pizza.nazwa AS nazwa_pizzy, " +
                                                                "array_to_string(array_agg(skladnik.nazwa), ', ') AS skladniki, " +
                                                                "oferta_pizza.cena AS cena_pizzy, "+
                                                                "oferta_pizza.wielkosc AS wielkosc_pizzy "+
                                                                "FROM oferta_pizza JOIN pizza using (id_pizza) JOIN sklad USING (id_pizza) JOIN skladnik USING (id_skladnik) " +
                                                                "GROUP BY id_pizzeria, pizza.nazwa, pizza.id_pizza, oferta_pizza.cena, oferta_pizza.wielkosc " +
                                                                "HAVING id_pizzeria = "+ App.Current.Properties["id_pizzeria"].ToString(),
                                                                DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString()));
            DataSet ds2 = new DataSet();
            pgDataAdapter2.Fill(ds2);
            myPizzas.DataContext = ds2.Tables[0].DefaultView;

        }

        private void myAccount_Click(object sender, RoutedEventArgs e)
        {
            var userAccountWindow = new UserAccountWindow();
            userAccountWindow.Show();
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }

        private void EditMenuPizza_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuPizzaPage());
        }

        private void EditMenuOtherDishes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuOtherDishesPage());
        }

        private void EditMenuDrinks_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuDrinksPage());
        }

        private void EditMenuAlkohols_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuAlkoholPage());
        }

        private void EditMenuAdditions_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuAdditionPage());
        }


        private void DeletePizza_Click(object sender, RoutedEventArgs e)
        {

            int selected = myPizzas.SelectedIndex;
            if (selected < 0)
            {
                MessageBox.Show("Nie wybrałeś żadnego produktu!");
                return;
            }
            DataRowView r = (DataRowView)myPizzas.Items[selected];

            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            string sql = "DELETE FROM oferta_pizza WHERE id_oferta_pizza = " + r[0].ToString();

            NpgsqlCommand command = new NpgsqlCommand(sql, conn);
            try
            {
                command.ExecuteNonQuery();
                conn.Close(); conn.ClearPool();

                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new EditMenuPizzaPage());
            }
            catch (Exception msg)
            {
                conn.Close(); conn.ClearPool();
                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas aktualizacji menu");
            }
        
        }

        private void AddPizza_Click(object sender, RoutedEventArgs e)
        {
            string err_msg;
            if (!Validate.RealNumber(Price_tb.Text, out err_msg))
            {
                MessageBox.Show("Cena ma niepoprawny format (DDD.DD)");
                return;
            }
            string Price = Price_tb.Text;
            Price = Price.Replace('.', ',');

            if (!Validate.OnlyNumeric(Size_tb.Text,"", out err_msg))
            {
                MessageBox.Show("Rozmiar powinien być liczbą całkowitą!");
                return;
            }

            int selected = allPizzas.SelectedIndex;
            if (selected < 0)
            {
                MessageBox.Show("Nie wybrałeś żadnego produktu!");
                return;
            }
            DataRowView r = (DataRowView)allPizzas.Items[selected];

            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            string sql = "INSERT INTO  oferta_pizza (id_pizza, id_pizzeria, cena, wielkosc) VALUES (@id_pizza, @id_pizzeria, @cena, @wielkosc);";

            NpgsqlCommand command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("@id_pizza", (int)r[0]);
            command.Parameters.AddWithValue("@id_pizzeria", App.Current.Properties["id_pizzeria"]);
            command.Parameters.AddWithValue("@cena", Double.Parse(Price));
            command.Parameters.AddWithValue("@wielkosc", Int32.Parse(Size_tb.Text));
            command.Prepare();

            try
            {
                command.ExecuteNonQuery();
                conn.Close(); conn.ClearPool();

                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new EditMenuPizzaPage());
            }
            catch (Exception msg)
            {
                conn.Close(); conn.ClearPool();
                #if DEBUG
                    MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas aktualizacji menu");
            }
        }

        private void CreateNewPizza_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuPizzaPage());
        }
        
    }
}
