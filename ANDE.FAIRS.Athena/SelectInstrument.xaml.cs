using System;
using System.Collections.Generic;
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
using System.Text.RegularExpressions;

namespace ANDE.FAIRS.Athena
{
    /// <summary>
    /// Interaction logic for SelectInstrument.xaml
    /// </summary>
    public partial class SelectInstrument : Page
    {
        public SelectInstrument()
        {
            InitializeComponent();
        }

        private void SelectClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(txtInstrument.Text.Trim()))
            {
                errLabel.Content = "Please enter Instrument Name.";
                return;
            }
            var re = new Regex(@"[Ii]\d{4}");
            if (re.IsMatch(txtInstrument.Text.Trim()))
            {
                NavigationService.Navigate(new SelectChipType(txtInstrument.Text.Trim()));
            }
            else
            {
                errLabel.Content = "Invalid Instrument Name.";
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void TxtInstrument_TextChanged(object sender, TextChangedEventArgs e)
        {
            errLabel.Content = string.Empty;
        }
    }
}
