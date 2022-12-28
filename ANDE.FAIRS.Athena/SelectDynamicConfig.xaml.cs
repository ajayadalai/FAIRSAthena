using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
    /// Interaction logic for SelectDynamicConfig.xaml
    /// </summary>
    public partial class SelectDynamicConfig : Page
    {
        private string Instrument { get; set; }
        private string ChipType { get; set; }

        private string OpticalFile { get; set; }
        private string ConfigFile { get; set; }
        public SelectDynamicConfig(string instrumentName, string chipType, string opticalFile)
        {
            InitializeComponent();
            Instrument = instrumentName;
            ChipType = chipType; 
            OpticalFile = opticalFile;
            this.Loaded += SelectOpticalFiles_Loaded;
            if (null != pagingCtrl_)
            {
                pagingCtrl_.PageChanged += () =>
                {
                    BindData(txtSearch.SearchText);
                };
            }
        }

        private void SelectOpticalFiles_Loaded(object sender, RoutedEventArgs e)
        {
            lblInstrument.Content = string.Format("Available Dynamic Config files for Instrument : {0}", Instrument);
            BindData(txtSearch.SearchText);
        }

        private void BindData(string searchContent)
        {
            if (searchContent == txtSearch.SearchInstructions)
                searchContent = string.Empty;
            var fileList = new List<ConfigPackage>();
            var files = Directory.GetDirectories(ConfigurationManager.AppSettings["DynamicConfigFilePath"]);
            foreach (var file in files.Where(i => !i.ToLower().EndsWith("_decrypted.zip")))
            {
                if (System.IO.Path.GetFileName(file).ToLower().Contains(searchContent.ToLower()))
                {
                    fileList.Add(new ConfigPackage { FullPath = file });
                }
            }
            try
            {
                SetPagingControl(fileList.Count);
                fileList_.ItemsSource = fileList.Skip(pagingCtrl_.PageSize * pagingCtrl_.CurrentPageIndex).Take(pagingCtrl_.PageSize);
                fileList_.DataContext = fileList;
            }
            catch(Exception ex)
            {

            }

        }

        private void SetPagingControl(int totalCount)
        {
            pagingCtrl_.TotalItems = totalCount;
            pagingCtrl_.currentPage.Text = (pagingCtrl_.CurrentPage).ToString();
            pagingCtrl_.totalPage.Text = pagingCtrl_.TotalPages.ToString();
            pagingCtrl_.ConfigureButtons();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SelectOpticalFiles(Instrument, ChipType));
        }

        private void SelectClick(object sender, RoutedEventArgs e)
        {
            var essFilePath = System.IO.Path.Combine(ConfigFile, "ESS", ChipType == "A" ? "2" : "4", "GeneFinder.exe");
            if(!File.Exists(essFilePath))
            {
                Message.Display("No ESS Found", string.Format("This dynamic config package does not contain the ESS (GeneFinder.exe) for '{0}' chip type.", ChipType));
            }
            NavigationService.Navigate(new ExecuteCogs(Instrument, ChipType, OpticalFile, ConfigFile));
        }

        private void fileList__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bdrConfigFile.IsEnabled = e.AddedItems.Count > 0;
            if(e.AddedItems.Count > 0)
            {
                ConfigFile = (e.AddedItems[0] as ConfigPackage).FullPath;
            }
            else
            {
                ConfigFile = string.Empty;
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void TxtSearch_OnSearch(string obj)
        {
            BindData(txtSearch.SearchText);
        }

        private void TxtSearch_OnClear()
        {
            BindData(string.Empty);
        }
    }

    public class ConfigPackage
    {
        public string FullPath { get; set; }

        public string ConfigPackageFile
        {
            get
            {
                return System.IO.Path.GetFileName(FullPath);
            }
        }

        public DateTime? Date
        {
            get
            {
                DateTime? retVal = null;
                retVal = System.IO.Directory.GetCreationTime(FullPath);
                return retVal;
            }
        }

      
    }
}
