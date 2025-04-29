// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.AppConfiguration;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Log.Producer;
using Exchange;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;

namespace BaseApp.ViewModel.Infrastructure
{
    #region Hilfsklassen

    /// <summary>
    ///     <para>GwSecretHelper</para>
    ///     Klasse GwSecretHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class GwSecretHelper
    {
        /// <summary>
        ///     GwSecretHelper
        /// </summary>
        /// <param name="gateway"></param>
        public GwSecretHelper(DcListDataPoint<ExGateway> gateway)
        {
            if (gateway == null!)
            {
                throw new ArgumentNullException($"[{nameof(GwSecretHelper)}]({nameof(GwSecretHelper)}): {nameof(gateway)}");
            }

            Gateway = gateway;
            var c = gateway.Data.DeviceCommon.Secret.Split('-');
            var s = string.Empty;
            for (var i = 0; i < c.Length; i++)
            {
                if (i == c.Length - 1)
                {
                    s += new string('*', c[i].Length);
                    Secret = c[i];
                }
                else
                {
                    s += c[i] + "-";
                }
            }

            Name = $"{Gateway.Data.Information.Name} ({s})";
        }

        #region Properties

        /// <summary>
        ///     Für Developer Version
        /// </summary>
        public string Secret { get; } = string.Empty;

        /// <summary>
        ///     Sichtbarer Name im Dropdown
        /// </summary>
        public string Name { get; } = string.Empty;

        /// <summary>
        ///     Gateway
        /// </summary>
        public DcListDataPoint<ExGateway> Gateway { get; }

        #endregion

        /// <summary>
        ///     Prüfung
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public bool Check(string secret)
        {
            return string.Equals(Secret, secret, StringComparison.InvariantCulture);
        }
    }

    #endregion

    /// <summary>
    ///     <para>Gateway einer Firma zuweisen</para>
    ///     Klasse VmAddGatewayToCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddGatewayToCompany", true)]
    public class VmAddGatewayToCompany : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddGatewayToCompany.DesignInstance}"
        /// </summary>
        public static VmAddGatewayToCompany DesignInstance = new VmAddGatewayToCompany();

        /// <summary>
        ///     VmAddGatewayToCompany
        /// </summary>
        public VmAddGatewayToCompany() : base("Gateway", subTitle: "Gateway einer Firma zuweisen")
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Eingegebnes Secret des User
        /// </summary>
        public string SecretBlock { get; set; } = string.Empty;

        /// <summary>
        ///     Picker für die Firma
        /// </summary>
        public VmPicker<GwSecretHelper> PickerGateways { get; } = new VmPicker<GwSecretHelper>(nameof(PickerGateways));

        /// <summary>
        ///     Entry für GatewaySecret
        /// </summary>
        public VmEntry EntryGatewaySecret { get; private set; } = null!;

        /// <summary>
        ///     Gatway Software laden
        /// </summary>
        public VmCommand CmdDownloadGateway { get; private set; } = null!;

        /// <summary>
        ///     Anzeige Dropdoen und Entry
        /// </summary>
        public bool ShowInputs { get; set; }

        /// <summary>
        ///     Secret in Zwischenablage kopieren
        /// </summary>
        public VmCommand CmdDevHepler { get; private set; } = null!;

        #endregion

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            Dc.DcExGateways.LoadingFromHostEvent += DcExGatewaysOnLoadingFromHostEvent;
            Dc.DcExGateways.CollectionChanged += DcExGatewaysOnCollectionChanged;

            return base.OnLoaded();
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            await base.OnActivated(args).ConfigureAwait(false);
            ShowInputs = await PickerGatewaysUpdate().ConfigureAwait(true);

            EntryGatewaySecret = new VmEntry(EnumVmEntryBehavior.StopTyping,
                "Letzter Block des Secrets:",
                "Letzter Block des Secrets",
                this,
                nameof(SecretBlock),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: false
            );
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdDevHepler = new VmCommand("", async () =>
            {
                if (PickerGateways.SelectedItem!.Key == null!)
                {
                    return;
                }

                if (DeviceInfo.Plattform == EnumPlattform.XamarinWpf)
                {
                    var powershell = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-command \"Set-Clipboard -Value \\\"{PickerGateways.SelectedItem.Key.Secret}\\\"\"",
                            CreateNoWindow = true
                        }
                    };
                    powershell.Start();
                    powershell.WaitForExit();
                }
                else
                {
                    await Clipboard.SetTextAsync($"{PickerGateways.SelectedItem.Key.Secret}").ConfigureAwait(true);
                }

                await MsgBox.Show("Secret in Zwischenablage ;-)").ConfigureAwait(true);
            }, glyph: Glyphs.Copy_paste);

            View.CmdSaveHeader = new VmCommand("Sichern", async () =>
            {
                if (!EntryGatewaySecret.DataValid)
                {
                    await MsgBox.Show("Speichern leider nicht möglich!", "Eingabefehler").ConfigureAwait(false);
                    return;
                }

                if (!PickerGateways.SelectedItem!.Key.Check(SecretBlock))
                {
                    await MsgBox.Show("Block ungültig").ConfigureAwait(true);
                    return;
                }

                Dc.DcExGateways.LoadingFromHostEvent -= DcExGatewaysOnLoadingFromHostEvent;
                Dc.DcExGateways.CollectionChanged -= DcExGatewaysOnCollectionChanged;

                PickerGateways.SelectedItem.Key.Gateway.Data.CompanyId = Dc.DcExCompanies.SelectedItem!.Index;
                var r = await PickerGateways.SelectedItem.Key.Gateway.StoreData().ConfigureAwait(true);
                if (!r.DataOk)
                {
                    Logging.Log.LogWarning($"[VmEditDcListPoint]({nameof(InitializeCommands)}): {r.ErrorType}-{r.ServerExceptionText}");

                    var msg = "Speichern leider nicht möglich!";
                    if (Constants.AppConfiguration.CurrentBuildType == EnumCurrentBuildType.Developer)
                    {
                        msg += $"\r\n{r.ErrorType}\r\n{r.ServerExceptionText}";
                    }

                    await MsgBox.Show(msg, "Serverfehler").ConfigureAwait(false);
                    Dc.DcExGateways.LoadingFromHostEvent += DcExGatewaysOnLoadingFromHostEvent;
                    Dc.DcExGateways.CollectionChanged += DcExGatewaysOnCollectionChanged;
                    return;
                }

                await Nav.Back().ConfigureAwait(true);
            }, glyph: Glyphs.Floppy_disk);
            View.CmdSaveHeader.IsVisible = true;

            CmdDownloadGateway = new VmCommand("Gateway Download", async () => { await MsgBox.Show("Erst in BETA").ConfigureAwait(true); });
        }

        /// <summary>
        ///     DcExGatewaysOnCollectionChanged - Neue GW vom Service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExGatewaysOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DcExGatewaysOnLoadingFromHostEvent(this, true);
        }

        /// <summary>
        ///     DcExGatewaysOnLoadingFromHostEvent - Neue GW vom Service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DcExGatewaysOnLoadingFromHostEvent(object sender, bool e)
        {
            ShowInputs = await PickerGatewaysUpdate().ConfigureAwait(true);
        }

        /// <summary>
        ///     Liste aktualisieren
        /// </summary>
        /// <returns></returns>
        private async Task<bool> PickerGatewaysUpdate()
        {
            PickerGateways.Clear();

            var noCompanyId = Dc.DcExCompanies.First(c => c.Data.CompanyType == EnumCompanyTypes.NoCompany).Index;
            var gw = Dc.DcExGateways.Where(g => g.Data.CompanyId == noCompanyId).ToList();

            if (gw == null || gw.Count == 0)
            {
                await MsgBox.Show("Keine neuen Gateways vorhanden.\r\nBitte zuerst einen neuen Gateway installieren.").ConfigureAwait(true);
                return false;
            }

            foreach (var gateway in gw)
            {
                var gwHelper = new GwSecretHelper(gateway);
                PickerGateways.AddKey(new GwSecretHelper(gateway), gwHelper.Name);
            }

            PickerGateways.SelectKey(PickerGateways.First().Key);

            return true;
        }
    }
}