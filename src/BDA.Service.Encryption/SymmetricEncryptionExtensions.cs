// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BDA.Service.Encryption
{
    /// <summary>
    ///     <para>Encryption Extension</para>
    ///     Klasse SymmetricEncryptionExtensions. (C) 2024 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class SymmetricEncryptionExtensions
    {
        /// <summary>
        ///     Adds the Encryption with the given private key
        /// </summary>
        /// <param name="services"></param>
        /// <param name="privateKey"></param>
        public static void AddSymmetricEncryption(this IServiceCollection services, string privateKey)
        {
            services.AddSingleton<ISymmetricEncryption>(new SymmetricEncryption(privateKey));
        }

        /// <summary>
        ///     Adds the Encryption with the given private key
        /// </summary>
        /// <param name="services"></param>
        /// <param name="privateKey"></param>
        public static void AddSymmetricEncryption(this IServiceCollection services, byte[] privateKey)
        {
            services.AddSingleton<ISymmetricEncryption>(new SymmetricEncryption(privateKey));
        }

        #region BDA Extensions

        /// <summary>
        ///     Verschlüsselt das Property AdditionalConfiguration
        /// </summary>
        /// <param name="symmetricEncryption"></param>
        /// <param name="data"></param>
        public static void EncryptAdditionalConfiguration(this ISymmetricEncryption symmetricEncryption, IAdditionalConfiguration data)
        {
            if (symmetricEncryption == null! || data == null!)
            {
                return;
            }

            symmetricEncryption.EncryptDecryptAdditionalConfiguration(data, true);
        }

        /// <summary>
        ///     Entschlüsselt das Property AdditionalConfiguration
        /// </summary>
        /// <param name="symmetricEncryption"></param>
        /// <param name="data"></param>
        public static void DecryptAdditionalConfiguration(this ISymmetricEncryption symmetricEncryption, IAdditionalConfiguration data)
        {
            if (symmetricEncryption == null! || data == null!)
            {
                return;
            }

            symmetricEncryption.EncryptDecryptAdditionalConfiguration(data, false);
        }

        private static void EncryptDecryptAdditionalConfiguration(this ISymmetricEncryption symmetricEncryption, IAdditionalConfiguration data, bool encrypt)
        {
            if (string.IsNullOrEmpty(data.AdditionalConfiguration))
            {
                return;
            }

            if (encrypt)
            {
                try
                {
                    //test if json -> if no exception, then it is json and not encrypted -> encryption necessary
                    JsonConvert.DeserializeObject<object>(data.AdditionalConfiguration);

                    data.AdditionalConfiguration = symmetricEncryption.Encrypt(data.AdditionalConfiguration);
                }
                catch
                {
                    //if exception, then it is already encrypted (avoid double encryption)
                }
            }
            else
            {
                try
                {
                    //test if json -> if no exception, then it is json and not encrypted (for old (not encrypted) data in Db)
                    JsonConvert.DeserializeObject<object>(data.AdditionalConfiguration);
                }
                catch
                {
                    //if exception, then it is encrypted
                    data.AdditionalConfiguration = symmetricEncryption.Decrypt(data.AdditionalConfiguration);
                }
            }
        }

        #endregion
    }
}