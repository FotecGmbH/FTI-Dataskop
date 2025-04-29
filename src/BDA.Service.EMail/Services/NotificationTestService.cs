// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.AppConfiguration;
using Biss.Apps.Service.Push;
using Biss.Dc.Core;
using Biss.Dc.Server;
using Biss.Log.Producer;
using Database;
using Database.Tables;
using Exchange;
using WebExchange;

namespace BDA.Service.EMail.Services
{
    /// <summary>
    ///     Notification Test Service
    /// </summary>
    public class NotificationTestService
    {
        /// <summary>
        ///     Send Test Email
        /// </summary>
        /// <param name="razorEngine"></param>
        /// <param name="user"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<bool> SendTestEmail(ICustomRazorEngine razorEngine, TableUser user, string text)
        {
            if (razorEngine == null)
            {
                throw new ArgumentNullException($"[{nameof(NotificationTestService)}]({nameof(SendTestEmail)}): {nameof(razorEngine)} is null");
            }

            if (user == null)
            {
                throw new ArgumentNullException($"[{nameof(NotificationTestService)}]({nameof(SendTestEmail)}): {nameof(user)} is null");
            }

            var notificationUser = new EMailNotificationUser
            {
                Firstname = user.FirstName,
                Lastname = user.LastName,
                Text = text
            };

            var bem = WebConstants.Email;
            var sender = "biss@fotec.at";
            var subject = "Notification Test!";
            var receiverBetaInfo = "";
            // ReSharper disable once UnusedVariable
            var receiver = user.LoginName;
            List<string> ccReceifer = new()
            {
                "benni.moser@outlook.de"
            };
            Logging.Log.LogInfo($"[{nameof(NotificationTestService)}]({nameof(SendTestEmail)}): Send Notification Test Mail to: " + user.LoginName);
            if (Constants.AppConfiguration.CurrentBuildType != EnumCurrentBuildType.CustomerRelease)
            {
                //receiverBetaInfo = $"To: {receiver}, CC: ";
                //foreach (var cc in ccReceifer)
                //{
                //    receiverBetaInfo += $"{cc}; ";
                //}

                //ccReceifer = new();
            }

            var htmlRendered = await razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailNotificationTest.cshtml", notificationUser).ConfigureAwait(true);

            // Email wird gesendet
            var sendResult = await bem.SendHtmlEMail(sender, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);
            return sendResult;
        }

        /// <summary>
        ///     Sends Test Dc Notification
        /// </summary>
        /// <param name="dcConnection"></param>
        /// <param name="user"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<int> SendTestDcNotification(IDcConnections dcConnection, TableUser? user, string text)
        {
            if (dcConnection == null)
            {
                throw new ArgumentNullException($"[{nameof(NotificationTestService)}]({nameof(SendTestDcNotification)}): {nameof(dcConnection)} is null");
            }

            if (user == null)
            {
                return await dcConnection.SendCommonData(new DcCommonData {Key = "Test", Value = text}).ConfigureAwait(true);
            }


            // ReSharper disable once RedundantAssignment
            var deviceIds = new List<long>();
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            deviceIds = db.TblDevices.Where(d => d.TblUserId == user.Id).Select(d => d.Id).ToList();

            var counter = 0;
            foreach (var deviceId in deviceIds)
            {
                var result = await dcConnection.SendCommonDataToDevice(deviceId, new DcCommonData {Key = "Test", Value = text}).ConfigureAwait(true);
                if (result)
                {
                    counter++;
                }
            }

            return counter;
        }

        /// <summary>
        ///     Send Test Push Notification
        /// </summary>
        /// <param name="push"></param>
        /// <param name="user"></param>
        /// <param name="title"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<PushResult> SendTestPushNotification(PushService push, TableUser? user, string title, string value)
        {
            PushResult result;

            if (push == null)
            {
                throw new ArgumentNullException($"[{nameof(NotificationTestService)}]({nameof(SendTestPushNotification)}): {nameof(push)} is null");
            }

            if (user == null)
            {
                result = await push.SendBroadcast(title, value).ConfigureAwait(true);
                return result;
            }

            // ReSharper disable once RedundantAssignment
            List<string> pushTokens = new List<string>();
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            pushTokens = db.TblDevices.Where(d => d.TblUserId == user.Id).Select(t => t.DeviceToken).ToList();

            if (pushTokens.Count > 0)
            {
                return await push.SendMessageToDevices(title, value, pushTokens).ConfigureAwait(true);
            }

            return new PushResult(0, 0, new List<string>());
        }
    }
}