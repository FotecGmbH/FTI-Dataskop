// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Globalization;
using Biss.Dc.Core;
using Xamarin.Forms;

namespace BaseApp.View.Xamarin.Converter
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse ConverterConnectionDisconnected. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConverterConnectionDisconnected : IValueConverter
    {
        #region Interface Implementations

        /// <summary>
        ///     Konvertiert ein Objekt für XAML
        /// </summary>
        /// <param name="value">Wert zum konvertieren für das UI</param>
        /// <param name="targetType">Zieltyp des Werts</param>
        /// <param name="parameter">Zusätzlicher Parameter aus XAML</param>
        /// <param name="culture">Aktuelle Kultur</param>
        /// <returns>Konvertierter Wert oder null</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumDcConnectionState state)
            {
                switch (state)
                {
                    case EnumDcConnectionState.Connected:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Konvertiert ein Objekt von XAML
        /// </summary>
        /// <param name="value">Wert zum konvertieren für das Datenobjekt</param>
        /// <param name="targetType">Zieltyp des Werts</param>
        /// <param name="parameter">Zusätzlicher Parameter aus XAML</param>
        /// <param name="culture">Aktuelle Kultur</param>
        /// <returns>Konvertierter Wert oder UnsetValue</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null!;
        }

        #endregion
    }
}