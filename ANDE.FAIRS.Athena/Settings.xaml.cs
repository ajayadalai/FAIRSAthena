using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            this.Loaded += Settings_Loaded;
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            var opticalPath = ConfigurationManager.AppSettings["OpticalFilePath"];
            var configPath = ConfigurationManager.AppSettings["DynamicConfigFilePath"];
            txtOpticalFileLocation.Text = opticalPath;
            txtDynamicConfigFileLocation.Text = configPath;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            txtOpticalFileLocation.Text = GetPath();
        }

        private void BrowseConfigClick(object sender, RoutedEventArgs e)
        {
            txtDynamicConfigFileLocation.Text = GetPath();
        }

        private string GetPath()
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                return openFileDlg.SelectedPath;
            }
            return string.Empty;
        }

        private void SaveSettings()
        {
            if(string.IsNullOrEmpty(txtOpticalFileLocation.Text.Trim()))
            {
                errLabel.Content = "Please select the optical file location.";
                return;
            }
            if (string.IsNullOrEmpty(txtDynamicConfigFileLocation.Text.Trim()))
            {
                errLabel.Content = "Please dynamic config package file location.";
                return;
            }
            string assemblypath = Assembly.GetExecutingAssembly().Location;
            var configFile = ConfigurationManager.OpenExeConfiguration(assemblypath);
            if (configFile.AppSettings.Settings["OpticalFilePath"] == null)
            {
                configFile.AppSettings.Settings.Add("OpticalFilePath", txtOpticalFileLocation.Text.Trim());
            }
            else
            {
                configFile.AppSettings.Settings["OpticalFilePath"].Value = txtOpticalFileLocation.Text.Trim();
            }

            if (configFile.AppSettings.Settings["DynamicConfigFilePath"] == null)
            {
                configFile.AppSettings.Settings.Add("DynamicConfigFilePath", txtDynamicConfigFileLocation.Text.Trim());
            }
            else
            {
                configFile.AppSettings.Settings["DynamicConfigFilePath"].Value = txtDynamicConfigFileLocation.Text.Trim();
            }
            configFile.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        
    }
}
