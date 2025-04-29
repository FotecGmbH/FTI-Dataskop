// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.Apps.Service.Push;
using WebExchange.Interfaces;

namespace WebExchange
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class WebSettings :
        IWebSettingsAzureFiles,
        IAppServiceSettingPush,
        IAppSettingsDataBase,
        IAppSettingsEMail,
        IAppSettingsEncryption
    {
        private static WebSettings _current = null!;

        #region Properties

        #region IAppSettingsEncryption

        /// <summary>
        ///     PrivateKey f. Verschlüsselung
        /// </summary>
        public string SymmetricEncryptionPrivateKey { get; set; } = "ToEnter";

        #endregion IAppSettingsEncryption

        #endregion

        /// <summary>
        ///     Get default Settings for WebSettings
        /// </summary>
        /// <returns></returns>
        public static WebSettings Current()
        {
            if (_current == null!)
            {
                _current = new WebSettings();
            }

            return _current;
        }

        #region IWebSettingsAzureFiles

        /// <summary>
        ///     Connection string für den Blob
        /// </summary>
        public string BlobConnectionString => "ToEnter";

        /// <summary>
        ///     Container Name im Blob
        /// </summary>
        public string BlobContainerName => "ToEnter";

        /// <summary>
        ///     Cdn link oder public Bloblink für Filelink
        /// </summary>
        public string CdnLink => "ToEnter";

        #endregion IWebSettingsAzureFiles

        #region IAppServiceSettingPush

        /// <summary>
        ///     Push - Firebase Project Id - <inheritdoc cref="IAppServiceSettingPush.PushProjectId" />
        /// </summary>
        public string PushProjectId => "ToEnter";

        /// <summary>
        ///     Push - Firebase Service Account Id - <inheritdoc cref="IAppServiceSettingPush.PushServiceAccountId" />
        /// </summary>
        public string PushServiceAccountId => "ToEnter";

        /// <summary>
        ///     Push - Firebase Private Key Id - <inheritdoc cref="IAppServiceSettingPush.PushPrivateKeyId" />
        /// </summary>
        public string PushPrivateKeyId => "ToEnter";

        /// <summary>
        ///     Push - Firebase Private Key - <inheritdoc cref="IAppServiceSettingPush.PushPrivateKey" />
        /// </summary>
        public string PushPrivateKey => "ToEnter";

        #endregion IAppServiceSettingPush

        #region IAppSettingsDataBase

        public string ConnectionString => string.Empty;

        /// <summary>
        ///     Datenbank
        /// </summary>
        public string ConnectionStringDb => "ToEnter";

        /// <summary>
        ///     Datenbank-Server
        /// </summary>
        public string ConnectionStringDbServer => "ToEnter";

        /// <summary>
        ///     Datenbank User
        /// </summary>
        public string ConnectionStringUser => "ToEnter";

        /// <summary>
        ///     Datenbank User Passwort
        /// </summary>
        public string ConnectionStringUserPwd => "ToEnter";

        #endregion IAppSettingsDataBase

        #region IAppSettingsEMail

        /// <summary>
        ///     Als wer (E-Mail Adresse) wird gesendet (für Antworten)
        /// </summary>
        public string SendEMailAs => "ToEnter";

        /// <summary>
        ///     Welcher Name des Senders wird angezeigt
        /// </summary>
        public string SendEMailAsDisplayName => "ToEnter";

        /// <summary>
        ///     Sendgrid Key (falls Sendgrid verwendet wird)
        /// </summary>
        public string SendGridApiKey => "ToEnter";

        #endregion IAppSettingsEMail
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
}