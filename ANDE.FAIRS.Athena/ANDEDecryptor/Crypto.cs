using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace ANDEDecryptor
{
    public class Crypto : IDisposable
    {
        X509Certificate2 cert_;
        X509Certificate2 signingCert_;
        RSACryptoServiceProvider public_;
        RSACryptoServiceProvider private_;

        public Crypto(X509Certificate2 cert)
        {
            cert_ = cert;
        }

        public Crypto(string certName)
        {
            cert_ = CertHelper.GetCertificate(certName);
        }

        public Crypto(string certName, string signingCert)
        {
            cert_ = CertHelper.GetCertificate(certName);
            signingCert_ = CertHelper.GetCertificate(signingCert);
        }

        public void Dispose()
        {
            public_ = null;
            private_ = null;
            cert_ = null;
            signingCert_ = null;
        }

        public bool Valid
        {
            get
            {
                return cert_ != null;
            }
        }

        private RSACryptoServiceProvider PublicKey
        {
            get
            {
                if (public_ == null)
                {
                    public_ = (RSACryptoServiceProvider)cert_.PublicKey.Key;
                }
                return public_;
            }
        }

        private RSACryptoServiceProvider PrivateKey
        {
            get
            {
                if (private_ == null)
                {
                    //private_ = (RSACryptoServiceProvider)cert_.GetRSAPrivateKey();
                    private_ = (RSACryptoServiceProvider)cert_.PrivateKey;
                }
                return private_;
            }
        }

        public MemoryStream Sign(MemoryStream content)
        {
            var envelope = new SignedCms(new ContentInfo(content.ToArray()));
            var signer = new CmsSigner(signingCert_);
            signer.IncludeOption = X509IncludeOption.WholeChain;
            envelope.ComputeSignature(signer);
            return new MemoryStream(envelope.Encode());
        }

        public MemoryStream Encrypt(MemoryStream clearStream)
        {
            var aes_provider = new AesCryptoServiceProvider();
            aes_provider.GenerateKey();
            aes_provider.GenerateIV();

            var encrypted_key = RSAEncrypt(aes_provider.Key);
            var encrypted_iv = RSAEncrypt(aes_provider.IV);

            var encrypted_content = AESEncrypt(clearStream.ToArray()
                     , aes_provider.Key
                     , aes_provider.IV);

            var encryped_stream = new MemoryStream();
            encryped_stream.Write(encrypted_content, 0, encrypted_content.Length);
            encryped_stream.Write(encrypted_key, 0, encrypted_key.Length);
            encryped_stream.Write(encrypted_iv, 0, encrypted_iv.Length);
            encryped_stream.Write(BitConverter.GetBytes((Int32)encrypted_key.Length), 0, 4);
            encryped_stream.Write(BitConverter.GetBytes((Int32)encrypted_iv.Length), 0, 4);

            return encryped_stream;
        }

        public MemoryStream CheckSignatureAndExtract(MemoryStream envelope)
        {
            X509Store store = new X509Store(CertHelper.StoreName, CertHelper.StoreLocation);
            store.Open(OpenFlags.ReadOnly);
            MemoryStream content = null;
            try
            {
                SignedCms signed = new SignedCms();
                signed.Decode(envelope.ToArray());
                signed.CheckSignature(store.Certificates, true);
                content = new MemoryStream(signed.ContentInfo.Content);
            }
            finally
            {
                store.Close();
            }
            return content;
        }

        public MemoryStream Decrypt(MemoryStream cipherStream)
        {
            if (cipherStream.Length < 4)
            {
                throw new ArgumentException("cipher stream too small");
            }

            byte[] cipher_text = cipherStream.ToArray();

            UInt32 tail = BitConverter.ToUInt32(cipher_text, (int)cipher_text.Length - 4);
            if (tail == 0u)
            {
                // If last 4 bytes 4, this indicates input is not actually ciphertext (internal flag).
                return cipherStream;
            }

            int total_len = cipher_text.Length;
            // Get key and iv length            
            Int32 iv_len = BitConverter.ToInt32(cipher_text, total_len - 4);
            Int32 key_len = BitConverter.ToInt32(cipher_text, total_len - 8);

            // Separate data, key and iv
            byte[] encrypted_key = new byte[key_len];
            byte[] encrypted_iv = new byte[iv_len];
            int encrypted_data_len = total_len - key_len - iv_len - 4 - 4;
            byte[] encrypted_data = new byte[encrypted_data_len];

            // Format: data + key + iv + key_len + iv_len
            Buffer.BlockCopy(cipher_text, 0, encrypted_data, 0, encrypted_data_len);
            Buffer.BlockCopy(cipher_text, encrypted_data_len, encrypted_key, 0, key_len);
            Buffer.BlockCopy(cipher_text, encrypted_data_len + key_len, encrypted_iv, 0, iv_len);
            byte[] key;
            byte[] iv;

            // Can throw CryptographicException
            key = RSADecrypt(encrypted_key);
            iv = RSADecrypt(encrypted_iv);

            byte[] clear_text = AESDecrypt(encrypted_data, key, iv);

            return new MemoryStream(clear_text);
        }

        public byte[] RSADecrypt(byte[] cipherText)
        {
            byte[] clear_text;
            clear_text = PrivateKey.Decrypt(cipherText, true);
            return clear_text;
        }

        public byte[] RSAEncrypt(byte[] clearText)
        {
            byte[] cipher_text;
            cipher_text = PublicKey.Encrypt(clearText, true);
            return cipher_text;
        }

        private byte[] AESDecrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText.Length <= 0)
            {
                throw new ArgumentException("no cipher text");
            }
            if (key.Length <= 0)
            {
                throw new ArgumentException("no key");
            }
            if (iv.Length <= 0)
            {
                throw new ArgumentException("no iv");
            }

            AesCryptoServiceProvider aes_provider = null;
            byte[] clear_text;

            try
            {
                // Create a decrytor to perform the stream transform.
                aes_provider = new AesCryptoServiceProvider();
                aes_provider.KeySize = 256;
                aes_provider.Key = key;
                aes_provider.IV = iv;
                ICryptoTransform decryptor = aes_provider.CreateDecryptor();
                if (decryptor == null)
                {
                    throw new NullReferenceException("could not instantiate decryptor");
                }
                clear_text = ApplyTransform(cipherText, decryptor);
            }
            finally
            {
                if (aes_provider != null)
                    aes_provider.Clear();
            }

            return clear_text;
        }

        private byte[] AESEncrypt(byte[] clearText, byte[] key, byte[] iv)
        {
            if (clearText.Length <= 0)
            {
                throw new ArgumentException("no cipher text");
            }
            if (key.Length <= 0)
            {
                throw new ArgumentException("no key");
            }
            if (iv.Length <= 0)
            {
                throw new ArgumentException("no iv");
            }

            AesCryptoServiceProvider aes_provider = null;
            byte[] cipher_text;

            try
            {
                // Create a decrytor to perform the stream transform.
                aes_provider = new AesCryptoServiceProvider();
                aes_provider.KeySize = 256;
                aes_provider.Key = key;
                aes_provider.IV = iv;
                var encryptor = aes_provider.CreateEncryptor(key, iv);
                if (encryptor == null)
                {
                    throw new NullReferenceException("could not instantiate encryptor");
                }
                cipher_text = ApplyTransform(clearText, encryptor);
            }
            finally
            {
                if (aes_provider != null)
                {
                    aes_provider.Clear();
                }
            }

            return cipher_text;
        }

        private byte[] ApplyTransform(byte[] sourceData, ICryptoTransform transform)
        {
            MemoryStream transformed_buffer = new MemoryStream();
            CryptoStream cs = null;
            try
            {
                cs = new CryptoStream(transformed_buffer, transform, CryptoStreamMode.Write);
                cs.Write(sourceData, 0, sourceData.Length);
            }
            finally
            {
                if (cs != null)
                    cs.Close();
            }
            byte[] transformed_data;
            transformed_data = transformed_buffer.ToArray();
            transformed_buffer.Close();
            return transformed_data;
        }
    }
}
