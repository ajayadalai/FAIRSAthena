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

namespace ANDE.FAIRS.Athena
{
    /// <summary>
    /// Interaction logic for SelectChipType.xaml
    /// </summary>
    public partial class SelectChipType : Page
    {
        private string Instrument { get; set; }
        public SelectChipType(string instrumentName)
        {
            InitializeComponent();
            Instrument = instrumentName;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SelectInstrument());
        }

        private void SelectClick(object sender, RoutedEventArgs e)
        {
            if (cmbChipType.SelectedIndex >= 0)
            {
                NavigationService.Navigate(new SelectOpticalFiles(Instrument, (cmbChipType.SelectedItem as ComboBoxItem).Content.ToString()));
            }
            else
            {
                errLabel.Content = "Please select chip type.";
            }
        }

        private void CmbChipType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            errLabel.Content = string.Empty;
        }
    }
}
