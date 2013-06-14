using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;

namespace Baza_pizzerii
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        void App_Startup(object sender, StartupEventArgs e)
        {
           
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            // przy wyjściu z aplikacji zamykamy połączenie z bazą
            if (App.Current.Properties.Contains("TemporaryConnection"))
            {
                NpgsqlConnection conn = (NpgsqlConnection)App.Current.Properties["TemporaryConnection"];
                conn.Close();
                App.Current.Properties.Remove("TemporaryConnection");
            }
        }
    
    }

}
