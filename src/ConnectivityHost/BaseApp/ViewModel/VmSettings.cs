// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using BDA.Common.Exchange.Enum;
using BDA.Service.AppConnectivity.DataConnector;
using BDA.Service.AppConnectivity.Helper;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Dc.Server;
using Database;
using Exchange.Resources;

namespace ConnectivityHost.BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Viewmodel für Einstellungen</para>
    ///     Klasse VmSettings. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewSettings")]
    public class VmSettings : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmSettings.DesignInstance}"
        /// </summary>
        public static VmSettings DesignInstance = new VmSettings();

        /// <summary>
        ///     VmSettings
        /// </summary>
        public VmSettings() : base("Einstellungen")
        {
            View.ShowFooter = false;
            View.ShowHeader = true;
            View.ShowBack = false;
            View.ShowMenu = true;
            View.ShowSubTitle = true;
        }

        #region Properties

        /// <summary>
        ///     Aktuelle Einstellungen
        /// </summary>
        public CurrentSettings Settings { get; set; } = new CurrentSettings();

        /// <summary>
        ///     AGB Entry
        /// </summary>
        public VmEntry EntryAgb { get; set; } = null!;

        /// <summary>
        ///     Aktuelle App Version
        /// </summary>
        public VmEntry EntryCurrentAppVersion { get; set; } = null!;

        /// <summary>
        ///     Minimale App Version
        /// </summary>
        public VmEntry EntryMinAppVersion { get; set; } = null!;

        /// <summary>
        ///     Allgemeine Nachricht
        /// </summary>

        public VmEntry EntryCommonMessage { get; set; } = null!;

        /// <summary>
        ///     Einstellungen speichern und per DC versenden
        /// </summary>
        public VmCommand CmdSendPerDc { get; set; } = null!;

        /// <summary>
        ///     Verbindung DC
        /// </summary>
        public IDcConnections DcConnection { get; set; } = null!;

        /// <summary>
        ///     Server Remote Calls
        /// </summary>
        public IServerRemoteCalls ServerRemoteCalls { get; set; } = null!;

        #endregion


        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            Settings = new CurrentSettings
            {
                Agb = CurrentSettingsInDb.Current.AgbString,
                CommonMessage = CurrentSettingsInDb.Current.CommonMessage,
                CurrentAppVersion = CurrentSettingsInDb.Current.CurrentAppVersionString,
                MinAppVersion = CurrentSettingsInDb.Current.MinAppVersionString
            };

            EntryAgb = new VmEntry(EnumVmEntryBehavior.LostFocus, "AGB", "", Settings, nameof(Settings.Agb));
            EntryCurrentAppVersion = new VmEntry(EnumVmEntryBehavior.Instantly, "Aktuelle App Version", "", Settings, nameof(Settings.CurrentAppVersion), ValidateCurrentAppVersionFunc);
            EntryMinAppVersion = new VmEntry(EnumVmEntryBehavior.Instantly, "Min App Version", "", Settings, nameof(Settings.MinAppVersion), ValidateMinAppFunc);
            EntryCommonMessage = new VmEntry(EnumVmEntryBehavior.LostFocus, "Allgemeine Nachricht", "", Settings, nameof(Settings.CommonMessage));

            CmdSendPerDc = new VmCommand("Einstellungen speichern und per DC übertragen", async () =>
                {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                    await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

                    CurrentSettingsInDb.UpdateElement(EnumDbSettings.Agb, Settings.Agb, db);
                    CurrentSettingsInDb.UpdateElement(EnumDbSettings.CommonMessage, Settings.CommonMessage, db);
                    CurrentSettingsInDb.UpdateElement(EnumDbSettings.CurrentAppVersion, Settings.CurrentAppVersion, db);
                    CurrentSettingsInDb.UpdateElement(EnumDbSettings.MinAppVersion, Settings.MinAppVersion, db);

                    var cnt = await ((ServerRemoteCallBase) ServerRemoteCalls).SendDcExSettingsInDb(CurrentSettingsInDb.Current).ConfigureAwait(true);
                    await MsgBox.Show($"Einstellungen wurden gespeichert und an {cnt} Geräte übertragen.").ConfigureAwait(true);
                }, glyph: Glyphs.Send_email,
                canExecuteNoParams: () => EntryCurrentAppVersion.DataValid && EntryMinAppVersion.DataValid);
        }

        /// <summary>
        ///     Nichts validieren
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private (string hint, bool valid) ValidateNothing(string arg)
        {
            return ("", true);
        }

        /// <summary>
        ///     Validierung für Min Version
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private (string hint, bool valid) ValidateMinAppFunc(string arg)
        {
            try
            {
                var minVersion = new Version(arg);

                var currentVersion = new Version(Settings.CurrentAppVersion);

                if (currentVersion < minVersion)
                {
                    return ("Min. Version darf nicht höher sein als die aktuelle Version.", false);
                }
            }
            catch (Exception)
            {
                return ("Keine gültige Versionsnummer.", false);
            }

            return ("", true);
        }

        /// <summary>
        ///     Validierung für Min Version
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private (string hint, bool valid) ValidateCurrentAppVersionFunc(string arg)
        {
            try
            {
                var minVersion = new Version(Settings.MinAppVersion);

                var currentVersion = new Version(arg);

                if (currentVersion < minVersion)
                {
                    return ("Min. Version darf nicht höher sein als die aktuelle Version.", false);
                }
            }
            catch (Exception)
            {
                return ("Keine gültige Versionsnummer.", false);
            }

            return ("", true);
        }
    }
}