// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using Biss.Apps.Attributes;
using Biss.Apps.Base;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Passwort verändern</para>
    ///     Klasse VmEditUserPassword. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditUserPassword")]
    public class VmEditUserPassword : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditUserPassword.DesignInstance}"
        /// </summary>
        public static VmEditUserPassword DesignInstance = new VmEditUserPassword();

        /// <summary>
        ///     VmEditUserPassword
        /// </summary>
        public VmEditUserPassword() : base(ResViewEditUserPassword.LblTitle, subTitle: ResViewEditUserPassword.LblSubTitle)
        {
            SetViewProperties(true);

            EntryCurrentPassword = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUserPassword.EntryTitleCurrentPassword,
                ResViewEditUserPassword.EntryPlaceholderCurrentPassword,
                showTitle: false,
                validateFunc: ValidateFunc,
                returnAction: () =>
                {
                    EntryCurrentPassword?.ValidateData();
                    EntryNewPassword?.Focus(EnumVmEntrySetFocusMode.FocusAndSelect);
                });
            EntryNewPassword = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUserPassword.EntryTitleNewPassword,
                ResViewEditUserPassword.EntryPlaceholderNewPassword,
                showTitle: false,
                validateFunc: ValidateFunc,
                returnAction: () =>
                {
                    EntryNewPassword?.ValidateData();
                    View.CmdSaveHeader!.Execute(null!);
                });

            EntryCurrentPassword.ValidChanged += (sender, args) => View.CmdSaveHeader!.CanExecute();
            EntryNewPassword.ValidChanged += (sender, args) => View.CmdSaveHeader!.CanExecute();
        }

        #region Properties

        /// <summary>
        ///     Aktuelles Passwort
        /// </summary>
        public VmEntry EntryCurrentPassword { get; set; }

        /// <summary>
        ///     Neues Passwort
        /// </summary>
        public VmEntry EntryNewPassword { get; set; }

        /// <summary>
        ///     In der View die Eingaben als Passwortfelder anzeigen
        /// </summary>
        public bool ShowEntriesAsPassword { get; set; } = true;

        #endregion

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            View.CmdSaveHeader = new VmCommand("Speichern", async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                Dc.DcExUserPassword.Data.CurrentPasswordHash = AppCrypt.CumputeHash(EntryCurrentPassword.Value);
                Dc.DcExUserPassword.Data.NewPasswordHash = AppCrypt.CumputeHash(EntryNewPassword.Value);
                var r = await Dc.DcExUserPassword.StoreData(true).ConfigureAwait(true);
                if (!r.DataOk)
                {
                    Logging.Log.LogError($"[{nameof(VmEditUserPassword)}]({nameof(View.CmdSaveHeader)}-Command): StoreData result is false. Type: {r.ErrorType} Msg: {r.ServerExceptionText}");
                    await MsgBox.Show(ResViewEditUserPassword.MsgChangeError, ResViewEditUserPassword.MsgTitleChangeError).ConfigureAwait(true);
                }
                else
                {
                    await Nav.Back().ConfigureAwait(true);
                }
            }, CanExecuteSaveCommand, glyph: Glyphs.Floppy_disk);
        }

        /// <summary>
        ///     Eingabevalidierung für die beiden Passwort-Felder
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private (string hint, bool valid) ValidateFunc(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     Kann Speichern gedrückt werden?
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSaveCommand()
        {
            if (EntryCurrentPassword == null! || EntryNewPassword == null!)
            {
                return false;
            }

            var r = EntryCurrentPassword.DataValid && EntryNewPassword.DataValid;
            if (View.CmdSaveHeader != null)
            {
                View.CmdSaveHeader.IsVisible = r;
            }

            return r;
        }
    }
}