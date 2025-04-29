// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

namespace BDA.Service.Encryption
{
    public interface ISymmetricEncryption
    {
        /// <summary>
        ///     Entschlüsselt Daten
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        string Decrypt(string encryptedData);

        /// <summary>
        ///     Verschlüsselt Text
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        string Encrypt(string plainText);
    }
}