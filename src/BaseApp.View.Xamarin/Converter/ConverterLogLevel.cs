// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Xamarin.Forms;

namespace BaseApp.View.Xamarin.Converter
{
    /// <summary>
    ///     <para>Einen Boolean in einen Wert umwandeln</para>
    ///     Klasse InvertedBooleanConverter. (C) 2018 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConverterLogLevel : IValueConverter
    {
        #region Interface Implementations

        /// <summary>
        ///     Boolean zu Value convertieren
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            if (value is LogLevel l)
            {
                switch (l)
                {
                    case LogLevel.Trace:
                        return Color.Gray;
                    case LogLevel.Debug:
                        return Color.Gray;
                    case LogLevel.Information:
                        return Color.DodgerBlue;
                    case LogLevel.Warning:
                        return Color.DarkOrange;
                    case LogLevel.Error:
                        return Color.Red;
                    case LogLevel.Critical:
                        return Color.Red;
                    case LogLevel.None:
                        return Color.DodgerBlue;
                    default:
                        throw new ArgumentOutOfRangeException($"[{nameof(ConverterLogLevel)}]({nameof(Convert)}): out of range {l}");
                }
            }

            return Color.DodgerBlue;
        }


        /// <summary>
        ///     Value zu Boolean convertieren
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"[{nameof(ConverterLogLevel)}]({nameof(ConvertBack)}): not supported");
        }

        #endregion
    }
}