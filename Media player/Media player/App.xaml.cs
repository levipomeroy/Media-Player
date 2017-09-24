using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Media_player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1) //make sure an argument is passed
            {
                FileInfo file = new FileInfo(e.Args[0]);
                if (file.Exists) //make sure it's actually a file
                {
                    this.Properties["ArbitraryArgName"] = e.Args[0];
                    //((MainWindow)System.Windows.Application.Current.MainWindow).MediaView.Source = new Uri(this.Properties["ArbitraryArgName"].ToString());
                }
            }
        }
    }


}
