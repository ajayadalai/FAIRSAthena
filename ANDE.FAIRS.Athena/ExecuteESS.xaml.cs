using Microsoft.Win32;
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
    /// Interaction logic for ExecuteESS.xaml
    /// </summary>
    public partial class ExecuteESS : Page
    {
        private string ChipType { get; set; }

        private string ConfigFile { get; set; }

        public ExecuteESS(string chipType, string configFile)
        {
            InitializeComponent();
            ChipType = chipType;
            ConfigFile = configFile;
        }

        private void InitiateClick(object sender, RoutedEventArgs e)
        {
            if(rbManual.IsChecked.HasValue && rbManual.IsChecked.Value && string.IsNullOrEmpty(txtEss.Text))
            {
                Message.Display("Select ESS", "Please select the ESS file you want to execute or select Default ESS.");

                return;

            }
            var GeneFinderPath = string.Empty;
            if (string.IsNullOrEmpty(txtEss.Text))
            {
                if (ChipType == "A")
                {
                    GeneFinderPath = System.IO.Path.Combine(ConfigFile, "ESS", "2");
                }
                else
                {
                    GeneFinderPath = System.IO.Path.Combine(ConfigFile, "ESS", "4");
                }
            }
            else
            {
                GeneFinderPath = System.IO.Path.GetDirectoryName(txtEss.Text);
            }

            var inputParameters = System.IO.Directory.GetFiles(GeneFinderPath, "InputParameters*.txt");
            if(inputParameters.Count() == 0)
            {
                Message.Display("No InputParameters File", "There is no InputParameters file inside the ESS folder for the selected Dynamic Config Package.");

                return;
            }
            var ladderRefF6 = System.IO.Directory.GetFiles(GeneFinderPath, "LadderRefF6*.txt");
            if (ladderRefF6.Count() == 0)
            {
                Message.Display("No LadderRefF6 File", "There is no LadderRefF6 file inside the ESS folder for the selected Dynamic Config Package.");

                return;
            }
            
            var alleleStutter = System.IO.Directory.GetFiles(GeneFinderPath, "AlleleStutter*.txt");
            if (alleleStutter.Count() == 0)
            {
                Message.Display("No AlleleStutter File", "There is no AlleleStutter file inside the ESS folder for the selected Dynamic Config Package.");

                return;
            }
            var peakRefILSF6 = System.IO.Directory.GetFiles(GeneFinderPath, "PeakRefILSF6*.txt");
            if (peakRefILSF6.Count() == 0)
            {
                Message.Display("No PeakRefILSF6 File", "There is no PeakRefILSF6 file inside the ESS folder for the selected Dynamic Config Package.");

                return;
            }
            System.IO.File.Copy(inputParameters[0], "InputParameters.txt", true);
            System.IO.File.Copy(ladderRefF6[0], "LadderRefF6.txt", true);
            System.IO.File.Copy(alleleStutter[0], "AlleleStutter.txt", true);
            System.IO.File.Copy(peakRefILSF6[0], "PeakRefILSF6.txt", true);
            if(System.IO.File.Exists(System.IO.Path.Combine(GeneFinderPath, "GeneProc.dll")))
            {
                System.IO.File.Copy(System.IO.Path.Combine(GeneFinderPath, "GeneProc.dll"), "GeneProc.dll", true);
            }
            if (System.IO.File.Exists(System.IO.Path.Combine(GeneFinderPath, "LatestLadderPX.txt")))
            {
                System.IO.File.Copy(System.IO.Path.Combine(GeneFinderPath, "LatestLadderPX.txt"), "LatestLadderPX.txt", true);
            }
            if (System.IO.File.Exists(System.IO.Path.Combine(GeneFinderPath, "Logo.bmp")))
            {
                System.IO.File.Copy(System.IO.Path.Combine(GeneFinderPath, "Logo.bmp"), "Logo.bmp", true);
            }
            if (System.IO.File.Exists(System.IO.Path.Combine(GeneFinderPath, "StdGS.fsa")))
            {
                System.IO.File.Copy(System.IO.Path.Combine(GeneFinderPath, "StdGS.fsa"), "StdGS.fsa", true);
            }
            if (System.IO.File.Exists(System.IO.Path.Combine(GeneFinderPath, "StdPX.fsa")))
            {
                System.IO.File.Copy(System.IO.Path.Combine(GeneFinderPath, "StdPX.fsa"), "StdPX.fsa", true);
            }
            NavigationService.Navigate(new ESS(ChipType, ConfigFile, txtEss.Text));
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SelectInstrument());
        }

        private void RbDef_Checked(object sender, RoutedEventArgs e)
        {
            if (txtEss != null)
            {
                txtEss.IsEnabled = false;
                bdrBrowse.IsEnabled = false;
                txtEss.Text = string.Empty;
            }
        }

        private void RbManual_Checked(object sender, RoutedEventArgs e)
        {
            txtEss.IsEnabled = true;
            bdrBrowse.IsEnabled = true;
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                //openFileDialog.Filter = "ESS files (*.exe)|";
                if (openFileDialog.ShowDialog() == true)
                {
                    if(!System.IO.Path.GetFileName(openFileDialog.FileName).EndsWith("genefinder.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        Message.Display("Invalid GeneFinder", "Please select any GeneFinder.exe file.");
                        return;
                    }
                    txtEss.Text = openFileDialog.FileName;
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
