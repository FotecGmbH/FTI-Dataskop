// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Security.Cryptography;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace BDA.Service.Encryption
{
    public class SymmetricEncryption : ISymmetricEncryption
    {
        private readonly Aes _aes;

        public SymmetricEncryption(byte[] privateKey)
        {
            if (privateKey == null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            _aes = Aes.Create();

            Setup(privateKey);
        }

        public SymmetricEncryption(string privateKey)
        {
            if (privateKey == null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            _aes = Aes.Create();

            Setup(Convert.FromBase64String(privateKey));
        }

        private void Setup(byte[] privateKey)
        {
            if (_aes.KeySize != privateKey.Length * 8)
            {
                throw new CryptographicException($"Schlüssellänge muss eine Länge von {_aes.KeySize} ({_aes.KeySize / 8} bytes) haben");
            }

            _aes.Key = privateKey;
        }

        #region Interface Implementations

        public string Encrypt(string plainText)
        {
            _aes.GenerateIV();
            var iv = _aes.IV;
            var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);

            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            var encrypted = msEncrypt.ToArray();

            // Kombiniere IV mit verschlüsseltem Text
            var combinedData = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, combinedData, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, combinedData, iv.Length, encrypted.Length);

            return Convert.ToBase64String(combinedData);
        }

        public string Decrypt(string encryptedData)
        {
            try
            {
                var combinedData = Convert.FromBase64String(encryptedData);

                // Splitte IV und verschlüsselten Text
                var iv = combinedData.Take(_aes.IV.Length).ToArray();
                var data = combinedData.Skip(iv.Length).ToArray();

                _aes.IV = iv;

                var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV);

                using var msDecrypt = new MemoryStream(data);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                return encryptedData;
            }
        }

        #endregion
    }
}