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
using System.ComponentModel;

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for PizzeriaPage.xaml
    /// </summary>

    public partial class PizzeriaPage : Page {
        private string pizzeria_id;
        public PizzeriaPage(string id) {
            InitializeComponent();
            this.pizzeria_id = id;
            IntializeLabels();
            IntializePizzas();
            IntializeMeals();
            IntializeDrinks();
            IntializeExtra();
            IntializeAlkohol();
            IntializeFeedback();
            //Pierwsza kolumna jest ukryta.
            GridView gridview = (GridView)((ListView)this.Pizza_ListView).View;
            GridViewColumn column = gridview.Columns[0];
            ((System.ComponentModel.INotifyPropertyChanged)column).PropertyChanged += (sender, e) => {
                if (e.PropertyName == "ActualWidth") {
                    column.Width = 0;
                }
            };
            if (App.Current.Properties["rola"].ToString() == "gosc") {
                //this.tabItem69.Visibility = Visibility.Hidden;
                this.tabItem69.IsEnabled = false;
            }
        }
        private void Review(object sender, EventArgs args) {
            if (App.Current.Properties["rola"].ToString() == "gosc") {
                MessageBox.Show("Korzystasz z aplikacji jako gość.\nFunkcjonalność dostępna dla zalogowanych użytkowników.");
                return;
            }
            Window x = new Feedback(this.pizzeria_id);
            x.ShowDialog();
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.Navigate(new PizzeriaPage(this.pizzeria_id));
        }
        public void UpdateFeedback(string id_feedback, int val) {
            using (Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB()) {
                string sql = "UPDATE opinia" +
                                " SET wartosc_oceny = wartosc_oceny+@inc" +
                                " WHERE id_opinia = @id;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@id", id_feedback);
                query.Parameters.AddWithValue("@inc", (float)val);
                query.Prepare();
                query.ExecuteNonQuery();
            }
        }

        class A : INotifyPropertyChanged {
            PizzeriaPage outerClass;
            public A(PizzeriaPage outerclass) {
                upVoteCommand = new DelegateCommand(OnUpVoteCommand);
                downVoteCommand = new DelegateCommand(OnDownVoteCommand);
                this.outerClass = outerclass;
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName) {
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            #endregion

            public string id_feedback {
                get;
                set;
            }
            public string feedback {
                get;
                set;
            }
            private bool assigned = false;
            private int _grade_value;
            public int grade_value {
                get {
                    return _grade_value;
                }
                set {
                    int val = value;
                    _grade_value += val;
                    if(assigned) this.outerClass.UpdateFeedback(id_feedback, val);
                    assigned = true;
                    OnPropertyChanged("grade_value");
                }
            }
            public int grade {
                get;
                set;
            }

            private ICommand _upVoteCommand;
            private ICommand _downVoteCommand;
            private int increase = 0;
            public ICommand upVoteCommand {
                get {
                    return _upVoteCommand;
                }
                set {
                    _upVoteCommand = value;
                }
            }
            public ICommand downVoteCommand {
                get {
                    return _downVoteCommand;
                }
                set {
                    _downVoteCommand = value;
                }
            }
            void OnUpVoteCommand(object aParameter) {
                if (increase < 1) {
                    increase += 1;
                    grade_value = 1;
                }
            }
            void OnDownVoteCommand(object aParameter) {
                if (increase > -1) {
                    increase -= 1;
                    grade_value = -1;
                }
            }

        }
        private void IntializeFeedback() {
            if (App.Current.Properties["rola"].ToString() == "gosc") {
                return;
            }
            FeedbackPizerria_ListView.Items.Clear();
            using (Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB()) {
                string sql = "SELECT id_opinia, id_pizzeria, id_pizza, imie||' '||nazwisko, komentarz, ocena, wartosc_oceny" +
                                        " FROM opinia join osoba on(wystawil=id_osoba)"
                                        + " WHERE id_pizzeria = @id;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@id", this.pizzeria_id);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    A a = new A(this);
                    a.id_feedback = reader.GetInt32(0).ToString();
                    a.feedback = "Ocena: " + reader.GetInt32(5) + "\n" + reader.GetString(4)
                        + "\nWystawił: " + reader.GetString(3);
                    a.grade_value = (int)reader.GetFloat(6);
                    FeedbackPizerria_ListView.Items.Add(a);
                }

            }
        }
        private void otherProductQuery(string rodzaj, Npgsql.NpgsqlConnection conn, out Npgsql.NpgsqlCommand query) {
            string sql = "SELECT inny_produkt.nazwa, cena" +
                                   " FROM pizzeria join oferta_inny_produkt using(id_pizzeria) join inny_produkt using(id_produkt)" +
                                   " WHERE id_pizzeria = @id and inny_produkt.rodzaj = @rodzaj" +
                                   " GROUP BY id_produkt, inny_produkt.nazwa,cena ORDER BY inny_produkt.nazwa;";
            query = new Npgsql.NpgsqlCommand(sql, conn);
            query.Parameters.AddWithValue("@id", this.pizzeria_id);
            query.Parameters.AddWithValue("@rodzaj", rodzaj);
            query.Prepare();
        }

        private void IntializeProduct(ListView list, string rodzaj) {
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB((string)App.Current.Properties["login"], (string)App.Current.Properties["password"])) {
                Npgsql.NpgsqlCommand query;
                otherProductQuery(rodzaj, conn, out query);
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    Product p = new Product();
                    p.name = reader.GetString(0);
                    p.price = (!reader.IsDBNull(1) ? reader.GetFloat(1) : (float?)null);
                    list.Items.Add(p);
                }
            }
        }

        private void IntializeAlkohol() {
            IntializeProduct(this.Alkohol_ListView, "alkohol");
        }
        private void IntializeExtra() {
            IntializeProduct(this.Extra_ListView, "dodatek");
        }
        private void IntializeDrinks() {
            IntializeProduct(this.Drinks_ListView, "napoj");
        }
        private void IntializeMeals() {
            IntializeProduct(this.OtherMeals_ListView, "danie");
        }

        private void IntializePizzas() {
            using (Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB()) {

                string sql = "SELECT id_pizza, pizza.nazwa, array_to_string(array_agg(skladnik.nazwa), ', '), oferta_pizza.wielkosc, oferta_pizza.cena " +
                                    "FROM pizzeria join oferta_pizza using(id_pizzeria) join pizza using(id_pizza)" +
                                          "join sklad using(id_pizza) join skladnik using(id_skladnik)" +
                                    " WHERE id_pizzeria = @id" +
                                    " GROUP BY id_pizza, pizza.nazwa, oferta_pizza.wielkosc, oferta_pizza.cena ORDER BY pizza.nazwa,oferta_pizza.wielkosc;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@id", this.pizzeria_id);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    Pizza pizza = new Pizza(reader.GetInt32(0).ToString(), reader.GetString(1));
                    pizza.pizza_ingredients = reader.GetString(2);
                    pizza.pizza_size = (!reader.IsDBNull(3) ? reader.GetInt32(3) : (int?)null);
                    pizza.price = (!reader.IsDBNull(4) ? reader.GetFloat(4) : (float?)null);
                    ListViewItem item = new ListViewItem();
                    this.Pizza_ListView.Items.Add(pizza);
                }
            }
        }

        private void IntializeLabels() {
            using (Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB()) {
                string sql = "SELECT id_pizzeria, nazwa, miasto, ulica, telefon, www, ocena, liczba_ocen " +
                                    "FROM pizzeria join laczna_ocena using(id_pizzeria) " +
                                    "WHERE id_pizzeria = @id;";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@id", this.pizzeria_id);
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    string name = reader.GetString(1);
                    string adress = reader.GetString(2) + " " + reader.GetString(3);
                    string phone = (reader.IsDBNull(4) ? "" : reader.GetString(4));
                    string www = (reader.IsDBNull(5) ? "" : reader.GetString(5));
                    float ocena = reader.GetFloat(6);
                    int liczba_ocen = reader.GetInt32(7);
                    this.name_label.Content = name;
                    this.adress_label.Content = adress;
                    this.phone_label.Content = phone;
                    this.www_label.Content = www;
                    this.grade_label.Content = String.Format("{0:F2} z {1}", ocena, liczba_ocen);


                }

            }
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

        private void myAccount_Click(object sender, RoutedEventArgs e) {
            if (App.Current.Properties["rola"].ToString() == "gosc") {
                MessageBox.Show("Korzystasz z aplikacji jako gość.\nFunkcjonalność dostępna dla zalogowanych użytkowników.");
                return;
            }
            if (App.Current.Properties["rola"].ToString() == "klient") {
                //testowo na razie tylko wyswietlamy wszystko globalne zmienne
                MessageBox.Show("Login: " + App.Current.Properties["login"].ToString() +
                                "\nId_osoba: " + App.Current.Properties["id_osoba"].ToString() +
                                "\nRola: " + App.Current.Properties["rola"].ToString() +
                                "\nImie: " + App.Current.Properties["imie"].ToString() +
                                "\nNazwisko: " + App.Current.Properties["nazwisko"].ToString());
                return;
            }
        }

        void Pizza_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as Pizza;
            if (item != null) {
                /*Window x = new Feedback();
                x.ShowDialog();*/
            }
        }
    }
    public class Product {
        public string name {
            get;
            set;
        }
        public float? price {
            get;
            set;
        }
    }

    public class Pizza : Product {
        public Pizza(string id_pizza, string pizza_name) {
            this.id_pizza = id_pizza;
            this.name = pizza_name;
        }
        public string id_pizza {
            get;
            set;
        }

        public string pizza_ingredients {
            get;
            set;
        }
        public int? pizza_size {
            get;
            set;
        }


    }

}
