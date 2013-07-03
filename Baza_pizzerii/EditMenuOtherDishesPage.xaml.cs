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
    /// Interaction logic for EditMenuOtherDishesPage.xaml
    /// </summary>
    public partial class EditMenuOtherDishesPage : Page
    {
        public EditMenuOtherDishesPage()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());

            NpgsqlDataAdapter pgDataAdapter1 = new NpgsqlDataAdapter();
            pgDataAdapter1.SelectCommand = new NpgsqlCommand("SELECT id_produkt, nazwa FROM inny_produkt WHERE rodzaj = 'danie' ORDER BY 2;", conn);
            DataSet ds1 = new DataSet();
            pgDataAdapter1.Fill(ds1);
            allOtherDshes.DataContext = ds1.Tables[0].DefaultView;


            NpgsqlDataAdapter pgDataAdapter2 = new NpgsqlDataAdapter();
            pgDataAdapter2.SelectCommand = new NpgsqlCommand("SELECT id_oferta_inny_produkt, nazwa, cena FROM oferta_inny_produkt JOIN inny_produkt USING (id_produkt)" +
                                                                " WHERE rodzaj = 'danie' AND id_pizzeria = " + App.Current.Properties["id_pizzeria"].ToString() + " ORDER BY 2;",
                                                                conn);
            DataSet ds2 = new DataSet();
            pgDataAdapter2.Fill(ds2);
            myOtherDishes.DataContext = ds2.Tables[0].DefaultView;

            conn.Close(); conn.ClearPool();

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

        private void ChoosePizzeria_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new PizzeriaManagementPage());
        }


        private void DeleteOtherDish_Click(object sender, RoutedEventArgs e)
        {
            int selected = myOtherDishes.SelectedIndex;
            if (selected < 0)
            {
                MessageBox.Show("Nie wybrałeś żadnego produktu!");
                return;
            }
            DataRowView r = (DataRowView)myOtherDishes.Items[selected];

            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            string sql = "DELETE FROM oferta_inny_produkt WHERE id_oferta_inny_produkt = " + r[0].ToString();

            NpgsqlCommand command = new NpgsqlCommand(sql, conn);
            try
            {
                command.ExecuteNonQuery();
                conn.Close(); conn.ClearPool();

                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new EditMenuOtherDishesPage());
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

        private void NewProduct_Click(object sender, RoutedEventArgs e)
        {
            if (NewProduct_tb.Text.Length == 0)
            {
                MessageBox.Show("Nie określiłeś nazwy nowego produktu!");
                return;
            }
            string msg;
            if (!Validate.OnlyLetters(NewProduct_tb.Text, "", out msg))
            {
                MessageBox.Show("Niepoprawna nazwa nowego produktu!");
                return;
            }

            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            string sql = "INSERT INTO inny_produkt (nazwa, rodzaj) VALUES ('" + NewProduct_tb.Text + "', 'danie');";

            NpgsqlCommand command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
            conn.Close();

            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuOtherDishesPage());
        }

        private void AddOtherDish_Click(object sender, RoutedEventArgs e)
        {
            string err_msg;
            if (!Validate.RealNumber(Price_tb.Text, out err_msg))
            {
                MessageBox.Show("Cena ma niepoprawny format (DDD.DD)");
                return;
            }
            string Price = Price_tb.Text;
            Price = Price.Replace('.', ',');

            int selected = allOtherDshes.SelectedIndex;
            if (selected < 0)
            {
                MessageBox.Show("Nie wybrałeś żadnego produktu!");
                return;
            }
            DataRowView r = (DataRowView)allOtherDshes.Items[selected];

            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            string sql = "INSERT INTO  oferta_inny_produkt (id_produkt, id_pizzeria, cena) VALUES (@id_produkt, @id_pizzeria, @cena);";

            NpgsqlCommand command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("@id_produkt", (int)r[0]);
            command.Parameters.AddWithValue("@id_pizzeria", App.Current.Properties["id_pizzeria"]);
            command.Parameters.AddWithValue("@cena", Double.Parse(Price));
            command.Prepare();

            try
            {
                command.ExecuteNonQuery();
                conn.Close(); conn.ClearPool();

                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new EditMenuOtherDishesPage());
            }
            catch (Exception msg)
            {
                conn.Close(); conn.ClearPool();
                #if DEBUG
                MessageBox.Show(msg.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas aktualizacji menu");
            }

            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuOtherDishesPage());
        }



    }
}
