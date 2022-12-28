using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Navigation;

namespace ANDEDecryptor
{
    public static class FileDecrypter
    {
        private static bool decryptMode_ = true;

        private static bool IsRecoveryFile(string path)
        {

            return System.IO.Path.GetFileName(path).ToLower().EndsWith("recovery.txt");
        }

        public static string DecryptFile(string path, string cert)
        {

            try
            {
                if (decryptMode_)
                {
                    if (IsRecoveryFile(path))
                    {
                        return DecryptRecovery(path, cert);
                    }
                    else
                    {
                        return DecryptExportedData(path, cert);
                    }
                }
                else
                {
                    return "";
                    //EncryptExportedData();
                }
            }
            catch (Exception ex)
            {
                return string.Format("Problem decrypting: {0}", ex.Message);
            }
        }

        private static string DecryptExportedData(string path, string cert)
        {
            Console.WriteLine("Decrypt Exported Data - Path: " + path + " Certificate: " + cert);

            string from_path = path.Trim();
            string output = string.Empty;
            if (string.IsNullOrEmpty(from_path))
            {
                return "Enter Zip File Path or Drag/Drop zip file";

            }
            //if (!HasFilesInFolder(from_path))
            //{
            //    return "Folder does not contain expected files.";
            //}
            //if (!File.Exists(from_path))
            //{
            //    return "Specified path does not exist.";

            //}

            try
            {
                if (File.Exists(from_path))
                {
                    output = FileDecrypter.Decrypt(from_path, cert);
                    if (!string.IsNullOrEmpty(output))
                        return string.Format("Success:{0}", output);
                    //Message.Display("Information", output);
                    else
                        return "The path doesn't contain any zip file to decrypt. The path should contain at least a zip file having encrypted data inside it.";
                }
                else
                {
                    string[] files = Directory.GetFiles(from_path, "*.zip", SearchOption.AllDirectories);

                    foreach (var filePath in files)
                    {
                        output += FileDecrypter.Decrypt(filePath, cert);
                    }
                    if (!string.IsNullOrEmpty(output))
                        return string.Format("Success:{0}", output.TrimStart('/'));
                    // return string.Join("Information: ", output.ToArray());
                    // Message.Display("Information", output);
                    else
                        return "The path doesn't contain any zip file to decrypt. The path should contain at least a zip file having encrypted data inside it.";
                }

            }
            finally
            {

            }
        }


        private static string Decrypt(string from_path, string cert)
        {
            List<string> msgOutput = new List<string>();
            var opticalFolderPath = System.Configuration.ConfigurationManager.AppSettings["OpticalFilePath"];
            var opticalFolderName = opticalFolderPath.Split('\\').Last();
            var decryptDirectory = Path.Combine(Path.GetDirectoryName(opticalFolderPath), string.Format("{0}{1}",opticalFolderName, "_Decrypted"));
            if(!Directory.Exists(decryptDirectory))
            {
                Directory.CreateDirectory(decryptDirectory);
            }
            string to_path = System.IO.Path.Combine(decryptDirectory, System.IO.Path.GetFileNameWithoutExtension(from_path) + "_decrypted.zip");
            string fileName = Path.GetFileName(from_path);
            using (var crypto = new Crypto(cert))
            {
                if (!crypto.Valid)
                {
                    return string.Format("No key for {0}.cer", cert);
                }

                using (ZipFile dec = new ZipFile())
                {
                    List<string> rejFiles = new List<string>();

                    using (ZipFile enc = ZipFile.Read(from_path))
                    {
                        if (!enc.Entries.Any(x => x.FileName.Contains(".enc")))
                        {
                            //Message.Display("Error", "The folder doesn't contain any encrypted file.");
                            return string.Empty;
                        }
                        foreach (ZipEntry enc_entry in enc)
                        {
                            try
                            {
                                MemoryStream buffer = new MemoryStream();
                                enc_entry.Extract(buffer);
                                buffer.Seek(0, 0);

                                buffer = crypto.CheckSignatureAndExtract(buffer);
                                buffer = crypto.Decrypt(buffer);
                                buffer = crypto.CheckSignatureAndExtract(buffer);

                                // Avoid .Replace() - be sure that we only remove from the end
                                string dec_filename = enc_entry.FileName;
                                if (dec_filename.ToLower().EndsWith(".enc"))
                                {
                                    dec_filename = dec_filename.Substring(0, dec_filename.Length - 4);
                                }
                                dec.AddEntry(dec_filename, buffer.ToArray());

                            }
                            catch (Exception ex)
                            {
                                rejFiles.Add(enc_entry.FileName);
                                continue;
                            }
                        }
                    }

                    dec.Save(to_path);

                    //string failedFiles = string.Format("\nUnable to decrypt the following files: {0}\n", string.Join(", ", rejFiles.ToArray()));
                    //if (rejFiles.Count() > 0)
                    //    msgOutput.Add(string.Format("\n{0}\n{1} files decrypted successfully.\n{2}", fileName, dec.Count(), failedFiles));
                    //else
                    //    msgOutput.Add(string.Format("\n{0}\n{1} files decrypted successfully.\n", fileName, dec.Count()));


                    string failedFiles =  string.Join(",", rejFiles.ToArray());
                    if (rejFiles.Count() > 0)
                        msgOutput.Add(string.Format("{0}|{1}|{2}", fileName, dec.Count(), failedFiles));
                    else
                        msgOutput.Add(string.Format("{0}|{1}", fileName, dec.Count()));
                }
            }
            return string.Format("/{0}", msgOutput.ToArray());
        }

        private static string DecryptRecovery(string path, string cert)
        {

            string rec_file = path.Trim();

            string[] lines = File.ReadAllLines(rec_file);
            if (lines.Length < 2)
            {
                return "Recovery.txt file not in expected format";
            }

            try
            {
                string tmp_pw = "";
                //return "Decrypting...";

                string enc = lines[1].Trim();
                byte[] enc_bytes = Convert.FromBase64String(enc);
                using (var crypto = new Crypto(cert))
                {
                    byte[] clear = crypto.RSADecrypt(enc_bytes);
                    tmp_pw = new string(Encoding.ASCII.GetChars(clear));
                }
                return "";
            }
            finally
            {
            }
        }

        private static bool HasFilesInFolder(string folder)
        {
            if (Directory.GetFiles(folder, "*.zip").Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
