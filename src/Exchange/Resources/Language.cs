// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Collections.Generic;
using System.Globalization;
using Biss.Apps.Interfaces;
using Biss.Apps.Model;

namespace Exchange.Resources
{
    /// <summary>
    ///     <para>Culture der Resource Files via Code setzen</para>
    ///     Klasse Language. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class Language : IAppLanguage
    {
        /// <summary>
        ///     Unterstütze Sprachen (erste Sprache ist die Default Sprache welche verwendet wird als Fallback)
        /// </summary>
        public static readonly IReadOnlyCollection<string> SupportedLanguages = new List<string> {"de", "en"};

        /// <summary>
        ///     Aktuelle Kultur
        /// </summary>
        public static CultureInfo CurrentCulture = CultureInfo.CurrentCulture;

        /// <summary>
        ///     Aktuelle Kultur des Gerätes
        /// </summary>
        public static CultureInfo CurrentDeviceCulture = null!;

        private static ExLanguageContent? _currentText;

        #region Properties

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public CultureInfo CurrentCultureInfo
        {
            get => CurrentCulture;
        }

        /// <summary>
        ///     Texte welche im Apps.Base verwendet werden
        /// </summary>
        public static ExLanguageContent GetText
        {
            get
            {
                if (_currentText == null)
                {
                    _currentText = new ExLanguageContent(
                        ResCommon.CmdBack,
                        ResCommon.MsgTitleNotSaved,
                        ResCommon.MsgNotSaved,
                        ResCommon.CmdOk,
                        ResCommon.CmdCancel,
                        ResCommon.CmdYes,
                        ResCommon.CmdNo
                    );
                }

                return _currentText;
            }
        }

        #endregion

        /// <summary>
        ///     Resource Files auf bestimmte Kultur setzen
        /// </summary>
        /// <param name="culture"></param>
        public static void SetLanguageStatic(CultureInfo culture)
        {
            CurrentCulture = culture;
            ResCommon.Culture = culture;
            ResViewEditUser.Culture = culture;
            ResViewEditUserPassword.Culture = culture;
            ResViewLogin.Culture = culture;
            ResViewMain.Culture = culture;
            ResViewSettings.Culture = culture;
            ResViewSettingsPush.Culture = culture;
            ResViewUser.Culture = culture;
            _currentText = null;
        }

        #region Interface Implementations

        /// <summary>
        ///     GetLanguageContent
        /// </summary>
        /// <returns></returns>
        public ExLanguageContent GetLanguageContent()
        {
            return GetText;
        }

        /// <summary>
        ///     Resource Files auf bestimmte Kultur setzen
        /// </summary>
        /// <param name="culture"></param>
        public void SetLanguage(CultureInfo culture)
        {
            CurrentDeviceCulture = culture;
            SetLanguageStatic(culture);
        }

        #endregion
    }
}