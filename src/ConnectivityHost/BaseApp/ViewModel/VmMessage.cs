// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDA.Service.AppConnectivity.DataConnector;
using BDA.Service.AppConnectivity.Helper;
using BDA.Service.EMail.Services;
using Biss.AppConfiguration;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Dc.Server;
using ConnectivityHost.Helper;
using Exchange;
using WebExchange;

namespace ConnectivityHost.BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Viewmodel fürs Nachricht versenden</para>
    ///     Klasse VmMessage. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewMessage")]
    public class VmMessage : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmMessage.DesignInstance}"
        /// </summary>
        public static VmMessage DesignInstance = new VmMessage();

        /// <summary>
        ///     VmMessage
        /// </summary>
        public VmMessage() : base("Nachricht")
        {
            View.ShowFooter = false;
            View.ShowHeader = true;
            View.ShowBack = true;
            View.ShowMenu = false;
        }

        #region Properties

        /// <summary>
        ///     Razor Engine
        /// </summary>
        public ICustomRazorEngine? RazorEngine { get; set; }

        /// <summary>
        ///     Send via Push Command
        /// </summary>
        public VmCommand CmdSend { get; set; } = null!;

        /// <summary>
        ///     Nachrichten Text
        /// </summary>
        public VmEntry TitleEntry { get; set; } = new(EnumVmEntryBehavior.Instantly, "Überschrift");

        /// <summary>
        ///     Nachrichten Text
        /// </summary>
        public VmEntry MessageEntry { get; set; } = new(EnumVmEntryBehavior.Instantly, "Nachricht");

        /// <summary>
        ///     Daten fürs Versenden der Nachricht.
        /// </summary>
        public SendMessageData Data { get; set; } = new();

        /// <summary>
        ///     Dc Connection
        /// </summary>
        public IDcConnections DcConnections { get; set; } = null!;

        /// <summary>
        ///     Server Remote Calls
        /// </summary>
        public IServerRemoteCalls ServerRemoteCalls { get; set; } = null!;

        #endregion

        /// <summary>
        ///     Wird aufgerufen sobald die View initialisiert wurde
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override Task OnActivated(object? args = null)
        {
            if (args is SendMessageData data)
            {
                Data = data;

                if (Data.SendVia == SendViaEnum.Email)
                {
                    TitleEntry.Title = "Betreff";
                }
            }

            TitleEntry.ValidateFunc = Validate;
            MessageEntry.ValidateFunc = Validate;

            View.BusyClear();
            base.OnActivated(args);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
#pragma warning disable CA1506
        protected override void InitializeCommands()
#pragma warning restore CA1506
        {
            CmdSend = new VmCommand("Senden", async () =>
                {
                    switch (Data.SendVia)
                    {
                        case SendViaEnum.Dc:
                            var successes = 0;
                            var fails = 0;
                            var currentConnectedDeviceIds = DcConnections.GetClients();

                            foreach (var tableDevice in Data.Devices)
                            {
                                if (currentConnectedDeviceIds.All(x => x.DeviceId != tableDevice.Id))
                                {
                                    fails++;
                                    continue;
                                }

                                try
                                {
                                    await ((ServerRemoteCalls) ServerRemoteCalls).SendMessage(MessageEntry.Value, TitleEntry.Value, tableDevice.Id, null).ConfigureAwait(true);
                                    successes++;
                                }
                                catch (Exception)
                                {
                                    fails++;
                                }
                            }

                            _ = await MsgBox.Show($"Es kamen {successes} Benachrichtigungen an und {fails} nicht an!").ConfigureAwait(true);

                            break;

                        case SendViaEnum.Push:
                            var tokens = Data.Devices.Where(x => !string.IsNullOrWhiteSpace(x.DeviceToken)).Select(x => x.DeviceToken).ToList();
                            var success = 0;
                            var fail = 0;
                            if (tokens.Any())
                            {
                                if (tokens.Count > 500)
                                {
#pragma warning disable CS0618 // Type or member is obsolete
                                    var chunks = tokens.Split(500);
#pragma warning restore CS0618 // Type or member is obsolete

                                    foreach (var chunk in chunks)
                                    {
                                        var result = await Push.SendMessageToDevices(TitleEntry.Value, MessageEntry.Value, chunk).ConfigureAwait(true);
                                        success += result.SuccessCount;
                                        fail += result.FailureCount;
                                    }
                                }
                                else
                                {
                                    var result = await Push.SendMessageToDevices(TitleEntry.Value, MessageEntry.Value, tokens).ConfigureAwait(true);
                                    success = result.SuccessCount;
                                    fail = result.FailureCount;
                                }

                                _ = await MsgBox.Show($"Es kamen {success} Benachrichtigungen an und {fail} nicht an!").ConfigureAwait(true);
                            }

                            break;

                        case SendViaEnum.Email:
                            if (RazorEngine == null)
                            {
                                throw new ArgumentNullException(nameof(RazorEngine));
                            }

                            if (!Data.Users.Any())
                            {
                                throw new ArgumentNullException(nameof(Data.Users));
                            }

                            var sentMails = 0;

                            foreach (var user in Data.Users)
                            {
                                var notificationUser = new EMailNotificationUser
                                {
                                    Firstname = user.FirstName,
                                    Lastname = user.LastName,
                                    Text = MessageEntry.Value
                                };

                                var bem = WebConstants.Email;
                                var sender = "biss@fotec.at";
                                var subject = TitleEntry.Value;
                                var receiverBetaInfo = "";
                                var receiver = user.LoginName;
                                List<string> ccReceiver = new();

                                if (Constants.AppConfiguration.CurrentBuildType != EnumCurrentBuildType.CustomerRelease)
                                {
                                    receiverBetaInfo = $"To: {receiver}, CC: ";
                                    foreach (var cc in ccReceiver)
                                    {
                                        receiverBetaInfo += $"{cc}; ";
                                    }

                                    ccReceiver = new();
                                }

                                var htmlRendered = await RazorEngine
                                    .RazorViewToHtmlAsync("Views/EMail/EmailNotificationTest.cshtml", notificationUser).ConfigureAwait(true);

                                // Email wird gesendet
                                var sendResult = await bem.SendHtmlEMail(sender, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceiver).ConfigureAwait(true);
                                if (sendResult)
                                {
                                    sentMails++;
                                }
                            }

                            _ = await MsgBox.Show("Es wurde(n) " + sentMails + " Email(s) versandt!").ConfigureAwait(true);
                            break;
                    }


                    await Nav.Back().ConfigureAwait(true);
                },
                () =>
                    !string.IsNullOrWhiteSpace(TitleEntry.Value) && !string.IsNullOrWhiteSpace(MessageEntry.Value));
        }

        private (string, bool) Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ("Darf nicht leer sein!", true);
            }

            CmdSend.CanExecute();
            return ("", true);
        }
    }
}