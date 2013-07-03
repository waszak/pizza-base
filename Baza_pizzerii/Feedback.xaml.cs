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
using System.Windows.Shapes;

namespace Baza_pizzerii {
    /// <summary>
    /// Interaction logic for Feedback.xaml
    /// </summary>
    public partial class Feedback : Window {
        string pizzeria_id;
        public Feedback(string pizzeria_id) {
            InitializeComponent();
            this.pizzeria_id = pizzeria_id;
        }

        private void save_Click(object sender, RoutedEventArgs e) {
            if (textBox1.Text.Trim() == "") {
                MessageBox.Show("Komentarz niemoże być pusty!");
                return;
            }
            int id_opinia;
            using (Npgsql.NpgsqlConnection conn = DB.loginUserToDB("rejestrator", "ndijo1s81a4") ) {
                 Npgsql.NpgsqlCommand pgCommand = new Npgsql.NpgsqlCommand("SELECT nextval('osoba_id_osoba_seq')", conn);
                id_opinia = Convert.ToInt32(pgCommand.ExecuteScalar());
            }
            using (Npgsql.NpgsqlConnection conn = DB.loginAppUserToDB()) {
                string sql = "INSERT into opinia values(@id_opinia, @id_pizzeria, @id_pizza, @wystawil, @komentarz, @ocena, @wartosc_oceny, @liczba_ocen);";
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand(sql, conn);
                query.Parameters.AddWithValue("@id_opinia", id_opinia);
                query.Parameters.AddWithValue("@id_pizzeria", this.pizzeria_id);
                query.Parameters.AddWithValue("@id_pizza", null);
                query.Parameters.AddWithValue("@wystawil", App.Current.Properties["id_osoba"]);
                query.Parameters.AddWithValue("@komentarz", this.textBox1.Text);
                query.Parameters.AddWithValue("@ocena", this.comboBox1.Text);
                query.Parameters.AddWithValue("@wartosc_oceny", 0);
                query.Parameters.AddWithValue("@liczba_ocen", 0);
                query.Prepare();
                query.ExecuteNonQuery();
            }
            this.Close();
        }
    }
}
