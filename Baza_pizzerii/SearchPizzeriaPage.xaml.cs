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
using System.Data;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for SearchPizzeriaPage.xaml
    /// </summary>
    public partial class SearchPizzeriaPage : SearchPizzeriaBase {
        public SearchPizzeriaPage() {
            InitializeComponent();
            InitializeCity(this.City_comboBox);
            GridView gridview = (GridView)((ListView)this.Pizzeria_listView).View;
            hideColumn(gridview.Columns[0]);
        }

        private void searchPizzeria_Click(object sender, RoutedEventArgs e) {
            this.Pizzeria_listView.Items.Clear();
            using (Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB()) {

                if (pizzeriaAddress_TextBox.Text.Trim() == "Wprowadź adres pizzerii") pizzeriaAddress_TextBox.Text = "";

                string adress = pizzeriaAddress_TextBox.Text.Trim();
                string sql = "SELECT id_pizzeria, nazwa, miasto, ulica" +
                                    " FROM pizzeria" +
                                    " WHERE " + (adress != ""
                                                        ? "(miasto like @miasto and (ulica like @ulicaFormat1 or ulica like @ulicaFormat2))"
                                                        : "miasto like @miasto");

                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@miasto", City_comboBox.Text);
                if (adress != "") {
                    //Adress format łukasza 47/4 
                    query.Parameters.AddWithValue("@ulicaFormat1", "%" + CultureInfo.CurrentCulture.TextInfo.ToLower(adress) + "%");
                    //Adress format Łukasza 47/4
                    adress = CultureInfo.CurrentCulture.TextInfo.ToUpper(adress.Substring(0, 1)) + CultureInfo.CurrentCulture.TextInfo.ToLower(adress.Substring(1));
                    query.Parameters.AddWithValue("@ulicaFormat2", "%" + adress + "%");
                }
                query.Prepare();
                Npgsql.NpgsqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) {
                    Pizzeria p = new Pizzeria();
                    p.Id = reader.GetInt32(0).ToString();
                    p.name = reader.GetString(1);
                    p.city = reader.GetString(2);
                    p.adress = reader.GetString(3);
                    this.Pizzeria_listView.Items.Add(p);
                }
            }
        }



    }

}










