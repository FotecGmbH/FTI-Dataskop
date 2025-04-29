// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.AppConfiguration;
using Biss.Apps.Base;
using Biss.Log.Producer;
using Database;
using Database.Tables;
using Exchange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebExchange;

namespace BDA.Service.EMail.Services
{
    /// <summary>
    ///     <para>Service für Benutzer-Accounts</para>
    ///     Klasse UserAccountEmailService. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class UserAccountEmailService
    {
        private readonly ICustomRazorEngine _razorEngine;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="razorEngine">RazorEngine</param>
        public UserAccountEmailService(ICustomRazorEngine razorEngine)
        {
            _razorEngine = razorEngine;
        }

        /// <summary>
        ///     Sendet Bestätigungemail nach Registrierung
        /// </summary>
        /// <param name="user"></param>
        /// <param name="deviceId">device Id von dem die Anforderung gekommen ist</param>
        /// <param name="db">Datenbank</param>
        /// <param name="route">Route (Methode des Controllers)</param>
        /// <returns></returns>
        public async Task<bool> SendValidationMail(TableUser user, long deviceId, Db db, string route = "UserValidateEMail")
        {
            if (db == null)
            {
                throw new ArgumentNullException($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMail)}): {nameof(db)} is null");
            }

            if (user == null)
            {
                throw new ArgumentNullException($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMail)}): {nameof(user)} is null");
            }

            // Notwendige Parameter werden gesetzt
            var bem = WebConstants.Email;
            var sender = "biss@fotec.at";
            var subject = "Bitte bestätigen Sie ihre Registrierung";
            var receiverBetaInfo = "";
            List<string> ccReceifer = new();
            var receiver = user.LoginName;

            user.ConfirmationToken = Guid.NewGuid().ToString("N");
            _ = await db.SaveChangesAsync().ConfigureAwait(true);

            var url = $"{AppSettings.Current().SaApiHost}/{route}/{deviceId}/{user.Id}/{user.ConfirmationToken}";
            Logging.Log.LogInfo($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMail)}): url: {url}");

            // Für Beta Versionen
            if (Constants.AppConfiguration.CurrentBuildType != EnumCurrentBuildType.CustomerRelease)
            {
                receiverBetaInfo = $"To: {receiver}, CC: ";
                foreach (var cc in ccReceifer)
                {
                    receiverBetaInfo += $"{cc}; ";
                }

                ccReceifer = new();
            }

            var user4Mail = new EMailModelUser
            {
                Firstname = user.FirstName,
                Lastname = user.LastName,
                Link = url
            };

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailVerification.cshtml", user4Mail).ConfigureAwait(true);

            // Email wird gesendet
            var sendResult = await bem.SendHtmlEMail(sender, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);
            return sendResult;
        }

        /// <summary>
        ///     Sendet Bestätigungemail nach Benutzeranlage
        /// </summary>
        /// <param name="user"></param>
        /// <param name="db">Datenbank</param>
        /// <param name="password">Das generierte Passwort</param>
        /// <param name="route">Route (Methode des Controllers)</param>
        /// <returns></returns>
        public async Task<bool> SendValidationMailWithoutDevice(TableUser user, Db db, string password, string route = "UserValidateEMail")
        {
            if (db == null)
            {
                throw new ArgumentNullException($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMailWithoutDevice)}): {nameof(db)} is null");
            }

            if (user == null)
            {
                throw new ArgumentNullException($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMailWithoutDevice)}): {nameof(user)} is null");
            }

            // Notwendige Parameter werden gesetzt
            var bem = WebConstants.Email;
            var sender = "biss@fotec.at";
            var subject = "Bitte bestätigen Sie ihre Registrierung";
            var receiverBetaInfo = "";
            List<string> ccReceifer = new();
            var receiver = user.LoginName;

            user.ConfirmationToken = Guid.NewGuid().ToString("N");
            _ = await db.SaveChangesAsync().ConfigureAwait(true);

            var url = $"{AppSettings.Current().SaApiHost}{route}/{user.Id}/{user.ConfirmationToken}";
            Logging.Log.LogInfo($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMailWithoutDevice)}): url: {url}");

            // Für Beta Versionen
            if (Constants.AppConfiguration.CurrentBuildType != EnumCurrentBuildType.CustomerRelease)
            {
                receiverBetaInfo = $"To: {receiver}, CC: ";
                foreach (var cc in ccReceifer)
                {
                    receiverBetaInfo += $"{cc}; ";
                }

                ccReceifer = new();
            }

            var user4Mail = new EMailModelUser
            {
                Firstname = user.FirstName,
                Lastname = user.LastName,
                Username = user.LoginName,
                Link = url,
                Password = password
            };

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailVerification.cshtml", user4Mail).ConfigureAwait(true);

            // Email wird gesendet
            var sendResult = await bem.SendHtmlEMail(sender, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);
            return sendResult;
        }

        /// <summary>
        ///     Passwort zurücksetzen
        /// </summary>
        /// <param name="user">Benutzer</param>
        /// <param name="db">Datenbank</param>
        /// ///
        /// <param name="route">Route (Methode des Controllers)</param>
        /// <returns></returns>
        public async Task<bool> SendPasswordResetMail(TableUser user, Db db, string route = "UserResetPassword")
        {
            if (user == null)
            {
                throw new ArgumentNullException($"[{nameof(UserAccountEmailService)}]({nameof(SendPasswordResetMail)}): {nameof(user)} is null");
            }

            if (db == null)
            {
                throw new ArgumentNullException($"[{nameof(UserAccountEmailService)}]({nameof(SendPasswordResetMail)}): {nameof(db)} is null");
            }

            // Nötige Daten werden gesetzt
            var sender = "biss@fotec.at";
            var subject = "Setzen Sie Ihr Passwort zurück!";
            var receiverBetaInfo = "";
            var ccReceifer = new List<string>();

            Logging.Log.LogInfo($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMail)}): UserStartResetPassword {user.Id}");

            user.ConfirmationToken = Guid.NewGuid().ToString("N");
            await db.SaveChangesAsync().ConfigureAwait(true);

            // PW Reset Url wird generiert
            var url = $"{AppSettings.Current().SaApiHost}{route}/{user.Id}/{user.ConfirmationToken}";
            Logging.Log.LogInfo($"[{nameof(UserAccountEmailService)}]({nameof(SendPasswordResetMail)}): Send StartResetPassword url: {url}");
            var bem = WebConstants.Email;

            // Für Darstellung nötige Daten werden zusammen gesammelt
            var emailModel = new EMailModelUser {Firstname = user.FirstName, Lastname = user.LastName, Link = url};

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailPasswordReset.cshtml", emailModel).ConfigureAwait(true);

            // Senden der Email
            var result = await bem.SendHtmlEMail(sender, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);

            return result;
        }

        /// <summary>
        ///     Erfolgsmeldung nach dem Zurücksetzen des Passwortes versenden
        /// </summary>
        /// <param name="user">Benutzer</param>
        /// <returns></returns>
        public async Task<bool> SendPasswordResetConfirmationMail(TableUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Nötige Daten werden gesetzt
            var sender = "biss@fotec.at";
            var subject = "Passwort wurde geändert!";
            var receiverBetaInfo = "";
            var ccReceifer = new List<string>();

            Logging.Log.LogInfo($"[{nameof(UserAccountEmailService)}]({nameof(SendPasswordResetConfirmationMail)}): UserStartResetPassword {user.Id}");

            var newPwd = AppCrypt.GeneratePassword(5);

            //Passwort wird in der Datenbankgeändert und gespeichert
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var data = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == user.Id).ConfigureAwait(true);

            data!.PasswordHash = AppCrypt.CumputeHash(newPwd);
            data.ConfirmationToken = string.Empty;

            // data.RefreshToken = string.Empty;
            // data.JwtToken = string.Empty;

            try
            {
                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(UserAccountEmailService)}]({nameof(SendPasswordResetConfirmationMail)}): UserResetPassword: {e}");
                return false;
            }

            Logging.Log.LogInfo($"[{nameof(UserAccountEmailService)}]({nameof(SendPasswordResetConfirmationMail)}): Send UserResetPassword");

            // Benötigte Daten für die Darstellung werden zusammengesammelt 
            var bem = WebConstants.Email;

            // Für Darstellung nötige Daten werden zusammen gesammelt
            var emailModel = new EMailModelUser {Firstname = user.FirstName, Lastname = user.LastName, Password = newPwd};

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailPasswordChanged.cshtml", emailModel).ConfigureAwait(true);

            // Email wird gesendet
            var result = await bem.SendHtmlEMail(sender, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);

            return result;
        }
    }
}