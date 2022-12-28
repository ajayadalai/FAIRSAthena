using ANDEDecryptor;
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
    /// Interaction logic for SelectOpticalFiles.xaml
    /// </summary>
    public partial class SelectOpticalFiles : Page
    {
        const string certName = @"PX_Service_20130402_160247.pfx";
        private string Instrument { get; set; }
        private string ChipType { get; set; }
        public OpticalFile SelectedFile { get; private set; }

        public SelectOpticalFiles(string instrumentName, string chipType)
        {
            InitializeComponent();
            Instrument = instrumentName;
            ChipType = chipType;
            this.Loaded += SelectOpticalFiles_Loaded;
            if (null != pagingCtrl_)
            {
                pagingCtrl_.PageChanged += () =>
                {
                    BindData(dateRange_.GetCurrStart(),dateRange_.GetCurrEnd(), txtSearch.SearchText);
                };
            }
            
           
        }

        private List<OpticalFile> GetOpticalFiles()
        {
            var fileList = new List<OpticalFile>();
            var files = Directory.GetFiles(ConfigurationManager.AppSettings["OpticalFilePath"], string.Format("{0}*.*", Instrument));
            foreach (var file in files)
            {
                fileList.Add(new OpticalFile { FullPath = file, Instrument = Instrument });
            }
            return fileList;
        }
        private void SelectOpticalFiles_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != dateRange_)
            {
                dateRange_.ForwardSelectionChanged += (a, b) =>
                {
                    pagingCtrl_.CurrentPageIndex = 0;
                    BindData(dateRange_.GetCurrStart(), dateRange_.GetCurrEnd(), txtSearch.SearchText);
                };
            }

            if(null != txtSearch)
            {
                txtSearch.OnSearch += TxtSearch_OnSearch;
                txtSearch.OnClear += TxtSearch_OnClear;
            }

            var fileList = GetOpticalFiles();
            DateTime beginDate = fileList.Select(i => i.Date).Min().HasValue ? fileList.Select(i => i.Date).Min().Value : DateTime.Now;
            DateTime endDate = fileList.Select(i => i.Date).Max().HasValue ? fileList.Select(i => i.Date).Max().Value : DateTime.Now;
            dateRange_.InitStartEnd(beginDate, endDate);
            BindData(dateRange_.GetCurrStart(), dateRange_.GetCurrEnd(), txtSearch.SearchText);
        }

        private void TxtSearch_OnClear()
        {
            BindData(dateRange_.GetCurrStart(), dateRange_.GetCurrEnd(), txtSearch.SearchText);
        }

        private void TxtSearch_OnSearch(string obj)
        {
            BindData(dateRange_.GetCurrStart(), dateRange_.GetCurrEnd(), txtSearch.SearchText);
        }

        private void BindData(DateTime start, DateTime end, string searchText)
        {
            var fileList = new List<OpticalFile>();
            var files = Directory.GetFiles(ConfigurationManager.AppSettings["OpticalFilePath"], string.Format("{0}*.*",Instrument));
            foreach (var file in files.Where(i=> !i.ToLower().EndsWith("_decrypted.zip")))
            {
                fileList.Add(new OpticalFile { FullPath = file, Instrument = Instrument });
            }
            if (txtSearch.SearchInstructions == txtSearch.SearchText)
                searchText = string.Empty;
            fileList = fileList.Where(i => i.Date.Value.Date >= start.Date && i.Date.Value.Date <= end.Date.Date && 
            (i.RunID.ToLower().Contains(searchText.ToLower()) || i.Instrument.ToLower().Contains(searchText.ToLower()))).ToList();
            SetPagingControl(fileList.Count);
            fileList_.ItemsSource = fileList.Skip(pagingCtrl_.PageSize * pagingCtrl_.CurrentPageIndex).Take(pagingCtrl_.PageSize);
            fileList_.DataContext = fileList;

            
        }

        private void SelectClick(object sender, RoutedEventArgs e)
        {
            DecryptOpticalFile();

            //NavigationService.Navigate(new SelectDynamicConfig(Instrument, ChipType, string.Empty));
        }

        private void DecryptOpticalFile()
        {
            InstallIfDecryptorCertNotInstalled();
            DecryptFile();
        }

        private void DecryptFile()
        {
            try
            {
                busyIndicator.IsBusy = true;
                string output = string.Empty;
                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(2000);
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string certFilePath = System.IO.Path.Combine(System.IO.Path.Combine(currentDirectory, "Certificate"), certName);
                    string fileNameExtless = System.IO.Path.GetFileNameWithoutExtension(certFilePath);

                    //Status(FileDecrypter.DecryptFile(path, fileNameExtless));
                    output = FileDecrypter.DecryptFile(SelectedFile.FullPath, fileNameExtless);
                }).ContinueWith((t) =>
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            busyIndicator.IsBusy = false;
                            if (output.StartsWith("Success:"))
                            {
                                //mainFrame_.Navigate(new Decryptor());
                                NavigationService.Navigate(new SelectDynamicConfig(Instrument, ChipType, SelectedFile.FullPath));
                            }
                            else
                            {
                                //Status(output);
                            }
                        });
                    }

                });
                //busyIndicator.IsBusy = false;

            }
            catch (Exception ex)
            {
                //Status(string.Format("Problem decrypting: {0}", ex.Message));
            }
        }

        private void InstallIfDecryptorCertNotInstalled()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string certFilePath = System.IO.Path.Combine(System.IO.Path.Combine(currentDirectory, "Certificate"), certName);
            string fileNameExtless = System.IO.Path.GetFileNameWithoutExtension(certFilePath);
            if (!IsCertificateInstalled("CN=" + fileNameExtless))
                CertHelper.ImportCertificateWithPrivateKey(certFilePath, "12345678");
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SelectChipType(Instrument));
        }

        private void fileList__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool enableButtons = false;
            enableButtons = e.AddedItems.Count > 0;
            if (enableButtons)
            {
                SelectedFile = e.AddedItems[0] as OpticalFile;
            }
            bdrSelect.IsEnabled = enableButtons;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = (sender as ListViewItem).Content as OpticalFile;
            
        }

        private void SetPagingControl(int totalCount)
        {
            pagingCtrl_.TotalItems = totalCount;
            pagingCtrl_.currentPage.Text = (pagingCtrl_.CurrentPage).ToString();
            pagingCtrl_.totalPage.Text = pagingCtrl_.TotalPages.ToString();
            pagingCtrl_.ConfigureButtons();
        }


        static bool IsCertificateInstalled(string certName)
        {
            var all_certs = CertHelper.GetCertificatesLike("(i[0-9][0-9][0-9][0-9]_[0-9]+_[0-9]+$)|(^PX_)|(^RDNA_)");
            foreach (var cert in all_certs)
            {
                if (cert.Subject == certName)
                    return true;
            }
            return false;
        }

        public class OpticalFile
        {
            public string FullPath { get; set; }

            public string RunID { get { return System.IO.Path.GetFileName(FullPath); } }

            public DateTime? Date
            {
                get
                {
                    DateTime? retVal = null;
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(RunID);
                    var dateString = fileName.Substring(fileName.Length - 13, 6);
                    var timeString = fileName.Substring(fileName.Length - 6, 6);
                    try
                    {
                        if (int.TryParse(dateString.Substring(0, 2), out int mm))
                        {
                            if (int.TryParse(dateString.Substring(2, 2), out int dd))
                            {
                                if (int.TryParse("20" + dateString.Substring(4, 2), out int yy))
                                {
                                    if (int.TryParse(timeString.Substring(0, 2), out int HH))
                                    {
                                        if (int.TryParse(timeString.Substring(2, 2), out int mi))
                                        {
                                            if (int.TryParse(timeString.Substring(4, 2), out int ss))
                                            {
                                                retVal = new DateTime(yy, mm, dd, HH, mi, ss);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                    return retVal;
                }
            }

            public string Instrument { get; set; }
        }

        
    }

    
}
