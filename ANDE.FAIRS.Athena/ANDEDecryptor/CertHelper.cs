
namespace ANDEDecryptor
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Security;
    using System.Security.Cryptography;
    using SystemX509 = System.Security.Cryptography.X509Certificates;
    using Org.BouncyCastle.X509;
    using Org.BouncyCastle.Asn1.X509;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Pkcs;
    using Org.BouncyCastle.Security;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Prng;
    using Org.BouncyCastle.Crypto.Generators;
    using Org.BouncyCastle.Crypto.Parameters;

    public class CertHelper
    {
        public static readonly int EXPIRE_AFTER_YEARS = 20;

        private static SystemX509.StoreName storeName_ = SystemX509.StoreName.My;
        private static SystemX509.StoreLocation storeLoc_ = SystemX509.StoreLocation.CurrentUser;
        private static SystemX509.X509KeyStorageFlags keySetFlags_ = SystemX509.X509KeyStorageFlags.UserKeySet
                                                                   | SystemX509.X509KeyStorageFlags.PersistKeySet;

        public static SystemX509.StoreName StoreName
        {
            get { return storeName_; }
            set { storeName_ = value; }
        }

        public static SystemX509.StoreLocation StoreLocation
        {
            get { return storeLoc_; }
            set { storeLoc_ = value; }
        }

        public static string ImportCertificateWithPrivateKey(string path, string password)
        {
            SystemX509.X509Certificate2 cert = new SystemX509.X509Certificate2();
            cert.Import(path, password, keySetFlags_);

            // Install - need to keep it under current user location so that
            // the certificate can be easily exported with private key.
            // Otherwise user will require more access permissions in the store
            // to export if private key stored in local machine location.
            SystemX509.X509Store store = new SystemX509.X509Store(storeName_, storeLoc_);

            try
            {
                store.Open(SystemX509.OpenFlags.ReadWrite);
                store.Add(cert);
            }
            finally
            {
                store.Close();
            }

            return cert.SubjectName.Name.Replace("CN=", "");
        }

        public static SystemX509.X509Certificate2 MakeCertificateWithPrivateKey(string subjectName, string friendlyName)
        {
            if (string.IsNullOrEmpty(subjectName))
            {
                throw new ApplicationException("Invalid certificate name");
            }

            AsymmetricCipherKeyPair kp;
            X509Certificate cert = GenerateCertificate(subjectName, out kp);

            // Make sure we can get a private key
            RsaPrivateCrtKeyParameters rsa_prv_crt_params = kp.Private as RsaPrivateCrtKeyParameters;
            if (rsa_prv_crt_params == null)
            {
                throw new ApplicationException("Could not generate certificate");
            }

            // Convert to .NET certificate instance
            SystemX509.X509Certificate2 ms_cert = new SystemX509.X509Certificate2(cert.GetEncoded());
            ms_cert.FriendlyName = friendlyName;

            // Attache private key to MS cert that was generated with Bouncy Castle
            RSA ms_rsa = DotNetUtilities.ToRSA(rsa_prv_crt_params);
            RSAParameters ms_rsa_params = ms_rsa.ExportParameters(true);
            RSACryptoServiceProvider rsa_provider = new RSACryptoServiceProvider();
            rsa_provider.ImportParameters(ms_rsa_params);
            ms_cert.PrivateKey = rsa_provider;

            return ms_cert;
        }

        public static void InstallCertificate(SystemX509.X509Certificate2 cert)
        {
            // Install - need to keep it under current user location so that
            // the certificate can be easily exported with private key.
            // Otherwise user will require more access permissions in the store
            // to export if private key stored in local machine location.
            SystemX509.X509Store store = new SystemX509.X509Store(storeName_, storeLoc_);

            try
            {
                store.Open(SystemX509.OpenFlags.ReadWrite);
                store.Add(cert);
            }
            finally
            {
                store.Close();
            }
        }

        public static void InstallCertificateWithPrivateKeyOneWay(SystemX509.X509Certificate2 cert)
        {
            // We don't plan on exporting, so just use the current time as a password
            string arbitrary_pw = DateTime.Now.ToString();
            char[] pw = arbitrary_pw.ToCharArray();

            using (var pw_sec = new SecureString())
            {
                foreach (char ch in pw)
                {
                    pw_sec.AppendChar(ch);
                }

                byte[] cert_bytes = cert.Export(SystemX509.X509ContentType.Pfx, arbitrary_pw);

                var pfx_cert = new SystemX509.X509Certificate2(cert_bytes, pw_sec, keySetFlags_);

                InstallCertificate(pfx_cert);
            }
        }

        
        private static X509Certificate GenerateCertificate(string subjectName, out AsymmetricCipherKeyPair kp)
        {
            var kpgen = new RsaKeyPairGenerator();

            // certificate strength 2048 bits
            kpgen.Init(new KeyGenerationParameters(
                  new SecureRandom(new CryptoApiRandomGenerator()), 2048));

            kp = kpgen.GenerateKeyPair();

            var gen = new X509V3CertificateGenerator();

            var certName = new X509Name("CN=" + subjectName);
            var serialNo = BigInteger.ProbablePrime(120, new Random());

            gen.SetSerialNumber(serialNo);
            gen.SetSubjectDN(certName);
            gen.SetIssuerDN(certName);
            gen.SetNotAfter(DateTime.Now.AddYears(EXPIRE_AFTER_YEARS));
            gen.SetNotBefore(DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)));
            gen.SetSignatureAlgorithm("SHA1withRSA");
            gen.SetPublicKey(kp.Public);

            gen.AddExtension(
                X509Extensions.AuthorityKeyIdentifier.Id
              , false
              , new AuthorityKeyIdentifier(
                    SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(kp.Public)
                  , new GeneralNames(new GeneralName(certName))
                  , serialNo));

            //gen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));

            /* 
             1.3.6.1.5.5.7.3.1 - id_kp_serverAuth 
             1.3.6.1.5.5.7.3.2 - id_kp_clientAuth 
             1.3.6.1.5.5.7.3.3 - id_kp_codeSigning 
             1.3.6.1.5.5.7.3.4 - id_kp_emailProtection 
             1.3.6.1.5.5.7.3.5 - id-kp-ipsecEndSystem 
             1.3.6.1.5.5.7.3.6 - id-kp-ipsecTunnel 
             1.3.6.1.5.5.7.3.7 - id-kp-ipsecUser 
             1.3.6.1.5.5.7.3.8 - id_kp_timeStamping 
             1.3.6.1.5.5.7.3.9 - OCSPSigning
             */

            gen.AddExtension(
                X509Extensions.ExtendedKeyUsage.Id
              , false
              , new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth, KeyPurposeID.AnyExtendedKeyUsage));

            var newCert = gen.Generate(kp.Private);

            return newCert;
        }

        public static void ExportCertificate(string targetDir, string name)
        {
            SystemX509.X509Certificate2 cert = GetCertificate(name);
            if (cert == null)
            {
                throw new ApplicationException(string.Format("{0} not found in certificate store", name));
            }

            byte[] cert_in_bytes;
            cert_in_bytes = cert.Export(SystemX509.X509ContentType.Cert);

            string target_path = Path.Combine(targetDir, name) + ".cer";
            File.WriteAllBytes(target_path, cert_in_bytes);
        }

        public static SystemX509.X509Certificate2 GetCertificate(string certName)
        {
            return GetCertificate(certName, storeName_, storeLoc_);
        }

        public static SystemX509.X509Certificate2 GetCertificate(string certName, SystemX509.StoreName storeName, SystemX509.StoreLocation storeLoc)
        {
            SystemX509.X509Store store = null;
            try
            {
                store = new SystemX509.X509Store(storeName, storeLoc);
                store.Open(SystemX509.OpenFlags.ReadOnly);
                foreach (var cert in store.Certificates)
                {
                    if (cert.SubjectName.Name.Contains(certName))
                    {
                        return cert;
                    }
                }
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }

            return null;
        }

        public static SystemX509.X509Certificate2Collection GetCertificatesLike(string pattern)
        {
            Regex re = new Regex(pattern);

            var ret = new SystemX509.X509Certificate2Collection();

            SystemX509.X509Store store = null;
            try
            {
                store = new SystemX509.X509Store(storeName_, storeLoc_);
                store.Open(SystemX509.OpenFlags.ReadOnly);
                foreach (var cert in store.Certificates)
                {
                    string name = cert.SubjectName.Name.Replace("CN=", "");
                    if (re.IsMatch(name))
                    {
                        ret.Add(cert);
                    }
                    if (re.IsMatch(cert.FriendlyName) && !ret.Contains(cert))
                    {
                        ret.Add(cert);
                    }
                }
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }

            return ret;
        }

        public static void RemoveCertificates(SystemX509.X509Certificate2Collection certs)
        {
            if (certs.Count == 0)
            {
                return;
            }

            SystemX509.X509Store store = null;
            try
            {
                store = new SystemX509.X509Store(storeName_, storeLoc_);
                store.Open(SystemX509.OpenFlags.ReadWrite);
                store.RemoveRange(certs);
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }
        }
    }
}
