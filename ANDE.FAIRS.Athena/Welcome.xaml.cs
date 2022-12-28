using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ANDE.FAIRS.Athena
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Page
    {
        public Welcome()
        {
            InitializeComponent();
            //versionLabel.Content = string.Format("Version {0}", ConfigurationManager.AppSettings["Version"]);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainGrid.Width = e.NewSize.Width;
            mainGrid.RowDefinitions[0].Height = new GridLength(e.NewSize.Height * (400.0 / 600.0));
            mainGrid.RowDefinitions[1].Height = new GridLength(e.NewSize.Height * (75.0 / 600.0));
            mainGrid.RowDefinitions[1].Height = new GridLength(e.NewSize.Height - (mainGrid.RowDefinitions[0].Height.Value + mainGrid.RowDefinitions[1].Height.Value));
        }
    }
}
