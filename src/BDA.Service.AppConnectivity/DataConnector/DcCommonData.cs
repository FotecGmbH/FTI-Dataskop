// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using BDA.Service.AppConnectivity.Helper;
using BDA.Service.EMail.Services;
using BDA.Service.Encryption;
using Biss.Apps.Service.Push;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Database;
using Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BDA.Service.AppConnectivity.DataConnector
{
    /// <summary>
    ///     <para>Allgemeine "Remote" Funktionen via DC</para>
    ///     Klasse DcCommonData. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        /// <summary>
        ///     Datei löschen aus DB und aus Blob
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task DeleteFile(string data)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var fileId = long.Parse(data);
            await FilesDbBlob.DeleteFile(db, fileId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Gateway Datan haben sich geändert (Quelle egal!) - alle User des Gateways welche gerade mit dem Service verbunden
        ///     sind informieren
        /// </summary>
        /// <param name="gatewayId">Id</param>
        /// <returns></returns>
        public async Task GatewayDataChanged(long gatewayId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var tmp = db.TblGateways.First(f => f.Id == gatewayId).ToExGateway();

            _symmetricEncryption.DecryptAdditionalConfiguration(tmp);

            var update = new DcServerListItem<ExGateway>
            {
                Data = tmp,
                Index = gatewayId,
                SortIndex = gatewayId
            };

            var usersToInform = db.TblUsers.Where(u =>
                u.IsAdmin ||
                u.TblPermissions.Any(p => p.TblCompany.CompanyType == EnumCompanyTypes.PublicCompany) ||
                u.TblPermissions.Any(p2 => p2.TblCompany.TblGateways.Any(g => g.Id == gatewayId)
                )).Select(i => i.Id).ToList().Distinct();

            foreach (var u in usersToInform)
            {
                if (ClientConnection.GetClients().Any(c => c.UserId == u))
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    await SendDcExGateways(new List<DcServerListItem<ExGateway>> {update}, userId: u).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }
        }

        /// <summary>
        ///     Bestätigungs E-Mail erneut senden
        /// </summary>
        /// <param name="data"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private async Task ResendAccessEMail(string data, long deviceId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var userId = long.Parse(data);
            var dbUser = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

            var email = new UserAccountEmailService(_razorEngine);
            await email.SendValidationMail(dbUser!, deviceId, db).ConfigureAwait(false);
        }

        /// <summary>
        ///     Bestätigungs E-Mail erneut senden
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task ResetPassword(string data)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var userId = long.Parse(data);
            var dbUser = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

            var email = new UserAccountEmailService(_razorEngine);
            await email.SendPasswordResetMail(dbUser!, db).ConfigureAwait(false);
        }

        /// <summary>
        ///     Push (Test)Nachricht senden
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task SendPush(string data)
        {
            await PushService.Instance.SendMessageToDevice("Testnachricht", $"Testnachricht gesendet um {DateTime.Now.ToShortTimeString()} an Device: {data}", data).ConfigureAwait(false);
        }

        #region Interface Implementations

        /// <summary>Allgemeine Daten vom Device empfangen</summary>
        /// <param name="deviceId">Geräte Id</param>
        /// <param name="userId">Benutzer Id</param>
        /// <param name="data">Daten</param>
        /// <returns></returns>
        public async Task<string> ReceivedDcCommonData(long deviceId, long userId, DcCommonData data)
        {
            if (data == null!)
            {
                throw new InvalidEnumArgumentException();
            }

            Logging.Log.LogTrace($"[{nameof(DcCommonData)}]({nameof(ReceivedDcCommonData)}): UserId {userId} from device {deviceId} key {data.Key}");

            if (!Enum.TryParse(data.Key, true, out EnumDcCommonCommands command))
            {
                throw new InvalidEnumArgumentException($"[{nameof(DcCommonData)}]({nameof(ReceivedDcCommonData)}): Key {data.Key} is not a valid Member of EnumDcCommonCommands");
            }

            switch (command)
            {
                case EnumDcCommonCommands.FileDelete:
                    await DeleteFile(data.Value).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.ResendAccessEMail:
                    await ResendAccessEMail(data.Value, deviceId).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.ResetPassword:
                    await ResetPassword(data.Value).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.SendTestPush:
                    await SendPush(data.Value).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"[{nameof(DcCommonData)}]({nameof(ReceivedDcCommonData)}): {nameof(command)} out of range");
            }

            return string.Empty;
        }

        #endregion
    }
}