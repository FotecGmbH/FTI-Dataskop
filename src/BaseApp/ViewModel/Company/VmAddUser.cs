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
using BaseApp.Helper;
using BDA.Common.Exchange.Enum;
using BDA.Common.Exchange.Model.ConfigApp;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.EMail;
using Exchange.Resources;

namespace BaseApp.ViewModel.Company
{
    /// <summary>
    ///     <para>VmAddUser</para>
    ///     Klasse VmAddUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddUser", true)]
    public class VmAddUser : VmEditDcListPoint<ExCompanyUser>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddUser.DesignInstance}"
        /// </summary>
        public static VmAddUser DesignInstance = new VmAddUser();

        private List<string> _currentUsers = new List<string>();


        /// <summary>
        ///     VmAddUser
        /// </summary>
        public VmAddUser() : base(ResViewAddUser.LblTitle, subTitle: ResViewAddUser.LblSubTitle)
        {
            SetViewProperties(true);

            PickerUserRight.AddKey(EnumUserRight.Read, "Daten lesen");
            PickerUserRight.AddKey(EnumUserRight.ReadWrite, "Daten bearbeiten");

            PickerUserRole.AddKey(EnumUserRole.Admin, "Firmenadministrator");
            PickerUserRole.AddKey(EnumUserRole.User, "Firmenbenutzer");
        }

        #region Properties

        /// <summary>
        ///     Rolle im Unternehmen
        /// </summary>
        public VmPicker<EnumUserRole> PickerUserRole { get; private set; } = new VmPicker<EnumUserRole>(nameof(PickerUserRole));

        /// <summary>
        ///     Rechte (nicht bei Firmenadmins)
        /// </summary>
        public VmPicker<EnumUserRight> PickerUserRight { get; private set; } = new VmPicker<EnumUserRight>(nameof(PickerUserRight));

        /// <summary>
        ///     Loginname
        /// </summary>
        public VmEntry EntryLoginName { get; private set; } = null!;

        #endregion

        /// <summary>
        ///     View wurde erzeugt und geladen - Aber noch nicht sichtbar (gerendert)
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            var r = base.OnActivated(args);

            _currentUsers = Dc.DcExCompanyUsers.Where(w => (w.Data.CompanyId == Data.CompanyId || w.Data.IsSuperadmin) && !string.IsNullOrEmpty(w.Data.UserLoginEmail)).Select(s => s.Data.UserLoginEmail).ToList();

            EntryLoginName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewAddUser.EntryTitleLoginName,
                ResViewAddUser.EntryPlaceholderLoginName,
                Data,
                nameof(ExCompanyUser.UserLoginEmail),
                ValidateLoginName,
                showTitle: false
            );

            PickerUserRole.SelectKey(Data.UserRole);
            PickerUserRight.SelectKey(Data.UserRight);

            PickerUserRole.SelectedItemChanged += (sender, eventArgs) =>
            {
                Data.UserRole = eventArgs.CurrentItem.Key;
                if (Data.UserRole == EnumUserRole.Admin)
                {
                    PickerUserRight.SelectKey(EnumUserRight.ReadWrite);
                }
            };
            PickerUserRight.SelectedItemChanged += (sender, eventArgs) => Data.UserRight = eventArgs.CurrentItem.Key;

            return r;
        }

        /// <summary>
        ///     Validierung für Loginname
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private (string hint, bool valid) ValidateLoginName(string arg)
        {
            if (Data.CanEditLoginEMail)
            {
                var r = VmEntryValidators.ValidateFuncStringEmpty(arg);
                if (!r.valid)
                {
                    return r;
                }

                if (!Validator.Check(arg))
                {
                    return (ResCommon.ValNoEmail, false);
                }

                if (_currentUsers.Any(a => string.Equals(a, arg, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return ("Benutzer bereits in Firma", false);
                }
            }

            return (string.Empty, true);
        }
    }
}