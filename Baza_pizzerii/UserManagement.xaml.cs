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
    /// Interaction logic for UserManagement.xaml
    /// </summary>
    public partial class UserManagement : Page
    {

        ObservableCollection<User> _allUsersSet = new ObservableCollection<User>();

        public ObservableCollection<User> allUsersSet { get { return _allUsersSet; } }

        public class User
        {
            public string Id { get; set; }
            public string Login { get; set; }
            public string Rola { get; set; }
            public string Imie { get; set; }
            public string Nazwisko { get; set; }
            public string Adres { get; set; }
            public string Mail { get; set; }
            public string Telefon { get; set; }
            public string LiczbaPizzerii { get; set; }

        }

        public UserManagement()
        {
            InitializeData();
            InitializeComponent();
        }

        private void InitializeData()
        {
            NpgsqlConnection conn = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            NpgsqlCommand cmd = new NpgsqlCommand(  "SELECT u.id_osoba, u.login, u.rola, o.imie, o.nazwisko, o.adres, o.mail, o.telefon, COUNT(p.id_pizzeria) "+
                                                    "FROM uzytkownik u LEFT JOIN osoba o USING (id_osoba) LEFT JOIN pizzeria p ON (u.id_osoba = p.wlasciciel) "+
                                                    "GROUP BY u.id_osoba, u.login, u.rola, o.imie, o.nazwisko, o.adres, o.mail, o.telefon;", conn);

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
                DataTable dt = dr.GetSchemaTable();
                DataRowCollection schemarows = dt.Rows;

                _allUsersSet.Add(new User{ 
                                            Id      = dr[0].ToString(), 
                                            Login   = dr[1].ToString(), 
                                            Rola    = dr[2].ToString(), 
                                            Imie    = dr[3].ToString(), 
                                            Nazwisko= dr[4].ToString(),
                                            Adres   = dr[5].ToString(), 
                                            Mail    = dr[6].ToString(), 
                                            Telefon = dr[7].ToString(),
                                            LiczbaPizzerii = dr[2].ToString() == "wlasciciel_pizzerii" ? dr[8].ToString() : "-"
                                         });
            }

            conn.Close(); conn.ClearPool();
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new LoginPage());
        }

     
        private void deleteUser_Click(object sender, RoutedEventArgs e)
        {
            int selected = allUsers.SelectedIndex;
            if (selected < 0)
            {
                return;
            }
            User u = (User)allUsers.Items[selected];

            if (u.Rola != "klient" && u.Rola != "wlasciciel_pizzerii")
            {
                MessageBox.Show("Nie można usunąć użytkownika!\nMożna usuwać jedynie konta klientów i właścicieli pizzerii");
                return;
            }

            int user_id = Int32.Parse(u.Id);
            string user_login = u.Login;

            NpgsqlConnection pgConnection = DB.loginUserToDB(App.Current.Properties["login"].ToString(), App.Current.Properties["password"].ToString());
            try
            {
                using (NpgsqlTransaction pgTransaction = (NpgsqlTransaction)pgConnection.BeginTransaction())
                {
                    try
                    {
                        NpgsqlCommand pgCommand = new NpgsqlCommand(    "DELETE FROM osoba WHERE id_osoba = @id_osoba;",
                                                                        pgConnection, pgTransaction);

                        pgCommand.Parameters.AddWithValue("@id_osoba", user_id);
                        pgCommand.ExecuteNonQuery();

                        pgCommand = new NpgsqlCommand("DROP USER " + user_login,
                                                        pgConnection, pgTransaction);
                        pgCommand.ExecuteNonQuery();

                        pgTransaction.Commit();
                        allUsersSet.Remove(u);
                    }
                    catch (Exception ex)
                    {
                        //Transaction rolled back to the original state
                        pgTransaction.Rollback();
                        throw new Exception("Transaction rolled back! " + ex.ToString());
                    }
                }
                pgConnection.Close();

                MessageBox.Show("Usunięto użytkownika");
            }
            catch (Exception ex)
            {
                pgConnection.Close();

                #if DEBUG
                    MessageBox.Show(ex.ToString());
                #endif
                MessageBox.Show("Wystąpił błąd podczas usuwania użytkownika!");
            }

        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new UserManagement());
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}