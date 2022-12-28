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
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Configuration;

namespace ANDE.FAIRS.Athena
{
    /// <summary>
    /// Interaction logic for ESS.xaml
    /// </summary>
    public partial class ESS : Page
    {
        private string ChipType { get; set; }

        private string ConfigFile { get; set; }

        private string GeneFinderPath { get; set; }
        public ESS(string chipType, string configFile, string essFilePath)
        {
            InitializeComponent();
            this.Loaded += ESS_Loaded;
            ChipType = chipType;
            ConfigFile = configFile;
            if(string.IsNullOrEmpty(essFilePath))
            {
                if(chipType == "A")
                {
                    GeneFinderPath = System.IO.Path.Combine(configFile, "ESS", "2", "GeneFinder.exe");
                }
                else
                {
                    GeneFinderPath = System.IO.Path.Combine(configFile, "ESS", "4", "GeneFinder.exe");
                }
            }
            else
            {
                GeneFinderPath = essFilePath;
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_MAXIMIZE = 0xF030;

        private void ESS_Loaded(object sender, RoutedEventArgs e)
        {
            Process p = Process.Start(GeneFinderPath);
            while (p.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(1);
            }

            SetParent(p.MainWindowHandle, CBox.Handle);
            SendMessage(p.MainWindowHandle, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
        }

       

        private void ReturnToInitiateClick(object sender, RoutedEventArgs e)
        {
            ClearInputFolderAndArchiveOutputData();
            NavigationService.Navigate(new SelectInstrument());
        }

        private void QuitClick(object sender, RoutedEventArgs e)
        {
            ClearInputFolderAndArchiveOutputData();
            App.Current.MainWindow.Close();
        }

        private void ClearInputFolderAndArchiveOutputData()
        {
            ClearInputData();
            ArchiveOutputData();
        }
        private void ClearInputData()
        {
            try
            {
                //string inputRootFolder = Helper.GetInputRootFolderName(ConfigurationManager.AppSettings["CogsInputFolder"]);
                var inputFolder = ConfigurationManager.AppSettings["CogsInputFolder"];
                if (Directory.Exists(inputFolder))
                {
                    foreach (var file in Directory.GetFiles(inputFolder))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch { }
        }
        private void ArchiveOutputData()
        {
            string inputRootFolder = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["CogsInputFolder"]);
            var outputDirectory = System.IO.Path.Combine(inputRootFolder, "Output");
            if(Directory.Exists(outputDirectory))
            {
                var archiveDirectory = System.IO.Path.Combine(inputRootFolder, string.Format("Output_Archive_{0}", DateTime.Now.ToString("MMddyyyy_HHmmss")));
                if(!Directory.Exists(archiveDirectory))
                {
                    Directory.CreateDirectory(archiveDirectory);
                }
                foreach (var file in Directory.GetFiles(outputDirectory))
                {
                    File.Move(file, System.IO.Path.Combine(archiveDirectory, System.IO.Path.GetFileName(file)));
                }
            }
        }
        private void WindowsFormsHost_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }
    }
}
