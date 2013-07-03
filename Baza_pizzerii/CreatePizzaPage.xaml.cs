using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for CreatePizzaPage.xaml
    /// </summary>
    /// 

    public class Ingred
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }


    public partial class CreatePizzaPage : Page
    {
        ObservableCollection<Ingred> _allIngredientsSet = new ObservableCollection<Ingred>();
        ObservableCollection<Ingred> _myIngredientsSet = new ObservableCollection<Ingred>();

        public ObservableCollection<Ingred> allIngredientsSet { get { return _allIngredientsSet; } }
        public ObservableCollection<Ingred> myIngredientsSet { get { return _myIngredientsSet; } }

        public CreatePizzaPage()
        {
            InitializeData();
            InitializeComponent();
        }


        private void InitializeData()
        {
            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM skladnik ORDER BY 2;", conn);

            NpgsqlDataReader dr;
            try
            {
                dr = cmd.ExecuteReader();
            }
            catch (NpgsqlException ex)
            {
                #if DEBUG
                    MessageBox.Show(ex.ToString());
                #endif

                MessageBox.Show("Błąd podczas łączenia z bazą danych!");
                return;
            }

            while(dr.Read())
            {
                int n = dr.FieldCount;
                DataTable dt = dr.GetSchemaTable();
                DataRowCollection schemarows = dt.Rows;

                allIngredientsSet.Add(new Ingred { Id = dr[0].ToString(), Name = dr[1].ToString() });
            }

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

        private void rightMove_Click(object sender, RoutedEventArgs e)
        {
            int selected = allIngredients.SelectedIndex;
            if (selected < 0)
            {
                return;
            }
            Ingred r = (Ingred)allIngredients.Items[selected];
            myIngredientsSet.Add(r);
            allIngredientsSet.Remove(r);
        }

        private void leftMove_Click(object sender, RoutedEventArgs e)
        {
            int selected = myIngredients.SelectedIndex;
            if (selected < 0)
            {
                return;
            }
            Ingred r = (Ingred)myIngredients.Items[selected];
            allIngredientsSet.Add(r);
            myIngredientsSet.Remove(r);
        }

        private void AddPizza_Click(object sender, RoutedEventArgs e)
        {
            if (myIngredientsSet.Count == 0)
            {
                MessageBox.Show("Wybierz przynajmniej jeden składnik pizzy!");
                return;
            }
            if (PizzaName_tb.Text.Length == 0)
            {
                MessageBox.Show("Wprowadź nazwę pizzy!");
                return;
            }
            string msg;
            if (!Validate.OnlyLetters(PizzaName_tb.Text,"Nazwa pizzy", out msg))
            {
                MessageBox.Show(msg);
                return;
            }

            NpgsqlConnection pgConnection = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString()); 
            try
            {
     
                //tworzymy nową, unikalną wartość identyfikatora dla pizzy
                NpgsqlCommand pgCommand = new NpgsqlCommand("SELECT nextval('pizza_id_pizza_seq')", pgConnection);
                int id = Convert.ToInt32(pgCommand.ExecuteScalar());

                using (NpgsqlTransaction pgTransaction = (NpgsqlTransaction)pgConnection.BeginTransaction())
                {
                    try
                    {
                        pgCommand = new NpgsqlCommand("INSERT INTO pizza VALUES(@id_pizza, @nazwa);",
                                                        pgConnection, pgTransaction);

                        pgCommand.Parameters.AddWithValue("@id_pizza", id);
                        pgCommand.Parameters.AddWithValue("@nazwa", PizzaName_tb.Text);
                        pgCommand.ExecuteNonQuery();

                        foreach (Ingred ingredient in myIngredientsSet)
                        {
                            pgCommand = new NpgsqlCommand("INSERT INTO sklad VALUES(@id_pizza, @id_skladnik);",
                                                           pgConnection, pgTransaction);

                            pgCommand.Parameters.AddWithValue("@id_pizza", id);
                            pgCommand.Parameters.AddWithValue("@id_skladnik", ingredient.Id);
                            pgCommand.ExecuteNonQuery();
                        }

                        pgTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        //Transaction rolled back to the original state
                        pgTransaction.Rollback();
                        throw new Exception("Transaction rolled back! " + ex.ToString());
                    }
                }
                pgConnection.Close();

                MessageBox.Show("Pizza została dodana do bazy.");

                this.NavigationService.RemoveBackEntry();
                this.NavigationService.Navigate(new EditMenuPizzaPage());
            }
            catch (Exception ex)
            {
                pgConnection.Close();

                #if DEBUG
                     MessageBox.Show(ex.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas dodawania nowej pizzy!");
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new EditMenuPizzaPage());
        }

    }
}
