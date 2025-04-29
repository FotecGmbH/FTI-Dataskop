// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using BDA.Common.Exchange.Model;
using Biss.Apps.Attributes;
using Biss.Apps.Connectivity.Dc;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.Model;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;
using Biss.ObjectEx;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using Plugin.Media.Abstractions;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>VmEditUser</para>
    ///     Klasse VmEditUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditUser")]
    public class VmEditUser : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditUser.DesignInstance}"
        /// </summary>
        public static VmEditUser DesignInstance = new VmEditUser();

        private ExFile? _newImage;
        private bool _takingPicture;

        /// <summary>
        ///     VmEditUser
        /// </summary>
        public VmEditUser() : base(ResViewEditUser.LblTitle, subTitle: ResViewEditUser.LblSubTitle)
        {
            SetViewProperties(true);

            EntryFirstName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUser.EntryTitleFirstName,
                ResViewEditUser.EntryPlaceholderFirstName,
                Dc.DcExUser.Data,
                nameof(Dc.DcExUser.Data.FirstName),
                VmEntryValidators.ValidateFuncStringEmpty,
                () => EntryLastName?.Focus(EnumVmEntrySetFocusMode.FocusAndSelect),
                50,
                false);

            EntryLastName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUser.EntryTitleLastName,
                ResViewEditUser.EntryPlaceholderLastName,
                Dc.DcExUser.Data,
                nameof(Dc.DcExUser.Data.LastName),
                VmEntryValidators.ValidateFuncStringEmpty,
                maxChar: 50,
                showTitle: false);

            Dc.DcExUser.DataChangedEvent += (sender, args) => View.CmdSaveHeader!.CanExecute();
            EntryLastName.ValidChanged += (sender, args) => View.CmdSaveHeader!.CanExecute();
            EntryFirstName.ValidChanged += (sender, args) => View.CmdSaveHeader!.CanExecute();
        }

        #region Properties

        /// <summary>
        ///     Vorname
        /// </summary>
        public VmEntry EntryFirstName { get; set; }

        /// <summary>
        ///     Nachname
        /// </summary>
        public VmEntry EntryLastName { get; set; }

        /// <summary>
        ///     Bild aufnehmen
        /// </summary>
        public VmCommand CmdTakePicture { get; set; } = null!;

        /// <summary>
        ///     Bild Löschen
        /// </summary>
        public VmCommand CmdDeletePicture { get; set; } = null!;

        /// <summary>
        ///     Aktuelles Bild
        /// </summary>
        public object CurrentImage { get; set; } = null!;

        #endregion

        /// <summary>
        ///     View wurde (wieder) ist aber noch nicht sichtabr.
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        /// <returns></returns>
        public override Task OnAppearing(IView view)
        {
            if (!_takingPicture)
            {
                CurrentImage = Dc.DcExUser.Data.UserImageLink;
                CheckSaveBehavior = new CheckSaveDcBehavior<ExUser>(Dc.DcExUser);
            }

            return base.OnAppearing(view);
        }

        /// <summary>View wurde inaktiv</summary>
        /// <returns></returns>
        public override Task OnDisappearing(IView view)
        {
            if (!_takingPicture)
            {
                if (CheckSaveBehavior == null)
                {
                    Dc.DcExUser.EndEdit();
                }
                else
                {
                    Dc.DcExUser.EndEdit(true);
                }
            }

            return base.OnDisappearing(view);
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            View.CmdSaveHeader = new VmCommand(ResViewEditUser.CmdSave, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                if (this.GetAllInstancesWithType<VmEntry>().Any(p => !p.DataValid))
                {
                    await MsgBox.Show("Speichern leider nicht möglich!", "Eingabefehler").ConfigureAwait(false);
                    return;
                }

                //Neues Bild?
                if (_newImage != null)
                {
                    var newImage = await Dc.TransferFile(_newImage.Name, new MemoryStream(_newImage.Bytes), "").ConfigureAwait(true);
                    if (newImage.StoreResult.DataOk && newImage.DbId.HasValue)
                    {
                        Dc.DcExUser.Data.UserImageDbId = newImage.DbId.Value;
                        Dc.DcExUser.Data.UserImageLink = newImage.FileLink;
                    }
                    else
                    {
                        Logging.Log.LogError($"[{nameof(VmEditUser)}]({nameof(View.CmdSaveHeader)}-Command): StoreImage result is false. Type: {newImage.StoreResult.ErrorType} Msg: {newImage.StoreResult.ServerExceptionText}");
                        await MsgBox.Show(ResViewEditUser.MsgSaveError, ResCommon.MsgTitleError).ConfigureAwait(true);
                        return;
                    }
                }

                var r = await Dc.DcExUser.StoreData().ConfigureAwait(true);
                if (!r.DataOk)
                {
                    Logging.Log.LogError(($"[{nameof(VmEditUser)}]({nameof(View.CmdSaveHeader)}-Command): StoreData result is false. Type: {r.ErrorType} Msg: {r.ServerExceptionText}"));
                    await MsgBox.Show(ResViewEditUser.MsgSaveError, ResCommon.MsgTitleError).ConfigureAwait(true);
                    return;
                }

                CheckSaveBehavior = null;
                ViewResult = true;

                await Nav.Back().ConfigureAwait(true);
            }, CanExecuteCmdSave, glyph: Glyphs.Floppy_disk);

            CmdTakePicture = new VmCommand(string.Empty, async () =>
            {
                var o = new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.MaxWidthHeight,
                    MaxWidthHeight = 512,
                    AllowCropping = true
                };

                _takingPicture = true;
                DcDoNotAutoDisconnect = true;
                _newImage = await Files.TakePhotoAsync(cameraOptions: o).ConfigureAwait(true);
                _takingPicture = false;
                DcDoNotAutoDisconnect = false;

                if (_newImage != null)
                {
                    CurrentImage = _newImage;
                    Dc.DcExUser.Data.UserImageDbId = -2;
                }
            }, glyph: Glyphs.Pencil_1);

            CmdDeletePicture = new VmCommand(string.Empty, () =>
            {
                Dc.DcExUser.Data.UserImageLink = string.Empty;
                Dc.DcExUser.Data.UserImageDbId = -3;
                CurrentImage = string.Empty;
            }, glyph: Glyphs.Bin_2);
        }


        /// <summary>
        ///     Kann speichern gedrückt werden
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCmdSave()
        {
            if (!IsLoaded)
            {
                return false;
            }


            if (CheckSaveBehavior == null!)
            {
                return false;
            }

            var r = CheckSaveBehavior.Check();

            if (View.CmdSaveHeader != null)
            {
                View.CmdSaveHeader.IsVisible = r;
            }

            return r;
        }
    }
}