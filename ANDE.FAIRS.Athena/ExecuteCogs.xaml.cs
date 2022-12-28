using Ionic.Zip;
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
    /// Interaction logic for ExecuteCogs.xaml
    /// </summary>
    public partial class ExecuteCogs : Page
    {
        private string Instrument { get; set; }
        private string ChipType { get; set; }

        private string OpticalFile { get; set; }
        private string ConfigFile { get; set; }

        private string DatFile { get; set; }
        private string BinFile { get; set; }
        private string NboFile { get; set; }

        public ExecuteCogs(string instrumentName, string chipType, string opticalFile, string configFile)
        {
            Instrument = instrumentName;
            ChipType = chipType;
            OpticalFile = opticalFile;
            ConfigFile = configFile;
            InitializeComponent();
        }

        private void InitiateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateInputOutputFolderIfNotExists();
                DatFile = string.Empty;
                NboFile = string.Empty;
                BinFile = string.Empty;
                var binFilePath = GetBinFilePath();
                if(string.IsNullOrEmpty(binFilePath))
                {
                    Message.Display("Error", "Could not find the bin file for the instrument.");
                    return;
                }
                busyIndicator.IsBusy = true;
                bdrExecute.IsEnabled = false;
                Task.Run(() => 
                {
                    CopyDatFileToTempFolder();
                    CopyFileToTempFolder("cogs.exe", true);
                    CopyFileToTempFolder(binFilePath, true);
                    ExecuteAndPrepareNbos();
                    System.Threading.Thread.Sleep(4000);
                }).ContinueWith((t)=> 
                {

                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            int i = 0;
                            
                            var nboCount = Directory.GetFiles("Temp", "*.nbo").Count();
                            while(nboCount == 0)
                            {
                                if (i >= 3)
                                    break;
                                System.Threading.Thread.Sleep(3000);
                                i = i + 1;
                            }

                            if (Directory.GetFiles("Temp", "*.nbo").Count() > 0)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("The following .nbo files created successfully.{0}{0}", Environment.NewLine);
                                if (Directory.Exists(ConfigurationManager.AppSettings["CogsInputFolder"]))
                                {
                                    Directory.Delete(ConfigurationManager.AppSettings["CogsInputFolder"], true);
                                }
                                CreateInputOutputFolderIfNotExists();
                                foreach (var file in Directory.GetFiles("Temp", "*.nbo"))
                                {
                                    sb.AppendFormat("{0}{1}", System.IO.Path.GetFileName(file), Environment.NewLine);
                                    File.Copy(file, System.IO.Path.Combine(ConfigurationManager.AppSettings["CogsInputFolder"], System.IO.Path.GetFileName(file)));
                                }
                                busyIndicator.IsBusy = false;
                                bdrExecute.IsEnabled = true;
                                Message.Display("Success", sb.ToString());
                                NavigationService.Navigate(new ExecuteESS(ChipType, ConfigFile));
                            }
                            else
                            {
                                Message.Display("Error", "Could not produce the nbo files.");
                                busyIndicator.IsBusy = false;
                                bdrExecute.IsEnabled = true;
                            }
                        }
                        catch(Exception ex)
                        {
                            busyIndicator.IsBusy = false;
                            bdrExecute.IsEnabled = true;
                        }
                    });
                });
                               
                
            }
            catch (Exception ex)
            {
                busyIndicator.IsBusy = false;
                bdrExecute.IsEnabled = true;
                Message.Display("Error", string.Format("System error. Please try again or kindly contact adminstrator if issue persists. {0}{0}{1}",Environment.NewLine,  ex.ToString()));
            }
            
        }

        private void ExecuteAndPrepareNbos()
        {
            // Start MongoDB
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            // Stop the process from opening a new window
            // process.StartInfo.RedirectStandardOutput = true;
            // process.StartInfo.UseShellExecute = false;
            // process.StartInfo.CreateNoWindow = true;

            var cogsPath = string.Empty;

            if (File.Exists(@"ExecuteCogs.bat"))
            {
                // Setup executable and parameters
               
                process.StartInfo.FileName = "ExecuteCogs.bat";
                var arguments = string.Format("{0} {1} {2} {3}", "Temp", DatFile, BinFile, NboFile);
                process.StartInfo.Arguments = arguments;

                // Go
                var started = process.Start();
                if (started)
                {
                    process.WaitForExit();
                }
            }
        }

        private void CopyDatFileToTempFolder()
        {
            if(Directory.Exists("Temp"))
                Directory.Delete("Temp", true);
            Directory.CreateDirectory("Temp");
            var decryptFolder = string.Format("{0}_Decrypted", ConfigurationManager.AppSettings["OpticalFilePath"]);
            var decryptPath = System.IO.Path.Combine(decryptFolder, System.IO.Path.GetFileName(OpticalFile).Replace(".zip", "_decrypted.zip"));
            using (var decryptedZipFile = new ZipFile(decryptPath))
            {   
                var datFile = decryptedZipFile.Where(i => i.FileName.ToLower().EndsWith("dat")).FirstOrDefault();
                DatFile = datFile.FileName;
                NboFile = DatFile.Replace(".dat", ".nbo");
                datFile.Extract("Temp");
            }
           
        }

        private void CreateInputOutputFolderIfNotExists()
        {
            if(!Directory.Exists(ConfigurationManager.AppSettings["CogsInputFolder"]))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["CogsInputFolder"]);
            }

            string inputRootFolder = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["CogsInputFolder"]);

            var outputPath = System.IO.Path.Combine(inputRootFolder, "Output");
            if(!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
        }

        private string GetBinFilePath()
        {
            if(ChipType.ToUpper() == "A")
            {
                return GetBinFile("2");
            }
            if(ChipType.ToUpper()=="I")
            {
                return GetBinFile("4");
            }
            return string.Empty;
        }

        private string GetBinFile(string paramFolder)
        {
            string retVal = string.Empty;
            var path = System.IO.Path.Combine(ConfigFile, "Data_Processing", paramFolder, "params");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, string.Format("{0}*.bin", Instrument));
                if (files.Count() > 0)
                {
                    retVal = files.First();
                    BinFile = System.IO.Path.GetFileName(retVal);
                }
            }
            return retVal;
        }

        private void CopyFileToTempFolder(string path, bool preserveOldFile)
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            if(!preserveOldFile)
            {
                Directory.Delete("Temp",true);
                Directory.CreateDirectory("Temp");
            }
            File.Copy(path, System.IO.Path.Combine("Temp", System.IO.Path.GetFileName(path)));
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SelectDynamicConfig(Instrument, ChipType, OpticalFile));
        }
    }
}
